using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using EatUp.Api;
using EatUp.Api.Domain;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;

namespace EatUp.Tests;

public class ApiTests : IClassFixture<ApiTests.Factory>
{
    public class Factory : WebApplicationFactory<Program>
    {
        private readonly string _dbPath = Path.Combine(Path.GetTempPath(), $"eatup-test-{Guid.NewGuid():N}.db");

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.UseEnvironment("Development");
            builder.UseSetting("ConnectionStrings:EatUp", $"Data Source={_dbPath};Pooling=False");
            builder.UseSetting("Push:EnableDailyJob", "false");
            builder.UseSetting("DataDirectory", Path.GetTempPath());
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            File.Delete(_dbPath);
        }
    }

    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web) { Converters = { new JsonStringEnumConverter() } };
    private readonly HttpClient _client;

    public ApiTests(Factory factory) => _client = factory.CreateClient();

    [Fact]
    public async Task Add_list_and_consume_an_item()
    {
        var created = await _client.PostAsJsonAsync("/api/items",
            new ItemInput("Test eggs", null, 6, "pcs", StorageLocation.Fridge, null, null), Json);
        Assert.Equal(HttpStatusCode.Created, created.StatusCode);
        var item = (await created.Content.ReadFromJsonAsync<ItemDto>(Json))!;
        Assert.Equal("egg", item.IngredientKey); // matched from the name
        Assert.Equal(Urgency.Later, item.Urgency); // default fridge shelf life

        var active = await _client.GetFromJsonAsync<List<ItemDto>>("/api/items", Json);
        Assert.NotNull(active);
        Assert.Contains(active, i => i.Id == item.Id);
        Assert.Equal(active.OrderBy(i => i.DaysLeft).Select(i => i.Id), active.Select(i => i.Id));

        var partial = await _client.PostAsJsonAsync($"/api/items/{item.Id}/consume", new ConsumeInput(ConsumeOutcome.Used, 2), Json);
        Assert.Equal(4, (await partial.Content.ReadFromJsonAsync<ItemDto>(Json))!.Quantity);

        await _client.PostAsJsonAsync($"/api/items/{item.Id}/consume", new ConsumeInput(ConsumeOutcome.Discarded, null), Json);
        var discarded = await _client.GetFromJsonAsync<List<ItemDto>>("/api/items?status=Discarded", Json);
        Assert.Contains(discarded!, i => i.Id == item.Id);
    }

    [Fact]
    public async Task Shopping_list_dedupes_open_items_ticks_and_clears()
    {
        var first = await (await _client.PostAsJsonAsync("/api/shopping", new ShoppingInput("Flour"), Json)).Content.ReadFromJsonAsync<ShoppingItem>(Json);
        var again = await (await _client.PostAsJsonAsync("/api/shopping", new ShoppingInput(" flour "), Json)).Content.ReadFromJsonAsync<ShoppingItem>(Json);
        Assert.Equal(first!.Id, again!.Id);

        await _client.PutAsJsonAsync($"/api/shopping/{first.Id}", new ShoppingInput("Flour", Done: true), Json);
        var list = await _client.GetFromJsonAsync<List<ShoppingItem>>("/api/shopping", Json);
        Assert.True(list!.Single(s => s.Id == first.Id).Done);

        await _client.PostAsync("/api/shopping/clear-done", null);
        list = await _client.GetFromJsonAsync<List<ShoppingItem>>("/api/shopping", Json);
        Assert.DoesNotContain(list!, s => s.Id == first.Id);
    }

    [Fact]
    public async Task Unknown_item_without_expiry_date_is_rejected()
    {
        var response = await _client.PostAsJsonAsync("/api/items",
            new ItemInput("Mystery jar", null, 1, "pcs", StorageLocation.Pantry, null, null), Json);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Demo_data_produces_suggestions()
    {
        var suggestions = await _client.GetFromJsonAsync<List<SuggestionDto>>("/api/suggestions", Json);

        Assert.NotEmpty(suggestions!);
    }

    [Fact]
    public void Real_catalogue_loads_and_is_mostly_plant_forward()
    {
        var catalog = Catalog.Load(Path.Combine(AppContext.BaseDirectory, "Seed", "catalog.json"));

        var withMeat = catalog.Recipes.Count(r => r.Ingredients.Any(i => catalog.Find(i.Key)!.Animal == AnimalProduct.Meat));
        Assert.True(withMeat * 4 < catalog.Recipes.Count, $"{withMeat} of {catalog.Recipes.Count} recipes contain meat");
    }
}
