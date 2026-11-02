using EatUp.Api.Domain;
using static EatUp.Tests.TestCatalog;

namespace EatUp.Tests;

public class SuggestionTests
{
    private static IReadOnlyList<string> Suggest(UserProfile profile, params PantryItem[] items) =>
        SuggestionEngine.Suggest(Default, profile, items, limit: 10).Select(s => s.Recipe.Key).ToList();

    [Fact]
    public void Vegan_never_gets_egg_dairy_meat_or_fish()
    {
        var keys = Suggest(new UserProfile { Diet = DietType.Vegan }, Item("egg", 1), Item("spinach", 1), Item("milk", 1));

        Assert.DoesNotContain("omelette", keys);
        Assert.DoesNotContain("creamed-spinach", keys);
        Assert.DoesNotContain("chicken-spinach", keys);
        Assert.DoesNotContain("salmon-spinach", keys);
        Assert.Contains("spinach-saute", keys);
    }

    [Fact]
    public void Vegetarian_gets_eggs_and_dairy_but_no_meat_or_fish()
    {
        var keys = Suggest(new UserProfile { Diet = DietType.Vegetarian }, Item("egg", 1), Item("spinach", 1));

        Assert.Contains("omelette", keys);
        Assert.DoesNotContain("chicken-spinach", keys);
        Assert.DoesNotContain("salmon-spinach", keys);
    }

    [Fact]
    public void Pescatarian_gets_fish_but_no_meat()
    {
        var keys = Suggest(new UserProfile { Diet = DietType.Pescatarian }, Item("spinach", 1));

        Assert.Contains("salmon-spinach", keys);
        Assert.DoesNotContain("chicken-spinach", keys);
    }

    [Fact]
    public void Recipes_with_the_users_allergens_are_excluded()
    {
        var keys = Suggest(new UserProfile { Allergens = [Allergen.TreeNuts, Allergen.Eggs] }, Item("spinach", 1), Item("egg", 1));

        Assert.DoesNotContain("spinach-walnut-salad", keys);
        Assert.DoesNotContain("omelette", keys); // egg is required
        Assert.Contains("spinach-saute", keys);
    }

    [Fact]
    public void Optional_ingredients_count_for_allergens_too()
    {
        var catalog = new Catalog([Ing("rice"), Ing("peanut", AnimalProduct.None, Allergen.Peanuts)], [Rec("rice-bowl", "rice", "peanut?")]);
        var result = SuggestionEngine.Suggest(catalog, new UserProfile { Allergens = [Allergen.Peanuts] }, [Item("rice", 0)]);

        Assert.Empty(result);
    }

    [Fact]
    public void Only_recipes_using_an_urgent_item_are_suggested()
    {
        var keys = Suggest(new UserProfile(), Item("spinach", 5), Item("egg", 1));

        Assert.Equal(["omelette"], keys);
    }

    [Fact]
    public void Expired_items_are_not_suggested()
    {
        Assert.Empty(Suggest(new UserProfile(), Item("egg", -1)));
    }

    [Fact]
    public void Items_closer_to_expiry_rank_higher()
    {
        var keys = Suggest(new UserProfile { Diet = DietType.Vegetarian }, Item("egg", 0), Item("spinach", 2));

        Assert.Equal("omelette", keys[0]);
    }

    [Fact]
    public void Plant_based_recipes_rank_above_meat_for_the_same_items()
    {
        var keys = Suggest(new UserProfile(), Item("spinach", 1), Item("chicken", 1), Item("tofu", 1));

        Assert.True(keys.ToList().IndexOf("spinach-tofu") < keys.ToList().IndexOf("chicken-spinach"));
    }

    [Fact]
    public void Flexitarian_sees_at_least_two_plant_based_options_per_meat_or_fish_option()
    {
        var result = SuggestionEngine.Suggest(Default, new UserProfile { Diet = DietType.Flexitarian },
            [Item("spinach", 0), Item("chicken", 0), Item("salmon", 0)], limit: 10);

        var animal = result.Count(s => !s.IsPlantBased);
        var plant = result.Count(s => s.IsPlantBased);
        Assert.True(animal >= 1);
        Assert.True(plant >= animal * 2, $"plant={plant}, animal={animal}");
    }

    [Fact]
    public void Missing_required_ingredients_are_reported()
    {
        var suggestion = SuggestionEngine.Suggest(Default, new UserProfile(), [Item("spinach", 0)], limit: 10)
            .Single(s => s.Recipe.Key == "chicken-spinach");

        Assert.Equal(["chicken"], suggestion.MissingIngredients);
    }
}
