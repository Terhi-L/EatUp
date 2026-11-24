using EatUp.Api.Domain;

namespace EatUp.Tests;

public class MealPlanningTests
{
    private static readonly DateOnly Monday = new(2026, 11, 23);
    private static readonly Catalog Catalog = TestCatalog.Default;

    [Theory]
    [InlineData("2026-11-23", "2026-11-23")] // Monday is its own week start
    [InlineData("2026-11-24", "2026-11-23")]
    [InlineData("2026-11-29", "2026-11-23")] // Sunday ends the week, it doesn't start the next one
    [InlineData("2026-11-30", "2026-11-30")]
    [InlineData("2027-01-03", "2026-12-28")] // across a year boundary
    public void A_week_starts_on_the_Monday_on_or_before_the_date(string date, string expected)
    {
        Assert.Equal(DateOnly.Parse(expected), MealPlanning.WeekStart(DateOnly.Parse(date)));
    }

    [Fact]
    public void A_recipe_meal_is_valid()
    {
        Assert.Empty(Validate(Choice(recipeKey: "omelette")));
    }

    [Fact]
    public void A_free_text_meal_is_valid()
    {
        Assert.Empty(Validate(Choice(freeText: "Pizza at Mari's")));
    }

    [Fact]
    public void A_meal_with_both_a_recipe_and_free_text_is_rejected()
    {
        Assert.Contains("meal", Validate(Choice(recipeKey: "omelette", freeText: "and toast")).Keys);
    }

    [Theory]
    [InlineData(null, null)]
    [InlineData("", "   ")]
    public void A_meal_with_neither_a_recipe_nor_free_text_is_rejected(string? recipeKey, string? freeText)
    {
        Assert.Contains("meal", Validate(Choice(recipeKey: recipeKey, freeText: freeText)).Keys);
    }

    [Fact]
    public void An_unknown_recipe_is_rejected()
    {
        Assert.Contains("recipeKey", Validate(Choice(recipeKey: "unicorn-stew")).Keys);
    }

    [Fact]
    public void Free_text_longer_than_the_limit_is_rejected()
    {
        Assert.Empty(Validate(Choice(freeText: new string('a', MealPlanning.MaxFreeTextLength))));
        Assert.Contains("freeText", Validate(Choice(freeText: new string('a', MealPlanning.MaxFreeTextLength + 1))).Keys);
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(7)]
    public void A_date_outside_the_week_is_rejected(int daysFromMonday)
    {
        Assert.Contains("date", Validate(Choice(recipeKey: "omelette", date: Monday.AddDays(daysFromMonday))).Keys);
    }

    [Fact]
    public void An_unknown_slot_is_rejected()
    {
        Assert.Contains("slot", Validate(Choice(recipeKey: "omelette", slot: (MealSlot)42)).Keys);
    }

    [Fact]
    public void Normalizing_trims_free_text_and_uses_the_catalogue_spelling_of_the_recipe_key()
    {
        var normalized = MealPlanning.Normalize(Choice(recipeKey: "OMELETTE", freeText: "  "), Catalog);

        Assert.Equal("omelette", normalized.RecipeKey);
        Assert.Null(normalized.FreeText);
    }

    private static MealChoice Choice(string? recipeKey = null, string? freeText = null, DateOnly? date = null, MealSlot slot = MealSlot.Dinner) =>
        new(date ?? Monday.AddDays(2), slot, recipeKey, freeText);

    private static IReadOnlyDictionary<string, string[]> Validate(MealChoice choice) =>
        MealPlanning.Validate(MealPlanning.Normalize(choice, Catalog), Monday, Catalog);
}
