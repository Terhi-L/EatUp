using EatUp.Api.Data;
using EatUp.Api.Domain;
using EatUp.Api.Services;
using Microsoft.EntityFrameworkCore;

namespace EatUp.Api;

public record PlannedMealDto(int Id, DateOnly Date, MealSlot Slot, string? RecipeKey, string? RecipeTitle, string? FreeText);

public record MealPlanDto(DateOnly WeekStart, IReadOnlyList<PlannedMealDto> Meals);

/// <summary>The weekly meal planner. A <c>{week}</c> route value is any date in the week; weeks start on Monday.</summary>
public static class MealPlanEndpoints
{
    public static void MapMealPlans(this RouteGroupBuilder api)
    {
        var plans = api.MapGroup("/meal-plans");
        plans.MapGet("/current", GetCurrentWeekAsync);
        plans.MapGet("/{week}", GetWeekAsync);
        plans.MapGet("/{week}/meals/{id:int}", GetMealAsync)
            .Produces<PlannedMealDto>().Produces(StatusCodes.Status404NotFound);
        plans.MapPost("/{week}/meals", AddMealAsync)
            .Produces<PlannedMealDto>(StatusCodes.Status201Created).ProducesValidationProblem();
        plans.MapPut("/{week}/meals/{id:int}", UpdateMealAsync)
            .Produces<PlannedMealDto>().ProducesValidationProblem().Produces(StatusCodes.Status404NotFound);
        plans.MapDelete("/{week}/meals/{id:int}", DeleteMealAsync)
            .Produces(StatusCodes.Status204NoContent).Produces(StatusCodes.Status404NotFound);
    }

    // "This week" is decided in the user's time zone, so Sunday evening abroad doesn't jump to next week.
    private static async Task<MealPlanDto> GetCurrentWeekAsync(EatUpDb db, Pantry pantry, Catalog catalog)
    {
        var profile = await pantry.GetProfileAsync();
        return await PlanDtoAsync(db, catalog, MealPlanning.WeekStart(pantry.Today(profile)), profile.LanguageOrDefault);
    }

    private static async Task<MealPlanDto> GetWeekAsync(DateOnly week, EatUpDb db, Pantry pantry, Catalog catalog) =>
        await PlanDtoAsync(db, catalog, MealPlanning.WeekStart(week), (await pantry.GetProfileAsync()).LanguageOrDefault);

    private static async Task<IResult> GetMealAsync(DateOnly week, int id, EatUpDb db, Pantry pantry, Catalog catalog) =>
        await MealsInWeek(db, MealPlanning.WeekStart(week)).SingleOrDefaultAsync(m => m.Id == id) is { } meal
            ? Results.Ok(ToDto(meal, catalog, (await pantry.GetProfileAsync()).LanguageOrDefault))
            : Results.NotFound();

    private static async Task<IResult> AddMealAsync(DateOnly week, MealChoice input, EatUpDb db, Pantry pantry, Catalog catalog)
    {
        var weekStart = MealPlanning.WeekStart(week);
        var choice = MealPlanning.Normalize(input, catalog);
        if (MealPlanning.Validate(choice, weekStart, catalog) is { Count: > 0 } errors) return Results.ValidationProblem(errors);

        var plan = await EnsurePlanAsync(db, weekStart);
        var meal = Apply(new PlannedMeal(), choice);
        plan.Meals.Add(meal);
        await db.SaveChangesAsync();
        var dto = ToDto(meal, catalog, (await pantry.GetProfileAsync()).LanguageOrDefault);
        return Results.Created($"/api/meal-plans/{weekStart:yyyy-MM-dd}/meals/{meal.Id}", dto);
    }

    private static async Task<IResult> UpdateMealAsync(DateOnly week, int id, MealChoice input, EatUpDb db, Pantry pantry, Catalog catalog)
    {
        var weekStart = MealPlanning.WeekStart(week);
        if (await MealsInWeek(db, weekStart).SingleOrDefaultAsync(m => m.Id == id) is not { } meal) return Results.NotFound();
        var choice = MealPlanning.Normalize(input, catalog);
        if (MealPlanning.Validate(choice, weekStart, catalog) is { Count: > 0 } errors) return Results.ValidationProblem(errors);

        Apply(meal, choice);
        await db.SaveChangesAsync();
        return Results.Ok(ToDto(meal, catalog, (await pantry.GetProfileAsync()).LanguageOrDefault));
    }

    private static async Task<IResult> DeleteMealAsync(DateOnly week, int id, EatUpDb db) =>
        await MealsInWeek(db, MealPlanning.WeekStart(week)).Where(m => m.Id == id).ExecuteDeleteAsync() > 0
            ? Results.NoContent()
            : Results.NotFound();

    // One atomic statement: first meals saved at the same moment would otherwise both insert the week and hit its unique index.
    private static async Task<MealPlan> EnsurePlanAsync(EatUpDb db, DateOnly weekStart)
    {
        await db.Database.ExecuteSqlAsync($"INSERT OR IGNORE INTO MealPlans (WeekStart) VALUES ({weekStart})");
        return await db.MealPlans.SingleAsync(p => p.WeekStart == weekStart);
    }

    private static IQueryable<PlannedMeal> MealsInWeek(EatUpDb db, DateOnly weekStart) =>
        db.PlannedMeals.Where(m => db.MealPlans.Any(p => p.Id == m.MealPlanId && p.WeekStart == weekStart));

    private static async Task<MealPlanDto> PlanDtoAsync(EatUpDb db, Catalog catalog, DateOnly weekStart, Language language)
    {
        var meals = await MealsInWeek(db, weekStart).ToListAsync();
        // Slots are stored as text, so they're ordered here by enum value rather than alphabetically in SQL.
        var ordered = meals.OrderBy(m => m.Date).ThenBy(m => m.Slot).ThenBy(m => m.Id);
        return new MealPlanDto(weekStart, ordered.Select(m => ToDto(m, catalog, language)).ToList());
    }

    private static PlannedMeal Apply(PlannedMeal meal, MealChoice choice)
    {
        meal.Date = choice.Date;
        meal.Slot = choice.Slot;
        meal.RecipeKey = choice.RecipeKey;
        meal.FreeText = choice.FreeText;
        return meal;
    }

    private static PlannedMealDto ToDto(PlannedMeal meal, Catalog catalog, Language language)
    {
        var title = catalog.FindRecipe(meal.RecipeKey) is { } recipe ? catalog.TextOf(recipe, language).Title : null;
        return new PlannedMealDto(meal.Id, meal.Date, meal.Slot, meal.RecipeKey, title, meal.FreeText);
    }
}
