namespace EatUp.Api.Domain;

/// <summary>An active item with its expiry already resolved against today.</summary>
public record PantryItem(int Id, string Name, string? IngredientKey, int DaysLeft);

public record Suggestion(Recipe Recipe, int Score, IReadOnlyList<PantryItem> UsesItems, IReadOnlyList<string> MissingIngredients, bool IsPlantBased);

public static class SuggestionEngine
{
    public static bool IsSuitable(IEnumerable<AnimalProduct> animals, DietType diet)
    {
        var set = animals.ToHashSet();
        return diet switch
        {
            DietType.Vegan => !set.Overlaps([AnimalProduct.Dairy, AnimalProduct.Egg, AnimalProduct.Fish, AnimalProduct.Meat]),
            DietType.Vegetarian => !set.Overlaps([AnimalProduct.Fish, AnimalProduct.Meat]),
            DietType.Pescatarian => !set.Contains(AnimalProduct.Meat),
            _ => true,
        };
    }

    /// <summary>
    /// Recipes that use at least one urgent item, with allergens and unsuitable diets excluded,
    /// ranked so the most urgent items and the most plant-based dishes come first.
    /// </summary>
    public static IReadOnlyList<Suggestion> Suggest(Catalog catalog, UserProfile profile, IReadOnlyList<PantryItem> items, int limit = 5)
    {
        var allergens = profile.Allergens.ToHashSet();
        var ranked = new List<Suggestion>();

        foreach (var recipe in catalog.Recipes)
        {
            // Optional ingredients count too: a suggestion must be safe to cook as written.
            var ingredients = recipe.Ingredients.Select(ri => (ri, ing: catalog.Find(ri.Key)!)).ToList();
            if (ingredients.Any(x => x.ing.Allergens.Any(allergens.Contains))) continue;

            var animals = ingredients.Select(x => x.ing.Animal).ToList();
            if (!IsSuitable(animals, profile.Diet)) continue;

            var keys = ingredients.Select(x => x.ing.Key).ToHashSet(StringComparer.OrdinalIgnoreCase);
            var uses = items
                .Where(i => i.IngredientKey is not null && keys.Contains(i.IngredientKey) && i.DaysLeft >= 0)
                .GroupBy(i => i.IngredientKey, StringComparer.OrdinalIgnoreCase)
                .Select(g => g.MinBy(i => i.DaysLeft)!)
                .ToList();
            var urgent = uses.Where(i => Expiry.IsUrgent(i.DaysLeft)).ToList();
            if (urgent.Count == 0) continue;

            var have = uses.Select(i => i.IngredientKey!).ToHashSet(StringComparer.OrdinalIgnoreCase);
            var missing = ingredients.Where(x => !x.ri.Optional && !have.Contains(x.ing.Key)).Select(x => x.ing.Name).ToList();

            var score =
                urgent.Sum(i => 10 + (Expiry.SoonDays - i.DaysLeft) * 2) +
                (uses.Count - urgent.Count) * 2 -
                missing.Count * 3 +
                PlantBonus(animals, profile.Diet);

            var plantBased = !animals.Any(a => a is AnimalProduct.Meat or AnimalProduct.Fish);
            ranked.Add(new Suggestion(recipe, score, uses.OrderBy(i => i.DaysLeft).ToList(), missing, plantBased));
        }

        var ordered = ranked.OrderByDescending(s => s.Score).ThenBy(s => s.Recipe.Minutes);
        return profile.Diet == DietType.Flexitarian ? TakeFlexitarian(ordered, limit) : ordered.Take(limit).ToList();
    }

    private static int PlantBonus(IReadOnlyList<AnimalProduct> animals, DietType diet)
    {
        if (animals.Contains(AnimalProduct.Meat)) return diet == DietType.Flexitarian ? -6 : 0;
        if (animals.Contains(AnimalProduct.Fish)) return diet == DietType.Flexitarian ? -2 : 1;
        if (animals.Any(a => a is AnimalProduct.Dairy or AnimalProduct.Egg)) return 2;
        return 3;
    }

    /// <summary>Flexitarians see at least two plant-based options for every meat or fish option.</summary>
    private static List<Suggestion> TakeFlexitarian(IOrderedEnumerable<Suggestion> ordered, int limit)
    {
        var plants = ordered.Where(s => s.IsPlantBased).ToList();
        var animals = ordered.Where(s => !s.IsPlantBased).ToList();

        var animalCount = Math.Min(animals.Count, limit / 3);
        var plantCount = Math.Min(plants.Count, limit - animalCount);
        animalCount = Math.Min(animalCount, plantCount / 2);

        return plants.Take(plantCount).Concat(animals.Take(animalCount))
            .OrderByDescending(s => s.Score).ThenBy(s => s.Recipe.Minutes)
            .ToList();
    }
}
