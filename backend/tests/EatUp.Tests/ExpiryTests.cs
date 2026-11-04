using EatUp.Api.Domain;

namespace EatUp.Tests;

public class ExpiryTests
{
    private static readonly DateOnly Today = new(2026, 11, 2);

    [Theory]
    [InlineData(-1, Urgency.Expired)]
    [InlineData(0, Urgency.Today)]
    [InlineData(1, Urgency.Soon)]
    [InlineData(2, Urgency.Soon)]
    [InlineData(3, Urgency.Later)]
    public void Urgency_is_based_on_days_left(int daysLeft, Urgency expected) =>
        Assert.Equal(expected, Expiry.UrgencyFor(daysLeft));

    [Fact]
    public void Opened_item_expires_at_opened_shelf_life_when_sooner_than_printed_date()
    {
        var milk = new Ingredient("milk", "Milk", [], "Dairy", [], AnimalProduct.Dairy, [], OpenedShelfLifeDays: 4);
        var item = new FoodItem { Name = "Milk", ExpiryDate = Today.AddDays(10), OpenedDate = Today.AddDays(-1) };

        Assert.Equal(Today.AddDays(3), Expiry.EffectiveExpiry(item, milk));
    }

    [Fact]
    public void Opened_item_keeps_printed_date_when_it_is_sooner()
    {
        var milk = new Ingredient("milk", "Milk", [], "Dairy", [], AnimalProduct.Dairy, [], OpenedShelfLifeDays: 4);
        var item = new FoodItem { Name = "Milk", ExpiryDate = Today.AddDays(1), OpenedDate = Today };

        Assert.Equal(Today.AddDays(1), Expiry.EffectiveExpiry(item, milk));
    }

    [Fact]
    public void Local_today_uses_the_users_time_zone()
    {
        // 23:30 UTC on 1 Nov is already 2 Nov in Oslo.
        var utc = new DateTimeOffset(2026, 11, 1, 23, 30, 0, TimeSpan.Zero);

        Assert.Equal(new DateOnly(2026, 11, 2), Expiry.LocalToday(utc, "Europe/Oslo"));
        Assert.Equal(new DateOnly(2026, 11, 1), Expiry.LocalToday(utc, "UTC"));
    }

    [Fact]
    public void Unknown_time_zone_falls_back_to_utc() =>
        Assert.Equal(TimeZoneInfo.Utc, Expiry.ResolveTimeZone("Not/AZone"));

    [Fact]
    public void Default_expiry_comes_from_shelf_life_for_the_storage_location()
    {
        var egg = TestCatalog.Ing("egg");

        Assert.Equal(Today.AddDays(7), Expiry.DefaultExpiry(egg, StorageLocation.Fridge, Today));
        Assert.Null(Expiry.DefaultExpiry(egg, StorageLocation.Cupboard, Today));
    }
}
