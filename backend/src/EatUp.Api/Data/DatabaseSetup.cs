using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;

namespace EatUp.Api.Data;

public static class DatabaseSetup
{
    public static async Task PrepareAsync(EatUpDb db, ILogger logger)
    {
        if (await WasCreatedWithoutMigrationsAsync(db)) await ResetPreMigrationDatabaseAsync(db, logger);
        await db.Database.MigrateAsync();
    }

    private static async Task<bool> WasCreatedWithoutMigrationsAsync(EatUpDb db)
    {
        var creator = db.GetService<IRelationalDatabaseCreator>();
        if (!await creator.ExistsAsync()) return false;
        if (!await creator.HasTablesAsync()) return false;
        return !(await db.Database.GetAppliedMigrationsAsync()).Any();
    }

    // One-time switch from EnsureCreated: that test data can't be upgraded, and only the database file goes (vapid.json stays).
    private static async Task ResetPreMigrationDatabaseAsync(EatUpDb db, ILogger logger)
    {
        if (!await HasExactlyPreMigrationTablesAsync(db))
            throw new InvalidOperationException($"The database '{db.Database.GetDbConnection().DataSource}' has tables EatUp did not create. Refusing to delete it; point ConnectionStrings:EatUp at an EatUp database.");

        logger.LogWarning("The database was created before EF migrations were introduced. Deleting it once and recreating it from migrations; from now on startup only upgrades it.");
        await db.Database.EnsureDeletedAsync();
    }

    // Frozen at the switch to migrations: newer tables never existed in an EnsureCreated database.
    private static readonly HashSet<string> PreMigrationTables = ["Items", "Profiles", "PushSubscriptions", "Shopping"];

    private static async Task<bool> HasExactlyPreMigrationTablesAsync(EatUpDb db)
    {
        var databaseTables = await db.Database
            .SqlQueryRaw<string>("SELECT name AS Value FROM sqlite_master WHERE type = 'table' AND name NOT LIKE 'sqlite_%'")
            .ToListAsync();
        return PreMigrationTables.SetEquals(databaseTables);
    }
}
