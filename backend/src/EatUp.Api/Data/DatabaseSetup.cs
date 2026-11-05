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
        logger.LogWarning("The database was created before EF migrations were introduced. Deleting it once and recreating it from migrations; from now on startup only upgrades it.");
        await db.Database.EnsureDeletedAsync();
    }
}
