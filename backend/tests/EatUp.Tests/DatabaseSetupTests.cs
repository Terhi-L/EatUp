using EatUp.Api.Data;
using EatUp.Api.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.Extensions.Logging.Abstractions;

namespace EatUp.Tests;

public sealed class DatabaseSetupTests : IDisposable
{
    private readonly string _dataDir = Directory.CreateTempSubdirectory("eatup-db-").FullName;

    private string DbPath => Path.Combine(_dataDir, "eatup.db");

    private EatUpDb OpenDb() =>
        new(new DbContextOptionsBuilder<EatUpDb>().UseSqlite($"Data Source={DbPath};Pooling=False").Options);

    private static FoodItem Milk() => new()
    {
        Name = "Milk", Quantity = 1, Unit = "pcs", Location = StorageLocation.Fridge,
        ExpiryDate = new DateOnly(2026, 11, 10),
    };

    [Fact]
    public void Migrations_match_the_model()
    {
        using var db = OpenDb();

        Assert.False(db.Database.HasPendingModelChanges());
    }

    [Fact]
    public async Task A_new_database_is_created_from_migrations()
    {
        await using (var db = OpenDb()) await DatabaseSetup.PrepareAsync(db, NullLogger.Instance);

        await using var reopened = OpenDb();
        Assert.NotEmpty(await reopened.Database.GetAppliedMigrationsAsync());
        Assert.Empty(await reopened.Database.GetPendingMigrationsAsync());
    }

    [Fact]
    public async Task A_pre_migration_database_is_reset_once_and_keeps_the_vapid_keys()
    {
        var vapidPath = Path.Combine(_dataDir, "vapid.json");
        await File.WriteAllTextAsync(vapidPath, "{}");
        await using (var legacy = OpenDb())
        {
            await legacy.Database.EnsureCreatedAsync();
            legacy.Items.Add(Milk());
            await legacy.SaveChangesAsync();
            // EnsureCreated builds today's model; databases from before migrations had only these tables.
            await DropTablesExceptAsync(legacy, "Items", "Profiles", "PushSubscriptions", "Shopping");
        }

        await using (var db = OpenDb()) await DatabaseSetup.PrepareAsync(db, NullLogger.Instance);

        await using var reopened = OpenDb();
        Assert.Empty(reopened.Items);
        Assert.NotEmpty(await reopened.Database.GetAppliedMigrationsAsync());
        Assert.True(File.Exists(vapidPath));
    }

    [Fact]
    public async Task An_unrelated_database_is_never_deleted()
    {
        await using (var foreign = OpenDb())
        {
            await foreign.Database.ExecuteSqlRawAsync("CREATE TABLE Notes (Text TEXT); INSERT INTO Notes VALUES ('keep me');");
        }

        await using (var db = OpenDb())
        {
            await Assert.ThrowsAsync<InvalidOperationException>(() => DatabaseSetup.PrepareAsync(db, NullLogger.Instance));
        }

        await using var reopened = OpenDb();
        var notes = await reopened.Database.SqlQueryRaw<string>("SELECT Text AS Value FROM Notes").ToListAsync();
        Assert.Equal(["keep me"], notes);
    }

    [Fact]
    public async Task A_foreign_database_that_shares_a_table_name_is_never_deleted()
    {
        await using (var foreign = OpenDb())
        {
            await foreign.Database.ExecuteSqlRawAsync("CREATE TABLE Items (Text TEXT); INSERT INTO Items VALUES ('keep me');");
        }

        await using (var db = OpenDb())
        {
            await Assert.ThrowsAsync<InvalidOperationException>(() => DatabaseSetup.PrepareAsync(db, NullLogger.Instance));
        }

        await using var reopened = OpenDb();
        var rows = await reopened.Database.SqlQueryRaw<string>("SELECT Text AS Value FROM Items").ToListAsync();
        Assert.Equal(["keep me"], rows);
    }

    [Fact]
    public async Task A_migrated_database_keeps_its_data_on_restart()
    {
        await using (var db = OpenDb())
        {
            await DatabaseSetup.PrepareAsync(db, NullLogger.Instance);
            db.Items.Add(Milk());
            await db.SaveChangesAsync();
        }

        await using (var db = OpenDb()) await DatabaseSetup.PrepareAsync(db, NullLogger.Instance);

        await using var reopened = OpenDb();
        Assert.Single(reopened.Items);
    }

    [Fact]
    public async Task Upgrading_records_one_consume_event_per_finished_item()
    {
        var discardedAt = new DateTimeOffset(2026, 11, 4, 12, 0, 0, TimeSpan.Zero);
        await using (var db = OpenDb())
        {
            await db.GetService<IMigrator>().MigrateAsync("20261105153237_InitialCreate");
            await db.Database.ExecuteSqlAsync($"""
                INSERT INTO Items (Name, IngredientKey, Quantity, Unit, Location, ExpiryDate, Status, CreatedAt, StatusChangedAt)
                VALUES ('Milk', 'milk', 1, 'l', 'Fridge', '2026-11-10', 'Active', '2026-11-01', NULL),
                       ('Bread', 'bread', 2, 'pcs', 'Counter', '2026-11-05', 'Discarded', '2026-11-01', {discardedAt})
                """);
        }

        await using (var db = OpenDb()) await DatabaseSetup.PrepareAsync(db, NullLogger.Instance);

        await using var reopened = OpenDb();
        var bread = Assert.Single(reopened.ConsumeEvents);
        Assert.Equal(("Bread", 2.0, "pcs", ConsumeOutcome.Discarded, discardedAt),
            (bread.ItemName, bread.Amount, bread.Unit, bread.Outcome, bread.ConsumedAt));
        Assert.NotNull(bread.FoodItemId);
    }

