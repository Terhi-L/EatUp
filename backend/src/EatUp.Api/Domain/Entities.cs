namespace EatUp.Api.Domain;

public class FoodItem
{
    public int Id { get; set; }
    public required string Name { get; set; }
    /// <summary>Key into the ingredient catalogue, when the item matches a known ingredient.</summary>
    public string? IngredientKey { get; set; }
    public double Quantity { get; set; } = 1;
    public string Unit { get; set; } = "pcs";
    public StorageLocation Location { get; set; }
    public DateOnly ExpiryDate { get; set; }
    public DateOnly? OpenedDate { get; set; }
    public ItemStatus Status { get; set; } = ItemStatus.Active;
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? StatusChangedAt { get; set; }
}

/// <summary>Single-user MVP: there is exactly one profile row (Id = 1).</summary>
public class UserProfile
{
    public const int SingletonId = 1;

    public int Id { get; set; } = SingletonId;
    public DietType Diet { get; set; } = DietType.Omnivore;
    public List<Allergen> Allergens { get; set; } = [];
    public TimeOnly NotificationTime { get; set; } = new(9, 0);
    /// <summary>IANA time zone id reported by the browser.</summary>
    public string TimeZone { get; set; } = "UTC";
    public bool NotificationsEnabled { get; set; } = true;
    /// <summary>Local date (in <see cref="TimeZone"/>) of the last daily notification.</summary>
    public DateOnly? LastNotifiedOn { get; set; }
}

public class ShoppingItem
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public bool Done { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
}

public class PushSubscriptionRecord
{
    public int Id { get; set; }
    public required string Endpoint { get; set; }
    public required string P256dh { get; set; }
    public required string Auth { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
}
