using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using EatUp.Api;
using EatUp.Api.Data;
using EatUp.Api.Domain;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;

namespace EatUp.Tests;

public class WasteStatsApiTests
{
    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web) { Converters = { new JsonStringEnumConverter() } };
    // 2026-12-01 00:30 in Oslo, still November 30 in UTC.
    private static readonly DateTimeOffset JustAfterMidnightInOslo = new(2026, 11, 30, 23, 30, 0, TimeSpan.Zero);

    [Fact]
    public async Task Without_history_the_stats_say_so()
    {
        using var factory = new ApiTests.Factory();

        var stats = await factory.CreateClient().GetFromJsonAsync<WasteSummary>("/api/stats/waste", Json);

        Assert.False(stats!.HasHistory);
    }

    [Fact]
    public async Task Counts_this_and_last_month_in_the_users_time_zone()
    {
        using var factory = new ApiTests.Factory();
        var (app, client) = await InOsloAsync(factory);
        var milk = await AddItemAsync(client, "Milk");
        await AddEventsAsync(app,
            Event(milk, ConsumeOutcome.Used, new DateTimeOffset(2026, 11, 30, 23, 10, 0, TimeSpan.Zero)), // Dec 1 in Oslo
            Event(milk, ConsumeOutcome.Used, new DateTimeOffset(2026, 11, 30, 23, 20, 0, TimeSpan.Zero)), // same milk again
            Event(milk, ConsumeOutcome.Discarded, new DateTimeOffset(2026, 11, 30, 22, 50, 0, TimeSpan.Zero))); // Nov 30 in Oslo

        var stats = await StatsAsync(client);

        Assert.True(stats.HasHistory);
        Assert.Equal(new MonthCounts(2026, 12, Used: 1, Discarded: 0), stats.ThisMonth);
        Assert.Equal(new MonthCounts(2026, 11, Used: 0, Discarded: 1), stats.LastMonth);
    }

    [Fact]
    public async Task Deleting_an_item_keeps_its_count()
    {
        using var factory = new ApiTests.Factory();
        var (_, client) = await InOsloAsync(factory);
        var milk = await AddItemAsync(client, "Milk");
        await client.PostAsJsonAsync($"/api/items/{milk}/consume", new ConsumeInput(ConsumeOutcome.Used, 0.25), Json);
        await client.PostAsJsonAsync($"/api/items/{milk}/consume", new ConsumeInput(ConsumeOutcome.Used, 0.25), Json);
        Assert.Equal(1, (await StatsAsync(client)).ThisMonth.Used);

        await client.DeleteAsync($"/api/items/{milk}");

        Assert.Equal(1, (await StatsAsync(client)).ThisMonth.Used);
    }

    [Fact]
    public async Task Month_edges_count_exactly_and_older_history_still_counts_as_history()
    {
        using var factory = new ApiTests.Factory();
        var (app, client) = await InOsloAsync(factory);
        var milk = await AddItemAsync(client, "Milk");
        var bread = await AddItemAsync(client, "Bread");
        var cheese = await AddItemAsync(client, "Cheese");
        await AddEventsAsync(app,
            Event(milk, ConsumeOutcome.Used, new DateTimeOffset(2026, 10, 31, 23, 0, 30, 500, TimeSpan.Zero)), // Nov 1 00:00:30.5 in Oslo
            Event(bread, ConsumeOutcome.Used, new DateTimeOffset(2026, 10, 31, 22, 59, 0, TimeSpan.Zero)), // Oct 31 23:59 in Oslo
            Event(cheese, ConsumeOutcome.Discarded, new DateTimeOffset(2026, 9, 10, 12, 0, 0, TimeSpan.Zero)));

        var stats = await StatsAsync(client);

        Assert.True(stats.HasHistory);
        Assert.Equal(new MonthCounts(2026, 11, Used: 1, Discarded: 0), stats.LastMonth);
    }

    private static async Task<(WebApplicationFactory<Program> App, HttpClient Client)> InOsloAsync(ApiTests.Factory factory)
    {
        var app = factory.WithWebHostBuilder(b => b.ConfigureTestServices(s => s.AddSingleton<TimeProvider>(new FixedTime(JustAfterMidnightInOslo))));
        var client = app.CreateClient();
        client.DefaultRequestHeaders.Add(Endpoints.ClientHeader, "1");
        await client.PatchAsJsonAsync("/api/profile", new { timeZone = "Europe/Oslo" }, Json);
        return (app, client);
    }

    private static async Task<WasteSummary> StatsAsync(HttpClient client) =>
        (await client.GetFromJsonAsync<WasteSummary>("/api/stats/waste", Json))!;

    private static async Task<int> AddItemAsync(HttpClient client, string name)
    {
        var response = await client.PostAsJsonAsync("/api/items", new ItemInput(name, null, 1, "l", StorageLocation.Fridge, new DateOnly(2026, 12, 10), null), Json);
        return (await response.Content.ReadFromJsonAsync<ItemDto>(Json))!.Id;
    }

    private static async Task AddEventsAsync(WebApplicationFactory<Program> app, params ConsumeEvent[] events)
    {
        using var scope = app.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<EatUpDb>();
        db.ConsumeEvents.AddRange(events);
        await db.SaveChangesAsync();
    }

    private static ConsumeEvent Event(int itemId, ConsumeOutcome outcome, DateTimeOffset at) => new()
    {
        FoodItemId = itemId, ItemName = "Milk", Amount = 0.5, Unit = "l", Outcome = outcome, ConsumedAt = at,
    };

    private sealed class FixedTime(DateTimeOffset utcNow) : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => utcNow;
    }
}
