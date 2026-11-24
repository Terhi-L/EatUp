using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using EatUp.Api;
using EatUp.Api.Domain;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;

namespace EatUp.Tests;

public class MealPlanApiTests : IClassFixture<ApiTests.Factory>
{
    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web) { Converters = { new JsonStringEnumConverter() } };
    private static readonly DateOnly Monday = new(2026, 11, 23);
    private static readonly DateOnly Wednesday = Monday.AddDays(2);
    private readonly ApiTests.Factory _factory;
    private readonly HttpClient _client;

    public MealPlanApiTests(ApiTests.Factory factory)
    {
        _factory = factory;
        _client = WithClientHeader(factory.CreateClient());
    }

    [Fact]
    public async Task Plan_a_week_then_change_and_remove_meals()
    {
        var week = new DateOnly(2027, 2, 1);
        var dinner = await AddAsync(week.AddDays(2), new PlannedMealInput(week.AddDays(2), MealSlot.Dinner, "veggie-omelette", null));
        var breakfast = await AddAsync(week.AddDays(2), new PlannedMealInput(week.AddDays(2), MealSlot.Breakfast, null, "  Porridge  "));
        Assert.NotNull(dinner.RecipeTitle);
        Assert.Equal("Porridge", breakfast.FreeText);

        var plan = await GetWeekAsync(week.AddDays(6));
        Assert.Equal(week, plan.WeekStart);
        Assert.Equal([breakfast.Id, dinner.Id], plan.Meals.Select(m => m.Id)); // breakfast first, not alphabetical

        var changed = await _client.PutAsJsonAsync($"/api/meal-plans/{week:yyyy-MM-dd}/meals/{dinner.Id}",
            new PlannedMealInput(week.AddDays(4), MealSlot.Lunch, null, "Leftovers"), Json);
        Assert.Equal(HttpStatusCode.OK, changed.StatusCode);
        var lunch = (await changed.Content.ReadFromJsonAsync<PlannedMealDto>(Json))!;
        Assert.Equal((week.AddDays(4), MealSlot.Lunch, null, "Leftovers"), (lunch.Date, lunch.Slot, lunch.RecipeKey, lunch.FreeText));

        var deleted = await _client.DeleteAsync($"/api/meal-plans/{week:yyyy-MM-dd}/meals/{breakfast.Id}");
        Assert.Equal(HttpStatusCode.NoContent, deleted.StatusCode);
        Assert.Equal([dinner.Id], (await GetWeekAsync(week)).Meals.Select(m => m.Id));
    }

    [Fact]
    public async Task A_new_meal_can_be_read_from_its_location()
    {
        var created = await _client.PostAsJsonAsync($"/api/meal-plans/{Monday:yyyy-MM-dd}/meals",
            new PlannedMealInput(Wednesday, MealSlot.Dinner, null, "Fish soup"), Json);

        var meal = await _client.GetFromJsonAsync<PlannedMealDto>(created.Headers.Location, Json);

        Assert.Equal((await created.Content.ReadFromJsonAsync<PlannedMealDto>(Json))!.Id, meal!.Id);
    }

    [Fact]
    public async Task First_meals_saved_at_the_same_moment_all_land_in_one_week()
    {
        var week = new DateOnly(2028, 3, 6);

        var responses = await Task.WhenAll(Enumerable.Range(0, 10).Select(i =>
            _client.PostAsJsonAsync($"/api/meal-plans/{week:yyyy-MM-dd}/meals", new PlannedMealInput(week, MealSlot.Lunch, null, $"Lunch {i}"), Json)));

        Assert.All(responses, r => Assert.Equal(HttpStatusCode.Created, r.StatusCode));
        Assert.Equal(10, (await GetWeekAsync(week)).Meals.Count);
    }

    [Fact]
    public async Task A_week_without_meals_is_empty()
    {
        var plan = await GetWeekAsync(new DateOnly(2030, 1, 9));

        Assert.Equal(new DateOnly(2030, 1, 7), plan.WeekStart);
        Assert.Empty(plan.Meals);
    }

    [Theory]
    [InlineData("veggie-omelette", "and toast", "meal")]
    [InlineData(null, null, "meal")]
    [InlineData(null, " ", "meal")]
    [InlineData("unicorn-stew", null, "recipeKey")]
    public async Task An_invalid_meal_is_rejected(string? recipeKey, string? freeText, string field)
    {
        var response = await _client.PostAsJsonAsync($"/api/meal-plans/{Monday:yyyy-MM-dd}/meals",
            new PlannedMealInput(Wednesday, MealSlot.Dinner, recipeKey, freeText), Json);

        await AssertValidationErrorAsync(response, field);
    }

    [Fact]
    public async Task A_meal_without_a_slot_or_date_is_rejected_instead_of_defaulting()
    {
        var noSlot = await _client.PostAsJsonAsync($"/api/meal-plans/{Monday:yyyy-MM-dd}/meals", new { date = Wednesday, freeText = "Tacos" }, Json);
        var noDate = await _client.PostAsJsonAsync($"/api/meal-plans/{Monday:yyyy-MM-dd}/meals", new { slot = "Dinner", freeText = "Tacos" }, Json);

        Assert.Equal(HttpStatusCode.BadRequest, noSlot.StatusCode);
        Assert.Equal(HttpStatusCode.BadRequest, noDate.StatusCode);
        Assert.DoesNotContain((await GetWeekAsync(Monday)).Meals, m => m.FreeText == "Tacos");
    }

    [Fact]
    public async Task A_meal_sends_only_the_recipe_or_the_free_text_it_uses()
    {
        var freeText = await _client.PostAsJsonAsync($"/api/meal-plans/{Monday:yyyy-MM-dd}/meals", new { date = Wednesday, slot = "Lunch", freeText = "Soup" }, Json);
        var recipe = await _client.PostAsJsonAsync($"/api/meal-plans/{Monday:yyyy-MM-dd}/meals", new { date = Wednesday, slot = "Dinner", recipeKey = "veggie-omelette" }, Json);

        Assert.Equal(HttpStatusCode.Created, freeText.StatusCode);
        Assert.Equal(HttpStatusCode.Created, recipe.StatusCode);
    }

    [Fact]
    public async Task A_meal_dated_outside_the_week_is_rejected()
    {
        var response = await _client.PostAsJsonAsync($"/api/meal-plans/{Monday:yyyy-MM-dd}/meals",
            new PlannedMealInput(Monday.AddDays(7), MealSlot.Dinner, "veggie-omelette", null), Json);

        await AssertValidationErrorAsync(response, "date");
    }

    [Fact]
    public async Task A_meal_is_only_found_in_its_own_week()
    {
        var meal = await AddAsync(Monday, new PlannedMealInput(Wednesday, MealSlot.Lunch, null, "Soup"));
        var otherWeek = Monday.AddDays(7);

        var update = await _client.PutAsJsonAsync($"/api/meal-plans/{otherWeek:yyyy-MM-dd}/meals/{meal.Id}",
            new PlannedMealInput(otherWeek, MealSlot.Lunch, null, "Soup"), Json);
        var delete = await _client.DeleteAsync($"/api/meal-plans/{otherWeek:yyyy-MM-dd}/meals/{meal.Id}");

        Assert.Equal(HttpStatusCode.NotFound, update.StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, delete.StatusCode);
        Assert.Contains((await GetWeekAsync(Monday)).Meals, m => m.Id == meal.Id);
    }

    [Fact]
    public async Task The_current_week_starts_on_Monday_in_the_users_time_zone()
    {
        // Sunday 23:30 UTC is already Monday 00:30 in Oslo.
        var sundayNightUtc = new DateTimeOffset(2026, 11, 29, 23, 30, 0, TimeSpan.Zero);
        var client = WithClientHeader(_factory.WithWebHostBuilder(b =>
            b.ConfigureTestServices(s => s.AddSingleton<TimeProvider>(new FixedTime(sundayNightUtc)))).CreateClient());
        await client.PatchAsJsonAsync("/api/profile", new { timeZone = "Europe/Oslo" }, Json);

        var plan = await client.GetFromJsonAsync<MealPlanDto>("/api/meal-plans/current", Json);

        Assert.Equal(new DateOnly(2026, 11, 30), plan!.WeekStart);
    }

    [Fact]
    public async Task Planning_a_meal_needs_the_client_header()
    {
        var bare = _factory.CreateClient();

        var response = await bare.PostAsJsonAsync($"/api/meal-plans/{Monday:yyyy-MM-dd}/meals",
            new PlannedMealInput(Wednesday, MealSlot.Dinner, null, "Tacos"), Json);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    private async Task<PlannedMealDto> AddAsync(DateOnly week, PlannedMealInput choice)
    {
        var response = await _client.PostAsJsonAsync($"/api/meal-plans/{week:yyyy-MM-dd}/meals", choice, Json);
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        return (await response.Content.ReadFromJsonAsync<PlannedMealDto>(Json))!;
    }

    private async Task<MealPlanDto> GetWeekAsync(DateOnly week) =>
        (await _client.GetFromJsonAsync<MealPlanDto>($"/api/meal-plans/{week:yyyy-MM-dd}", Json))!;

    private static async Task AssertValidationErrorAsync(HttpResponseMessage response, string field)
    {
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        using var problem = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        Assert.True(problem.RootElement.GetProperty("errors").TryGetProperty(field, out _), $"Expected an error for '{field}'.");
    }

    private static HttpClient WithClientHeader(HttpClient client)
    {
        client.DefaultRequestHeaders.Add(Endpoints.ClientHeader, "1");
        return client;
    }

    private sealed class FixedTime(DateTimeOffset utcNow) : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => utcNow;
    }
}
