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
            // EnsureCreated builds today's model; databases from before migrations had no ConsumeEvents table.
            await legacy.Database.ExecuteSqlRawAsync("DROP TABLE ConsumeEvents");
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

    public void Dispose() => Directory.Delete(_dataDir, recursive: true);
}
