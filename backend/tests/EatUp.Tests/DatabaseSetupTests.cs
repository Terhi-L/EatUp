using EatUp.Api.Data;
using EatUp.Api.Domain;
using Microsoft.EntityFrameworkCore;
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

    public void Dispose() => Directory.Delete(_dataDir, recursive: true);
}
