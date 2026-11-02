using EatUp.Api.Domain;
using Microsoft.EntityFrameworkCore;

namespace EatUp.Api.Data;

/// <summary>Creates the single profile and a fridge full of demo items, with dates relative to today.</summary>
public static class DemoData
{
    public static async Task EnsureSeededAsync(EatUpDb db, TimeProvider time)
    {
        if (await db.Profiles.AnyAsync()) return;
        db.Profiles.Add(new UserProfile { TimeZone = TimeZoneInfo.Local.Id });
        await db.SaveChangesAsync();
        await ResetItemsAsync(db, time);
    }

    public static async Task ResetItemsAsync(EatUpDb db, TimeProvider time)
    {
        await db.Items.ExecuteDeleteAsync();

        var profile = await db.Profiles.SingleAsync();
        profile.LastNotifiedOn = null;

        var now = time.GetUtcNow();
        var today = Expiry.LocalToday(now, profile.TimeZone);
        FoodItem Item(string name, string key, double qty, string unit, StorageLocation location, int days, int? openedDaysAgo = null) => new()
        {
            Name = name, IngredientKey = key, Quantity = qty, Unit = unit, Location = location,
            ExpiryDate = today.AddDays(days),
            OpenedDate = openedDaysAgo is { } d ? today.AddDays(-d) : null,
            CreatedAt = now,
        };

        db.Items.AddRange(
            Item("Avocado", "avocado", 2, "pcs", StorageLocation.Counter, 0),
            Item("Eggs", "egg", 6, "pcs", StorageLocation.Fridge, 1),
            Item("Milk", "milk", 1, "l", StorageLocation.Fridge, 6, openedDaysAgo: 2),
            Item("Baby spinach", "spinach", 1, "bag", StorageLocation.Fridge, 2),
            Item("Tomatoes", "tomato", 4, "pcs", StorageLocation.Counter, 3),
            Item("Yogurt", "yogurt", 1, "tub", StorageLocation.Fridge, -1),
            Item("Cream cheese", "cream-cheese", 1, "tub", StorageLocation.Fridge, 10),
            Item("Cheddar", "cheese", 200, "g", StorageLocation.Fridge, 20),
            Item("Spaghetti", "pasta", 500, "g", StorageLocation.Pantry, 200),
            Item("Onions", "onion", 3, "pcs", StorageLocation.Pantry, 25),
            Item("Garlic", "garlic", 1, "bulb", StorageLocation.Pantry, 25),
            Item("Bread", "bread", 1, "loaf", StorageLocation.Counter, 2),
            Item("Chickpeas", "chickpeas", 1, "can", StorageLocation.Pantry, 300));

        await db.SaveChangesAsync();
    }
}
