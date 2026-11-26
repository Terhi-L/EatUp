namespace EatUp.Api.Domain;

public enum StorageLocation { Fridge, Freezer, Cupboard, Counter }

public enum ItemStatus { Active, Used, Discarded }

public enum ConsumeOutcome { Used, Discarded }

/// <summary>Expired: throw it out. Today: eat it today, best-before food still worth checking included.</summary>
public enum Urgency { Expired, Today, Soon, Later }

/// <summary>What the package date means: past use by the food may be unsafe, past best before it is often still fine.</summary>
public enum ExpiryKind { UseBy, BestBefore }

/// <summary>What to do before eating or throwing out food past its date.</summary>
public enum ExpiryAdvice { None, LookSmellTaste, CheckPackage }

public enum DietType { Omnivore, Flexitarian, Pescatarian, Vegetarian, Vegan }

/// <summary>The kind of animal product an ingredient is, used to decide diet suitability.</summary>
public enum AnimalProduct { None, Dairy, Egg, Fish, Meat }

/// <summary>A dish's main protein, for the weekly health feedback. Meat or fish wins over egg or dairy in the same dish.</summary>
public enum ProteinSource { None, Legumes, Egg, Dairy, Fish, Poultry, RedMeat, ProcessedMeat }

/// <summary>The 14 EU major allergens.</summary>
public enum Allergen
{
    Gluten, Crustaceans, Eggs, Fish, Peanuts, Soy, Milk,
    TreeNuts, Celery, Mustard, Sesame, Sulphites, Lupin, Molluscs,
}

public enum Language { English, Norwegian, Finnish }

public enum MealSlot { Breakfast, Lunch, Dinner }
