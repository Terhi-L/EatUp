using System.Text.Json;
using EatUp.Api.Domain;
using static EatUp.Tests.TestCatalog;

namespace EatUp.Tests;

public class NutritionTests
{
    private static readonly Catalog Real = Catalog.Load(Path.Combine(AppContext.BaseDirectory, "Seed", "catalog.json"));

    private static readonly Dictionary<ProteinSource, AnimalProduct> AnimalOf = new()
    {
        [ProteinSource.Egg] = AnimalProduct.Egg,
        [ProteinSource.Dairy] = AnimalProduct.Dairy,
        [ProteinSource.Fish] = AnimalProduct.Fish,
        [ProteinSource.Poultry] = AnimalProduct.Meat,
        [ProteinSource.RedMeat] = AnimalProduct.Meat,
        [ProteinSource.ProcessedMeat] = AnimalProduct.Meat,
    };

    public static TheoryData<string> SeedRecipes() => new(Real.Recipes.Select(r => r.Key));

    [Fact]
    public void Every_seed_recipe_has_nutrition_tags() =>
        Assert.All(Real.Recipes, r => Assert.InRange(r.Nutrition.VegetablePortions, 0, 5));

    [Fact]
    public void A_recipe_without_nutrition_tags_is_rejected()
    {
        var untagged = Rec("omelette", "egg") with { Nutrition = null! };

        var error = Assert.Throws<InvalidOperationException>(() => new Catalog([Ing("egg")], [untagged]));
        Assert.Contains("omelette", error.Message);
    }

    [Fact]
    public void Nutrition_tags_missing_a_field_are_rejected()
    {
        var path = WriteCatalog("""{ "protein": "Egg", "omega3": false, "ultraProcessed": false }""");

        var error = Assert.Throws<JsonException>(() => Catalog.Load(path));
        Assert.Contains("vegetablePortions", error.Message);
    }

    [Theory]
    [MemberData(nameof(SeedRecipes))]
    public void Main_protein_comes_from_the_recipes_ingredients(string key)
    {
        var (recipe, ingredients) = RecipeWithIngredients(key);
        var animals = ingredients.Select(i => i.Animal).ToHashSet();
        var protein = recipe.Nutrition.Protein;

        if (animals.Contains(AnimalProduct.Meat)) Assert.Equal(AnimalProduct.Meat, AnimalOf.GetValueOrDefault(protein));
        else if (animals.Contains(AnimalProduct.Fish)) Assert.Equal(ProteinSource.Fish, protein);
        if (AnimalOf.TryGetValue(protein, out var animal)) Assert.Contains(animal, animals);
        if (protein == ProteinSource.Legumes) Assert.Contains(ingredients, i => i.Category is "Legumes" or "Plant protein");
    }

    [Theory]
    [MemberData(nameof(SeedRecipes))]
    public void Fish_dishes_are_an_omega3_source(string key)
    {
        var (recipe, ingredients) = RecipeWithIngredients(key);

        if (ingredients.Any(i => i.Animal == AnimalProduct.Fish)) Assert.True(recipe.Nutrition.Omega3);
    }

    [Theory]
    [MemberData(nameof(SeedRecipes))]
    public void Only_vegetables_and_fruit_count_as_vegetable_portions_not_potatoes(string key)
    {
        var (recipe, ingredients) = RecipeWithIngredients(key);

        if (!ingredients.Any(CountsTowardsVegetables)) Assert.Equal(0, recipe.Nutrition.VegetablePortions);
    }

    // Like "5 a day": vegetables, fruit and berries count, potatoes don't; canned tomatoes are filed under dry goods.
    private static bool CountsTowardsVegetables(Ingredient i) =>
        i.Key == "canned-tomatoes" || (i.Category is "Vegetables" or "Fruit" && i.Key != "potato");

    private static (Recipe Recipe, List<Ingredient> Ingredients) RecipeWithIngredients(string key)
    {
        var recipe = Real.Recipes.Single(r => r.Key == key);
        return (recipe, recipe.Ingredients.Select(ri => Real.Find(ri.Key)!).ToList());
    }

    private static string WriteCatalog(string nutritionJson)
    {
        var directory = Directory.CreateTempSubdirectory("eatup-catalog-");
        var path = Path.Combine(directory.FullName, "catalog.json");
        File.WriteAllText(path, $$"""
            {
              "ingredients": [{ "key": "egg", "name": "Egg", "aliases": [], "category": "Eggs", "allergens": [], "animal": "Egg",
                                "shelfLifeDays": { "Fridge": 28 }, "openedShelfLifeDays": null }],
              "recipes": [{ "key": "omelette", "title": "Omelette", "description": "", "minutes": 10,
                            "ingredients": [{ "key": "egg" }], "steps": [], "nutrition": {{nutritionJson}} }]
            }
            """);
        return path;
    }
}
