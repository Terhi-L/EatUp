namespace EatUp.Api.Domain;

public enum StorageLocation { Fridge, Freezer, Pantry, Counter }

public enum ItemStatus { Active, Used, Discarded }

public enum ConsumeOutcome { Used, Discarded }

public enum Urgency { Expired, Today, Soon, Later }

public enum DietType { Omnivore, Flexitarian, Pescatarian, Vegetarian, Vegan }

/// <summary>The kind of animal product an ingredient is, used to decide diet suitability.</summary>
public enum AnimalProduct { None, Dairy, Egg, Fish, Meat }

/// <summary>The 14 EU major allergens.</summary>
public enum Allergen
{
    Gluten, Crustaceans, Eggs, Fish, Peanuts, Soy, Milk,
    TreeNuts, Celery, Mustard, Sesame, Sulphites, Lupin, Molluscs,
}
