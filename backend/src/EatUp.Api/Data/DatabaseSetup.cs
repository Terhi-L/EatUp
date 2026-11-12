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
        if (!await HasOnlyEatUpTablesAsync(db))
            throw new InvalidOperationException($"The database '{db.Database.GetDbConnection().DataSource}' has tables EatUp did not create. Refusing to delete it; point ConnectionStrings:EatUp at an EatUp database.");

        logger.LogWarning("The database was created before EF migrations were introduced. Deleting it once and recreating it from migrations; from now on startup only upgrades it.");
        await db.Database.EnsureDeletedAsync();
    }

    // Tables added after the switch to migrations are missing from those databases, so a subset still counts as EatUp's.
    private static async Task<bool> HasOnlyEatUpTablesAsync(EatUpDb db)
    {
        var modelTables = db.Model.GetEntityTypes().Select(entity => entity.GetTableName()).OfType<string>().ToHashSet();
        var databaseTables = await db.Database
            .SqlQueryRaw<string>("SELECT name AS Value FROM sqlite_master WHERE type = 'table' AND name NOT LIKE 'sqlite_%'")
            .ToListAsync();
        return modelTables.IsSupersetOf(databaseTables);
    }
}
