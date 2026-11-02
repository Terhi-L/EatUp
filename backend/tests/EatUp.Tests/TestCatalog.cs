using EatUp.Api.Domain;

namespace EatUp.Tests;

/// <summary>Small hand-built catalogue so each test controls exactly which recipes exist.</summary>
public static class TestCatalog
{
    public static Ingredient Ing(string key, AnimalProduct animal = AnimalProduct.None, params Allergen[] allergens) =>
        new(key, key, [key], "Test", allergens, animal, new() { [StorageLocation.Fridge] = 7 }, null);

    public static Recipe Rec(string key, params string[] ingredients) =>
        new(key, key, "", 10, ingredients.Select(i => new RecipeIngredient(i.TrimEnd('?'), i.EndsWith('?'))).ToArray(), []);

    public static readonly Catalog Default = new(
        [
            Ing("egg", AnimalProduct.Egg, Allergen.Eggs),
            Ing("milk", AnimalProduct.Dairy, Allergen.Milk),
            Ing("spinach"),
            Ing("walnut", AnimalProduct.None, Allergen.TreeNuts),
            Ing("chicken", AnimalProduct.Meat),
            Ing("salmon", AnimalProduct.Fish, Allergen.Fish),
            Ing("tofu", AnimalProduct.None, Allergen.Soy),
        ],
        [
            Rec("omelette", "egg", "spinach?"),
            Rec("spinach-walnut-salad", "spinach", "walnut"),
            Rec("spinach-tofu", "spinach", "tofu"),
            Rec("spinach-saute", "spinach"),
            Rec("chicken-spinach", "chicken", "spinach"),
            Rec("salmon-spinach", "salmon", "spinach"),
            Rec("creamed-spinach", "spinach", "milk"),
        ]);

    public static PantryItem Item(string key, int daysLeft) => new(key.GetHashCode(), key, key, daysLeft);
}
