using System.Text.Json;
using System.Text.Json.Serialization;

namespace EatUp.Api.Domain;

public record Ingredient(
    string Key,
    string Name,
    string[] Aliases,
    string Category,
    Allergen[] Allergens,
    AnimalProduct Animal,
    Dictionary<StorageLocation, int> ShelfLifeDays,
    int? OpenedShelfLifeDays);

public record RecipeIngredient(string Key, bool Optional = false);

public record Recipe(
    string Key,
    string Title,
    string Description,
    int Minutes,
    RecipeIngredient[] Ingredients,
    string[] Steps);

/// <summary>Read-only ingredient and recipe catalogue, loaded from <c>Seed/catalog.json</c>.</summary>
public class Catalog
{
    private readonly Dictionary<string, Ingredient> _byKey;

    public Catalog(IReadOnlyList<Ingredient> ingredients, IReadOnlyList<Recipe> recipes)
    {
        Ingredients = ingredients;
        Recipes = recipes;
        _byKey = ingredients.ToDictionary(i => i.Key, StringComparer.OrdinalIgnoreCase);

        var unknown = recipes.SelectMany(r => r.Ingredients).Select(i => i.Key).Where(k => !_byKey.ContainsKey(k)).ToList();
        if (unknown.Count > 0)
            throw new InvalidOperationException($"Recipes reference unknown ingredients: {string.Join(", ", unknown.Distinct())}");
    }

    public IReadOnlyList<Ingredient> Ingredients { get; }
    public IReadOnlyList<Recipe> Recipes { get; }

    public Ingredient? Find(string? key) => key is not null && _byKey.TryGetValue(key, out var i) ? i : null;

    /// <summary>Best catalogue match for free text typed by the user, e.g. "Free-range eggs" → egg.</summary>
    public Ingredient? Match(string name)
    {
        var text = name.Trim().ToLowerInvariant();
        var exact = Ingredients.FirstOrDefault(i => i.Name.Equals(text, StringComparison.OrdinalIgnoreCase) || i.Aliases.Contains(text));
        if (exact is not null) return exact;

        // Longest alias found as whole words wins, so "coconut milk" beats "milk" and "peanut butter" isn't butter.
        var padded = $" {Words(text)} ";
        return Ingredients
            .SelectMany(i => i.Aliases.Select(a => (i, alias: Words(a))))
            .Where(x => x.alias.Length > 0 && padded.Contains($" {x.alias} "))
            .OrderByDescending(x => x.alias.Length)
            .Select(x => x.i)
            .FirstOrDefault();
    }

    private static string Words(string s) =>
        string.Join(' ', s.ToLowerInvariant().Split([' ', '-'], StringSplitOptions.RemoveEmptyEntries));

    public IEnumerable<Ingredient> Search(string? query) =>
        string.IsNullOrWhiteSpace(query)
            ? Ingredients
            : Ingredients.Where(i =>
                i.Name.Contains(query, StringComparison.OrdinalIgnoreCase) ||
                i.Aliases.Any(a => a.StartsWith(query, StringComparison.OrdinalIgnoreCase)));

    public static Catalog Load(string path)
    {
        var options = new JsonSerializerOptions(JsonSerializerDefaults.Web) { Converters = { new JsonStringEnumConverter() } };
        var file = JsonSerializer.Deserialize<CatalogFile>(File.ReadAllText(path), options)
            ?? throw new InvalidOperationException($"Could not read catalogue at {path}");
        return new Catalog(file.Ingredients, file.Recipes);
    }

    private record CatalogFile(List<Ingredient> Ingredients, List<Recipe> Recipes);
}
