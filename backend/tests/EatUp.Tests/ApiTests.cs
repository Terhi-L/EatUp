using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using EatUp.Api;
using EatUp.Api.Data;
using EatUp.Api.Domain;
using EatUp.Api.Services;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

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
    private readonly Factory _factory;
    private readonly HttpClient _client;

    public ApiTests(Factory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
        _client.DefaultRequestHeaders.Add(Endpoints.ClientHeader, "1");
    }

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

    private async Task<ItemDto> AddEggsAsync(double quantity = 6) =>
        (await (await _client.PostAsJsonAsync("/api/items",
            new ItemInput("Test eggs", null, quantity, "pcs", StorageLocation.Fridge, null, null), Json)).Content.ReadFromJsonAsync<ItemDto>(Json))!;

    private Task<HttpResponseMessage> ConsumeAsync(int id, ConsumeOutcome outcome, double? amount) =>
        _client.PostAsJsonAsync($"/api/items/{id}/consume", new ConsumeInput(outcome, amount), Json);

    private async Task<List<ConsumeEvent>> EventsAsync(Func<ConsumeEvent, bool> match)
    {
        using var scope = _factory.Services.CreateScope();
        var events = await scope.ServiceProvider.GetRequiredService<EatUpDb>().ConsumeEvents.AsNoTracking().ToListAsync();
        return events.Where(match).OrderBy(e => e.Id).ToList();
    }

    [Fact]
    public async Task Partial_full_and_overshooting_consumes_record_what_was_consumed()
    {
        var partlyThrownAway = await AddEggsAsync();
        var overshot = await AddEggsAsync();

        await ConsumeAsync(partlyThrownAway.Id, ConsumeOutcome.Discarded, 2);
        await ConsumeAsync(partlyThrownAway.Id, ConsumeOutcome.Used, null);
        await ConsumeAsync(overshot.Id, ConsumeOutcome.Used, 10);

        var partly = await EventsAsync(e => e.FoodItemId == partlyThrownAway.Id);
        Assert.Equal([(2.0, ConsumeOutcome.Discarded), (4.0, ConsumeOutcome.Used)], partly.Select(e => (e.Amount, e.Outcome)));
        Assert.All(partly, e => Assert.Equal(("Test eggs", "egg", "pcs"), (e.ItemName, e.IngredientKey, e.Unit)));
        Assert.Equal(6, Assert.Single(await EventsAsync(e => e.FoodItemId == overshot.Id)).Amount);
    }

    [Fact]
    public async Task Consuming_an_item_that_is_no_longer_active_is_a_conflict_and_records_nothing()
    {
        var eggs = await AddEggsAsync();
        await ConsumeAsync(eggs.Id, ConsumeOutcome.Used, null);

        var again = await ConsumeAsync(eggs.Id, ConsumeOutcome.Discarded, 1);

        Assert.Equal(HttpStatusCode.Conflict, again.StatusCode);
        Assert.Equal(ConsumeOutcome.Used, Assert.Single(await EventsAsync(e => e.FoodItemId == eggs.Id)).Outcome);
    }

    [Fact]
    public async Task An_undefined_outcome_is_rejected_and_records_nothing()
    {
        var eggs = await AddEggsAsync();

        var response = await _client.PostAsJsonAsync($"/api/items/{eggs.Id}/consume", new { outcome = 99 });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Empty(await EventsAsync(e => e.FoodItemId == eggs.Id));
    }

    [Fact]
    public async Task A_consume_that_lost_a_race_cannot_overwrite_the_winner()
    {
        var eggs = await AddEggsAsync();
        using var firstScope = _factory.Services.CreateScope();
        using var secondScope = _factory.Services.CreateScope();
        var first = firstScope.ServiceProvider.GetRequiredService<EatUpDb>();
        var second = secondScope.ServiceProvider.GetRequiredService<EatUpDb>();
        var firstCopy = (await first.Items.FindAsync(eggs.Id))!;
        var secondCopy = (await second.Items.FindAsync(eggs.Id))!;
        var now = DateTimeOffset.UtcNow;

        first.ConsumeEvents.Add(Consumption.Consume(firstCopy, ConsumeOutcome.Used, null, now));
        await first.SaveChangesAsync();
        second.ConsumeEvents.Add(Consumption.Consume(secondCopy, ConsumeOutcome.Discarded, 2, now));

        await Assert.ThrowsAsync<DbUpdateConcurrencyException>(() => second.SaveChangesAsync());
        Assert.Single(await EventsAsync(e => e.FoodItemId == eggs.Id));
    }

    [Fact]
    public async Task Deleting_an_item_or_resetting_demo_data_keeps_its_history()
    {
        var deleted = await AddEggsAsync();
        var reset = await AddEggsAsync(quantity: 12);
        await ConsumeAsync(deleted.Id, ConsumeOutcome.Discarded, 1);
        await ConsumeAsync(reset.Id, ConsumeOutcome.Used, 1);
        var recorded = (await EventsAsync(_ => true)).Select(e => e.Id).ToHashSet();

        await _client.DeleteAsync($"/api/items/{deleted.Id}");
        await _client.PostAsync("/api/dev/reset-demo-data", null);

        var kept = await EventsAsync(e => recorded.Contains(e.Id));
        Assert.Equal(recorded.Count, kept.Count);
        Assert.All(kept, e => Assert.Null(e.FoodItemId));
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
            new ItemInput("Mystery jar", null, 1, "pcs", StorageLocation.Cupboard, null, null), Json);

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

    [Fact]
    public async Task Writes_without_the_client_header_are_rejected()
    {
        var bare = _factory.CreateClient(); // no X-EatUp header, like a request from another website

        Assert.Equal(HttpStatusCode.Forbidden, (await bare.PostAsync("/api/dev/reset-demo-data", null)).StatusCode);
        Assert.Equal(HttpStatusCode.OK, (await bare.GetAsync("/api/profile")).StatusCode);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public async Task Consuming_a_non_positive_amount_is_rejected(double amount)
    {
        var item = await (await _client.PostAsJsonAsync("/api/items",
            new ItemInput("Test milk", null, 2, "l", StorageLocation.Fridge, null, null), Json)).Content.ReadFromJsonAsync<ItemDto>(Json);

        var response = await _client.PostAsJsonAsync($"/api/items/{item!.Id}/consume", new ConsumeInput(ConsumeOutcome.Used, amount), Json);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal(ItemStatus.Active, (await _client.GetFromJsonAsync<ItemDto>($"/api/items/{item.Id}", Json))!.Status);
    }

    [Fact]
    public async Task Profile_patch_only_changes_the_fields_sent()
    {
        await _client.PatchAsJsonAsync("/api/profile", new { diet = "Vegan" }, Json);
        var response = await _client.PatchAsJsonAsync("/api/profile", new { allergens = new[] { "Soy" } }, Json);

        var profile = (await response.Content.ReadFromJsonAsync<ProfileDto>(Json))!;
        Assert.Equal(DietType.Vegan, profile.Diet);
        Assert.Equal([Allergen.Soy], profile.Allergens);
    }

    [Fact]
    public async Task Unknown_language_is_rejected()
    {
        var response = await _client.PatchAsJsonAsync("/api/profile", new { language = 99 }, Json);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Catalogue_names_follow_the_profile_language()
    {
        try
        {
            await _client.PatchAsJsonAsync("/api/profile", new { language = "Norwegian" }, Json);

            var ingredients = await _client.GetFromJsonAsync<List<Ingredient>>("/api/ingredients?search=milk", Json);

            Assert.Contains(ingredients!, i => i.Key == "milk" && i.Name == "Melk");
        }
        finally
        {
            await _client.PatchAsJsonAsync("/api/profile", new { language = "English" }, Json);
        }
    }

    [Fact]
    public void Recipes_that_use_soy_sauce_declare_it()
    {
        var catalog = Catalog.Load(Path.Combine(AppContext.BaseDirectory, "Seed", "catalog.json"));

        var undeclared = catalog.Recipes
            .Where(r => r.Steps.Any(s => s.Contains("soy sauce", StringComparison.OrdinalIgnoreCase)))
            .Where(r => r.Ingredients.All(i => i.Key != "soy-sauce"))
            .Select(r => r.Key);
        Assert.Empty(undeclared);
    }

    [Theory]
    [InlineData("coconut-milk", StorageLocation.Fridge)]
    [InlineData("canned-tomatoes", StorageLocation.Fridge)]
    [InlineData("peanut-butter", null)]
    public void Catalog_says_where_opened_food_belongs(string key, StorageLocation? expected)
    {
        var catalog = Catalog.Load(Path.Combine(AppContext.BaseDirectory, "Seed", "catalog.json"));

        Assert.Equal(expected, catalog.Find(key)?.OpenedLocation);
    }

    [Theory]
    [InlineData("Coconut milk", "coconut-milk")]
    [InlineData("Organic coconut milk", "coconut-milk")]
    [InlineData("Free-range eggs", "egg")]
    [InlineData("Peanut butter", "peanut-butter")]
    public void Free_text_matches_the_longest_alias(string name, string? expected)
    {
        var catalog = Catalog.Load(Path.Combine(AppContext.BaseDirectory, "Seed", "catalog.json"));

        Assert.Equal(expected, catalog.Match(name)?.Key);
    }

    private class FakeProductLookup(ScannedProduct? product, Exception? failure = null) : IProductLookup
    {
        public Task<ScannedProduct?> FindAsync(string barcode, Language language, CancellationToken cancellationToken) =>
            failure is null ? Task.FromResult(product) : Task.FromException<ScannedProduct?>(failure);
    }

    private HttpClient ClientWithProducts(FakeProductLookup lookup) =>
        _factory.WithWebHostBuilder(b => b.ConfigureTestServices(s => s.AddSingleton<IProductLookup>(lookup))).CreateClient();

    [Fact]
    public async Task Scanned_product_is_matched_to_the_catalogue()
    {
        var client = ClientWithProducts(new(new ScannedProduct("Tine Lettmelk", ["en:dairies", "en:milks"], "1 l")));

        var product = await client.GetFromJsonAsync<ProductDto>("/api/products/7038010000737", Json);

        Assert.Equal(new ProductDto("Milk", "Tine Lettmelk", 1, "l"), product);
    }

    [Theory]
    [InlineData("12345", HttpStatusCode.BadRequest)]
    [InlineData("70380100007ab", HttpStatusCode.BadRequest)]
    [InlineData("7038010000737", HttpStatusCode.NotFound)]
    public async Task Unknown_or_invalid_barcodes_are_reported(string barcode, HttpStatusCode expected)
    {
        var client = ClientWithProducts(new(null));

        Assert.Equal(expected, (await client.GetAsync($"/api/products/{barcode}")).StatusCode);
    }

    [Fact]
    public async Task Product_database_outage_is_a_bad_gateway()
    {
        var client = ClientWithProducts(new(null, new HttpRequestException("offline")));

        Assert.Equal(HttpStatusCode.BadGateway, (await client.GetAsync("/api/products/7038010000737")).StatusCode);
    }
}
