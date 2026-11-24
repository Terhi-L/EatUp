using System.Text.Json.Serialization;

namespace EatUp.Api.Domain;

/// <summary>What the user wants to eat in one slot: a catalogue recipe or free text, never both.</summary>
/// <remarks>Date and slot are required: left out, they would quietly become 0001-01-01 and breakfast.</remarks>
public record MealChoice(
    [property: JsonRequired] DateOnly Date,
    [property: JsonRequired] MealSlot Slot,
    string? RecipeKey,
    string? FreeText);

public static class MealPlanning
{
    public const int MaxFreeTextLength = 200;

    public static DateOnly WeekStart(DateOnly date) => date.AddDays(-DaysSinceMonday(date.DayOfWeek));

    /// <summary>Blank text counts as missing, and a known recipe key takes the catalogue's spelling.</summary>
    public static MealChoice Normalize(MealChoice choice, Catalog catalog) => choice with
    {
        RecipeKey = catalog.FindRecipe(choice.RecipeKey)?.Key ?? NullIfBlank(choice.RecipeKey),
        FreeText = NullIfBlank(choice.FreeText?.Trim()),
    };

    /// <summary>Errors by field name; empty when the choice can be stored in the week starting <paramref name="weekStart"/>.</summary>
    public static IReadOnlyDictionary<string, string[]> Validate(MealChoice choice, DateOnly weekStart, Catalog catalog)
    {
        var errors = new Dictionary<string, string[]>();
        if (!Enum.IsDefined(choice.Slot)) errors["slot"] = ["Unknown slot."];
        if (WeekStart(choice.Date) != weekStart) errors["date"] = [$"Date must be in the week starting {weekStart:yyyy-MM-dd}."];
        if (MealError(choice, catalog) is { } meal) errors[meal.Field] = [meal.Message];
        return errors;
    }

    private static (string Field, string Message)? MealError(MealChoice choice, Catalog catalog) => choice switch
    {
        { RecipeKey: not null, FreeText: not null } => ("meal", "Choose a recipe or free text, not both."),
        { RecipeKey: null, FreeText: null } => ("meal", "Choose a recipe or write what you'll eat."),
        { RecipeKey: { } key } when catalog.FindRecipe(key) is null => ("recipeKey", "Unknown recipe."),
        { FreeText.Length: > MaxFreeTextLength } => ("freeText", $"Free text can be at most {MaxFreeTextLength} characters."),
        _ => null,
    };

    private static int DaysSinceMonday(DayOfWeek day) => ((int)day + 6) % 7;

    private static string? NullIfBlank(string? text) => string.IsNullOrWhiteSpace(text) ? null : text;
}
