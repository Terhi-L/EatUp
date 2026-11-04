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
    int? OpenedShelfLifeDays,
    StorageLocation? OpenedLocation = null);

public record RecipeIngredient(string Key, bool Optional = false);

public record Recipe(
    string Key,
    string Title,
    string Description,
    int Minutes,
    RecipeIngredient[] Ingredients,
    string[] Steps);

public record IngredientText(string Name, string[] Aliases);

public record RecipeText(string Title, string Description, string[] Steps);

/// <summary>One language's names and recipe texts, keyed by ingredient and recipe key.</summary>
public record CatalogTranslation(Dictionary<string, IngredientText> Ingredients, Dictionary<string, RecipeText> Recipes);

/// <summary>
/// Read-only ingredient and recipe catalogue, loaded from <c>Seed/catalog.json</c> (English)
/// plus one <c>Seed/catalog.{Language}.json</c> per translation.
/// </summary>
public class Catalog
{
    private readonly Dictionary<string, Ingredient> _byKey;
    private readonly IReadOnlyDictionary<Language, CatalogTranslation> _translations;
    private readonly List<Alias> _aliases;

    // Compound words put the food last ("lettmelk", "kevytmaito"), so Nordic aliases also match a word's ending.
    // Short ones don't: "mel" (flour) would match "caramel"; list their compounds ("rugmel") as aliases instead.
    private record Alias(Ingredient Ingredient, string Words, bool MatchesWordEnding);
    private const int MinWordEndingLength = 4;

    public Catalog(IReadOnlyList<Ingredient> ingredients, IReadOnlyList<Recipe> recipes,
        IReadOnlyDictionary<Language, CatalogTranslation>? translations = null)
    {
        Ingredients = ingredients;
        Recipes = recipes;
        _byKey = ingredients.ToDictionary(i => i.Key, StringComparer.OrdinalIgnoreCase);
        _translations = translations ?? new Dictionary<Language, CatalogTranslation>();

        ThrowOnUnknownKeys();
        _aliases = ingredients.SelectMany(AliasesOf).Where(a => a.Words.Length > 0).ToList();
    }

    public IReadOnlyList<Ingredient> Ingredients { get; }
    public IReadOnlyList<Recipe> Recipes { get; }

    public Ingredient? Find(string? key) => key is not null && _byKey.TryGetValue(key, out var i) ? i : null;

    /// <summary>Best catalogue match for free text in any language, e.g. "Free-range eggs" → egg, "Tine lettmelk" → milk.</summary>
    public Ingredient? Match(string name)
    {
        var text = Words(name);
        if (FindByName(text) is { } exact) return exact;

        // Longest alias wins, so "coconut milk" beats "milk" and "peanut butter" isn't butter.
        var words = text.Split(' ');
        var padded = $" {text} ";
        return _aliases
            .Where(a => padded.Contains($" {a.Words} ") || (a.MatchesWordEnding && words.Any(w => w.EndsWith(a.Words))))
            .OrderByDescending(a => a.Words.Length)
            .Select(a => a.Ingredient)
            .FirstOrDefault();
    }

    /// <summary>Ingredient whose name or alias, in any language, is exactly this text (ignoring case).</summary>
    public Ingredient? FindByName(string text)
    {
        var words = Words(text);
        return _aliases.FirstOrDefault(a => a.Words == words)?.Ingredient;
    }

    /// <summary>Ingredients whose name or alias in any language starts with the query, named in the given language.</summary>
    public IEnumerable<Ingredient> Search(string? query, Language language)
    {
        var matches = string.IsNullOrWhiteSpace(query)
            ? Ingredients
            : Ingredients.Where(i => AliasesOf(i).Any(a => a.Words.StartsWith(Words(query))));
        return matches.Select(i => i with { Name = NameOf(i, language) });
    }

    public string NameOf(Ingredient ingredient, Language language) =>
        Translation(language)?.Ingredients.GetValueOrDefault(ingredient.Key)?.Name ?? ingredient.Name;

    public RecipeText TextOf(Recipe recipe, Language language) =>
        Translation(language)?.Recipes.GetValueOrDefault(recipe.Key)
        ?? new RecipeText(recipe.Title, recipe.Description, recipe.Steps);

    private CatalogTranslation? Translation(Language language) => _translations.GetValueOrDefault(language);

    private IEnumerable<Alias> AliasesOf(Ingredient ingredient)
    {
        foreach (var english in ingredient.Aliases.Prepend(ingredient.Name))
            yield return new Alias(ingredient, Words(english), MatchesWordEnding: false);

        foreach (var translation in _translations.Values)
        {
            if (translation.Ingredients.GetValueOrDefault(ingredient.Key) is not { } text) continue;
            foreach (var alias in text.Aliases.Prepend(text.Name))
                yield return new Alias(ingredient, Words(alias), MatchesWordEnding: alias.Length >= MinWordEndingLength);
        }
    }

    private static string Words(string s) =>
        string.Join(' ', s.ToLowerInvariant().Split([' ', '-'], StringSplitOptions.RemoveEmptyEntries));

    private void ThrowOnUnknownKeys()
    {
        var recipeKeys = Recipes.Select(r => r.Key).ToHashSet(StringComparer.OrdinalIgnoreCase);
        var unknown = Recipes.SelectMany(r => r.Ingredients).Select(i => i.Key).Where(k => !_byKey.ContainsKey(k))
            .Concat(_translations.Values.SelectMany(t => t.Ingredients.Keys).Where(k => !_byKey.ContainsKey(k)))
            .Concat(_translations.Values.SelectMany(t => t.Recipes.Keys).Where(k => !recipeKeys.Contains(k)))
            .Distinct()
            .ToList();
        if (unknown.Count > 0)
            throw new InvalidOperationException($"Catalogue references unknown keys: {string.Join(", ", unknown)}");
    }

    public static Catalog Load(string path)
    {
        var file = Read<CatalogFile>(path);
        var translations = Enum.GetValues<Language>()
            .Select(language => (language, path: Path.ChangeExtension(path, $"{language}.json")))
            .Where(x => File.Exists(x.path))
            .ToDictionary(x => x.language, x => Read<CatalogTranslation>(x.path));
        return new Catalog(file.Ingredients, file.Recipes, translations);
    }

    private static T Read<T>(string path)
    {
        var options = new JsonSerializerOptions(JsonSerializerDefaults.Web) { Converters = { new JsonStringEnumConverter() } };
        return JsonSerializer.Deserialize<T>(File.ReadAllText(path), options)
            ?? throw new InvalidOperationException($"Could not read catalogue file {path}");
    }

    private record CatalogFile(List<Ingredient> Ingredients, List<Recipe> Recipes);
}
