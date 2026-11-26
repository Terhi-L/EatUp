using System.Text.Json;
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

    private static readonly Ingredient Yogurt = TestCatalog.Ing("yogurt", AnimalProduct.Dairy) with
    {
        ExpiryKind = ExpiryKind.BestBefore,
        CheckDaysPastBestBefore = new() { [StorageLocation.Fridge] = 3 },
        OpenedShelfLifeDays = 4,
    };

    private static readonly Ingredient Chicken = TestCatalog.Ing("chicken", AnimalProduct.Meat);

    private static FoodItem Item(Ingredient? ingredient, int daysLeft, ExpiryKind? kind = null, StorageLocation location = StorageLocation.Fridge) => new()
    {
        Name = ingredient?.Name ?? "Kimchi",
        IngredientKey = ingredient?.Key,
        Location = location,
        ExpiryDate = Today.AddDays(daysLeft),
        ExpiryKind = kind ?? Expiry.DefaultKind(ingredient),
    };

    private static (int DaysLeft, Urgency Urgency, ExpiryAdvice Advice) FreshnessOf(FoodItem item, Ingredient? ingredient)
    {
        var freshness = Expiry.FreshnessOf(item, ingredient, Today);
        return (freshness.DaysLeft, freshness.Urgency, freshness.Advice);
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(-3)]
    public void Best_before_food_inside_its_window_is_to_eat_today_after_a_look_smell_and_taste(int daysLeft) =>
        Assert.Equal((daysLeft, Urgency.Today, ExpiryAdvice.LookSmellTaste), FreshnessOf(Item(Yogurt, daysLeft), Yogurt));

    [Fact]
    public void Best_before_food_past_its_window_is_to_throw_away() =>
        Assert.Equal((-4, Urgency.Expired, ExpiryAdvice.None), FreshnessOf(Item(Yogurt, -4), Yogurt));

    [Fact]
    public void Use_by_food_past_its_date_is_to_throw_away() =>
        Assert.Equal((-1, Urgency.Expired, ExpiryAdvice.None), FreshnessOf(Item(Chicken, -1), Chicken));

    // Cooked rice in the fridge is the case this protects: dry rice is best before, but only in the cupboard.
    [Fact]
    public void Best_before_food_kept_where_it_has_no_window_is_to_throw_away_once_past_its_date() =>
        Assert.Equal((-1, Urgency.Expired, ExpiryAdvice.None), FreshnessOf(Item(Yogurt, -1, location: StorageLocation.Counter), Yogurt));

    [Fact]
    public void Opened_food_past_its_opened_shelf_life_gets_no_extra_days()
    {
        var opened = Item(Yogurt, 10);
        opened.OpenedDate = Today.AddDays(-5);

        Assert.Equal((-1, Urgency.Expired, ExpiryAdvice.None), FreshnessOf(opened, Yogurt));
    }

    // Yogurt here: 3 days worth checking in the fridge, and 4 days once opened.
    [Theory]
    [InlineData(-1, -5, Urgency.Expired)] // opened limit on the printed date: no extra days
    [InlineData(-2, -4, Urgency.Today)] // opened limit two days after the printed date
    [InlineData(-3, -5, Urgency.Expired)]
    [InlineData(-3, -3, Urgency.Today)] // opened on the printed date: the window ends before the opened limit
    [InlineData(-4, -4, Urgency.Expired)]
    public void Opened_food_is_worth_checking_only_until_its_opened_limit(int printedDaysLeft, int openedDaysAgo, Urgency expected)
    {
        var opened = Item(Yogurt, printedDaysLeft);
        opened.OpenedDate = Today.AddDays(openedDaysAgo);

        Assert.Equal(expected, FreshnessOf(opened, Yogurt).Urgency);
    }

    [Fact]
    public void Food_nobody_picked_a_kind_for_takes_the_catalogues_or_counts_as_use_by()
    {
        Assert.Equal(ExpiryKind.BestBefore, Expiry.DefaultKind(Yogurt));
        Assert.Equal(ExpiryKind.UseBy, Expiry.DefaultKind(Chicken));
        Assert.Equal(ExpiryKind.UseBy, Expiry.DefaultKind(null));
    }

    [Fact]
    public void Unknown_food_counts_as_use_by_and_asks_to_check_the_package_before_throwing_it_out() =>
        Assert.Equal((-1, Urgency.Expired, ExpiryAdvice.CheckPackage), FreshnessOf(Item(null, -1), null));

    [Fact]
    public void Unknown_food_switched_to_best_before_is_worth_checking_for_the_fallback_days()
    {
        var lastDay = -Expiry.FallbackCheckDays;

        Assert.Equal((lastDay, Urgency.Today, ExpiryAdvice.LookSmellTaste), FreshnessOf(Item(null, lastDay, ExpiryKind.BestBefore), null));
        Assert.Equal((lastDay - 1, Urgency.Expired, ExpiryAdvice.None), FreshnessOf(Item(null, lastDay - 1, ExpiryKind.BestBefore), null));
    }

    [Fact]
    public void Use_by_food_switched_to_best_before_is_worth_checking_for_the_fallback_days() =>
        Assert.Equal((-Expiry.FallbackCheckDays, Urgency.Today, ExpiryAdvice.LookSmellTaste),
            FreshnessOf(Item(Chicken, -Expiry.FallbackCheckDays, ExpiryKind.BestBefore), Chicken));

    [Fact]
    public void Food_not_past_its_date_needs_no_advice()
    {
        Assert.Equal((0, Urgency.Today, ExpiryAdvice.None), FreshnessOf(Item(null, 0), null));
        Assert.Equal((1, Urgency.Soon, ExpiryAdvice.None), FreshnessOf(Item(Yogurt, 1), Yogurt));
    }

    [Fact]
    public void Catalogue_rejects_best_before_food_without_days_worth_checking()
    {
        var noWindow = TestCatalog.Ing("milk") with { ExpiryKind = ExpiryKind.BestBefore };
        var zeroDays = TestCatalog.Ing("cream") with { ExpiryKind = ExpiryKind.BestBefore, CheckDaysPastBestBefore = new() { [StorageLocation.Fridge] = 0 } };

        var error = Assert.Throws<InvalidOperationException>(() => new Catalog([noWindow, zeroDays], []));
        Assert.Contains("milk, cream", error.Message);
    }

    [Fact]
    public void Catalogue_rejects_use_by_food_with_days_worth_checking()
    {
        var chicken = Chicken with { CheckDaysPastBestBefore = new() { [StorageLocation.Fridge] = 1 } };

        var error = Assert.Throws<InvalidOperationException>(() => new Catalog([chicken], []));
        Assert.Contains("chicken", error.Message);
    }

    [Fact]
    public void Catalogue_food_without_an_expiry_kind_is_rejected()
    {
        var directory = Directory.CreateTempSubdirectory("eatup-catalog-");
        var path = Path.Combine(directory.FullName, "catalog.json");
        File.WriteAllText(path, """
            { "ingredients": [{ "key": "egg", "name": "Egg", "aliases": [], "category": "Eggs", "allergens": [], "animal": "Egg",
                                "shelfLifeDays": { "Fridge": 28 }, "openedShelfLifeDays": null }],
              "recipes": [] }
            """);

        var error = Assert.Throws<JsonException>(() => Catalog.Load(path));
        Assert.Contains("expiryKind", error.Message);
    }

    private static readonly Catalog Real = Catalog.Load(Path.Combine(AppContext.BaseDirectory, "Seed", "catalog.json"));

    [Fact]
    public void Seed_meat_and_fish_are_use_by() =>
        Assert.All(Real.Ingredients.Where(i => i.Animal is AnimalProduct.Meat or AnimalProduct.Fish),
            i => Assert.Equal(ExpiryKind.UseBy, i.ExpiryKind));

    [Fact]
    public void Seed_rice_is_never_worth_checking_past_its_date_in_the_fridge_where_it_is_cooked() =>
        Assert.False(Real.Find("rice")!.CheckDaysPastBestBefore!.ContainsKey(StorageLocation.Fridge));
}