    [Fact]
    public async Task Upgrading_keeps_consume_history_and_lets_it_outlive_its_item()
    {
        var consumedAt = new DateTimeOffset(2026, 11, 20, 18, 0, 0, TimeSpan.Zero);
        await using (var db = OpenDb())
        {
            await db.GetService<IMigrator>().MigrateAsync("20261124100549_AddMealPlans");
            var milkId = await InsertOldItemAsync(db, "Milk", "milk");
            db.ConsumeEvents.AddRange(
                new ConsumeEvent { FoodItemId = milkId, ItemName = "Milk", Amount = 0.25, Unit = "l", Outcome = ConsumeOutcome.Used, ConsumedAt = consumedAt },
                new ConsumeEvent { FoodItemId = milkId, ItemName = "Milk", Amount = 0.25, Unit = "l", Outcome = ConsumeOutcome.Used, ConsumedAt = consumedAt });
            await db.SaveChangesAsync();
        }

        await using (var db = OpenDb()) await DatabaseSetup.PrepareAsync(db, NullLogger.Instance);
        await using (var db = OpenDb()) await db.Items.ExecuteDeleteAsync();

        await using var reopened = OpenDb();
        var events = await reopened.ConsumeEvents.ToListAsync();
        Assert.Equal(2, events.Count);
        Assert.All(events, e => Assert.Equal((consumedAt, true), (e.ConsumedAt, e.FoodItemId is not null)));
    }

    [Fact]
    public async Task Upgrading_gives_every_item_its_catalogue_expiry_kind_and_keeps_its_data()
    {
        var catalog = Catalog.Load(Path.Combine(AppContext.BaseDirectory, "Seed", "catalog.json"));
        await using (var db = OpenDb())
        {
            await db.GetService<IMigrator>().MigrateAsync("20261124120724_KeepConsumeEventItemIdsAndIndexTime");
            foreach (var key in catalog.Ingredients.Select(i => i.Key)) await InsertOldItemAsync(db, key, key);
            await InsertOldItemAsync(db, "Kimchi", null);
        }

        await using (var db = OpenDb()) await DatabaseSetup.PrepareAsync(db, NullLogger.Instance);

        await using var reopened = OpenDb();
        var items = await reopened.Items.ToListAsync();
        Assert.Equal(catalog.Ingredients.Count + 1, items.Count);
        Assert.All(items, i => Assert.Equal(catalog.Find(i.IngredientKey)?.ExpiryKind ?? ExpiryKind.UseBy, i.ExpiryKind));
        Assert.All(items, i => Assert.Equal((2.0, new DateOnly(2026, 11, 10)), (i.Quantity, i.ExpiryDate)));
    }

    [Fact]
    public async Task Rolling_back_clears_only_item_ids_whose_item_is_gone()
    {
        int keptId, deletedId;
        await using (var db = OpenDb())
        {
            await DatabaseSetup.PrepareAsync(db, NullLogger.Instance);
            db.Items.AddRange(Milk(), Milk());
            await db.SaveChangesAsync();
            (keptId, deletedId) = (db.Items.Min(i => i.Id), db.Items.Max(i => i.Id));
            db.ConsumeEvents.AddRange(UsedMilk(keptId), UsedMilk(deletedId));
            await db.SaveChangesAsync();
            await db.Items.Where(i => i.Id == deletedId).ExecuteDeleteAsync();
        }

        await using (var db = OpenDb()) await db.GetService<IMigrator>().MigrateAsync("20261124100549_AddMealPlans");

        await using var reopened = OpenDb();
        var itemIds = await reopened.ConsumeEvents.OrderBy(e => e.Id).Select(e => e.FoodItemId).ToListAsync();
        Assert.Equal([keptId, null], itemIds);
        var brokenLinks = await reopened.Database.SqlQueryRaw<int>("SELECT COUNT(*) AS Value FROM pragma_foreign_key_check('ConsumeEvents')").SingleAsync();
        Assert.Equal(0, brokenLinks);
    }

    private static ConsumeEvent UsedMilk(int itemId) => new()
    {
        FoodItemId = itemId, ItemName = "Milk", Amount = 0.5, Unit = "l", Outcome = ConsumeOutcome.Used,
        ConsumedAt = new DateTimeOffset(2026, 11, 20, 18, 0, 0, TimeSpan.Zero),
    };

    // SQL, not EF: an older schema lacks columns today's model has.
    private static async Task<int> InsertOldItemAsync(EatUpDb db, string name, string? ingredientKey)
    {
        await db.Database.ExecuteSqlAsync($"""
            INSERT INTO Items (Name, IngredientKey, Quantity, Unit, Location, ExpiryDate, Status, CreatedAt)
            VALUES ({name}, {ingredientKey}, 2, 'pcs', 'Fridge', '2026-11-10', 'Active', '2026-11-01')
            """);
        return await db.Database.SqlQueryRaw<int>("SELECT MAX(Id) AS Value FROM Items").SingleAsync();
    }

    private static async Task DropTablesExceptAsync(EatUpDb db, params string[] keep)
    {
        var tables = await db.Database
            .SqlQueryRaw<string>("SELECT name AS Value FROM sqlite_master WHERE type = 'table' AND name NOT LIKE 'sqlite_%'")
            .ToListAsync();
#pragma warning disable EF1003 // Table names come from sqlite_master, and identifiers can't be SQL parameters.
        foreach (var table in tables.Except(keep)) await db.Database.ExecuteSqlRawAsync("DROP TABLE \"" + table + "\"");
#pragma warning restore EF1003
    }

    public void Dispose() => Directory.Delete(_dataDir, recursive: true);
}
