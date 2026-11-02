using EatUp.Api.Data;
using EatUp.Api.Domain;
using EatUp.Api.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EatUp.Api;

public record ItemDto(
    int Id, string Name, string? IngredientKey, double Quantity, string Unit, StorageLocation Location,
    DateOnly ExpiryDate, DateOnly? OpenedDate, DateOnly EffectiveExpiry, int DaysLeft, Urgency Urgency, ItemStatus Status);

public record ItemInput(
    string Name, string? IngredientKey, double Quantity, string Unit, StorageLocation Location, DateOnly? ExpiryDate, DateOnly? OpenedDate);

public record ConsumeInput(ConsumeOutcome Outcome, double? Amount);

public record ProfileDto(DietType Diet, List<Allergen> Allergens, TimeOnly NotificationTime, string TimeZone, bool NotificationsEnabled);

public record SuggestionDto(
    string Key, string Title, string Description, int Minutes, string[] Steps,
    IReadOnlyList<PantryItem> UsesItems, IReadOnlyList<string> MissingIngredients, bool IsPlantBased);

public record ShoppingInput(string Name, bool Done = false);

public record PushSubscriptionInput(string Endpoint, PushKeys Keys);
public record PushKeys(string P256dh, string Auth);
public record EndpointInput(string Endpoint);

public static class Endpoints
{
    public static void MapEatUpApi(this WebApplication app)
    {
        var api = app.MapGroup("/api");

        api.MapGet("/health", () => Results.Ok(new { status = "ok" }));

        // Profile (single user in the MVP)
        api.MapGet("/profile", async (Pantry pantry) => ToDto(await pantry.GetProfileAsync()));
        api.MapPut("/profile", async (ProfileDto input, EatUpDb db) =>
        {
            var p = await db.Profiles.SingleAsync();
            if (p.TimeZone != input.TimeZone || p.NotificationTime != input.NotificationTime) p.LastNotifiedOn = null;
            p.Diet = input.Diet;
            p.Allergens = input.Allergens.Distinct().ToList();
            p.NotificationTime = input.NotificationTime;
            p.TimeZone = input.TimeZone;
            p.NotificationsEnabled = input.NotificationsEnabled;
            await db.SaveChangesAsync();
            return ToDto(p);
        });

        // Items
        api.MapGet("/items", async (EatUpDb db, Pantry pantry, Catalog catalog, ItemStatus? status) =>
        {
            var today = pantry.Today(await pantry.GetProfileAsync());
            var items = await db.Items.Where(i => i.Status == (status ?? ItemStatus.Active)).ToListAsync();
            return items.Select(i => ToDto(i, catalog, today)).OrderBy(i => i.DaysLeft).ThenBy(i => i.Name);
        });

        api.MapGet("/items/{id:int}", async (int id, EatUpDb db, Pantry pantry, Catalog catalog) =>
            await db.Items.FindAsync(id) is { } item
                ? Results.Ok(ToDto(item, catalog, pantry.Today(await pantry.GetProfileAsync())))
                : Results.NotFound());

        api.MapPost("/items", async (ItemInput input, EatUpDb db, Pantry pantry, Catalog catalog, TimeProvider time) =>
        {
            var today = pantry.Today(await pantry.GetProfileAsync());
            var item = new FoodItem { Name = "", CreatedAt = time.GetUtcNow() };
            if (Apply(item, input, catalog, today) is { } error) return error;
            db.Items.Add(item);
            await db.SaveChangesAsync();
            return Results.Created($"/api/items/{item.Id}", ToDto(item, catalog, today));
        });

        api.MapPut("/items/{id:int}", async (int id, ItemInput input, EatUpDb db, Pantry pantry, Catalog catalog) =>
        {
            if (await db.Items.FindAsync(id) is not { } item) return Results.NotFound();
            var today = pantry.Today(await pantry.GetProfileAsync());
            if (Apply(item, input, catalog, today) is { } error) return error;
            await db.SaveChangesAsync();
            return Results.Ok(ToDto(item, catalog, today));
        });

        api.MapDelete("/items/{id:int}", async (int id, EatUpDb db) =>
            await db.Items.Where(i => i.Id == id).ExecuteDeleteAsync() > 0 ? Results.NoContent() : Results.NotFound());

        // Used or thrown away. A partial amount reduces the quantity and keeps the item active.
        api.MapPost("/items/{id:int}/consume", async (int id, ConsumeInput input, EatUpDb db, Pantry pantry, Catalog catalog, TimeProvider time) =>
        {
            if (await db.Items.FindAsync(id) is not { } item) return Results.NotFound();
            if (input.Amount is { } amount && amount > 0 && amount < item.Quantity)
            {
                item.Quantity -= amount;
            }
            else
            {
                item.Status = input.Outcome == ConsumeOutcome.Used ? ItemStatus.Used : ItemStatus.Discarded;
                item.StatusChangedAt = time.GetUtcNow();
            }
            await db.SaveChangesAsync();
            return Results.Ok(ToDto(item, catalog, pantry.Today(await pantry.GetProfileAsync())));
        });

        // Catalogue & suggestions
        api.MapGet("/ingredients", (Catalog catalog, string? search) => catalog.Search(search).Take(20));

        api.MapGet("/suggestions", async (Pantry pantry, int? limit) =>
            (await pantry.SuggestAsync(await pantry.GetProfileAsync(), limit ?? 5)).Select(s => new SuggestionDto(
                s.Recipe.Key, s.Recipe.Title, s.Recipe.Description, s.Recipe.Minutes, s.Recipe.Steps,
                s.UsesItems, s.MissingIngredients, s.IsPlantBased)));

        // Shopping list
        api.MapGet("/shopping", async (EatUpDb db) =>
            (await db.Shopping.ToListAsync()).OrderBy(s => s.Done).ThenBy(s => s.CreatedAt));

        // Adding something already on the list (and not ticked) returns the existing entry.
        api.MapPost("/shopping", async (ShoppingInput input, EatUpDb db, TimeProvider time) =>
        {
            var name = input.Name.Trim();
            if (name.Length == 0) return Results.ValidationProblem(new Dictionary<string, string[]> { ["name"] = ["Name is required."] });
            var open = await db.Shopping.Where(s => !s.Done).ToListAsync();
            if (open.FirstOrDefault(s => s.Name.Equals(name, StringComparison.OrdinalIgnoreCase)) is { } existing)
                return Results.Ok(existing);
            var item = new ShoppingItem { Name = name, CreatedAt = time.GetUtcNow() };
            db.Shopping.Add(item);
            await db.SaveChangesAsync();
            return Results.Created($"/api/shopping/{item.Id}", item);
        });

        api.MapPut("/shopping/{id:int}", async (int id, ShoppingInput input, EatUpDb db) =>
        {
            if (await db.Shopping.FindAsync(id) is not { } item) return Results.NotFound();
            if (!string.IsNullOrWhiteSpace(input.Name)) item.Name = input.Name.Trim();
            item.Done = input.Done;
            await db.SaveChangesAsync();
            return Results.Ok(item);
        });

        api.MapDelete("/shopping/{id:int}", async (int id, EatUpDb db) =>
            await db.Shopping.Where(s => s.Id == id).ExecuteDeleteAsync() > 0 ? Results.NoContent() : Results.NotFound());

        api.MapPost("/shopping/clear-done", async (EatUpDb db) =>
        {
            await db.Shopping.Where(s => s.Done).ExecuteDeleteAsync();
            return Results.NoContent();
        });

        // Web Push
        api.MapGet("/push/vapid-public-key", (VapidKeys keys) => new { publicKey = keys.PublicKey });

        api.MapPost("/push/subscriptions", async (PushSubscriptionInput input, EatUpDb db, TimeProvider time) =>
        {
            var existing = await db.PushSubscriptions.SingleOrDefaultAsync(s => s.Endpoint == input.Endpoint);
            if (existing is null)
            {
                db.PushSubscriptions.Add(new PushSubscriptionRecord
                {
                    Endpoint = input.Endpoint, P256dh = input.Keys.P256dh, Auth = input.Keys.Auth, CreatedAt = time.GetUtcNow(),
                });
            }
            else
            {
                existing.P256dh = input.Keys.P256dh;
                existing.Auth = input.Keys.Auth;
            }
            await db.SaveChangesAsync();
            return Results.NoContent();
        });

        api.MapDelete("/push/subscriptions", async ([FromBody] EndpointInput input, EatUpDb db) =>
        {
            await db.PushSubscriptions.Where(s => s.Endpoint == input.Endpoint).ExecuteDeleteAsync();
            return Results.NoContent();
        });

        if (app.Environment.IsDevelopment()) MapDevEndpoints(api.MapGroup("/dev"));
    }

    private static void MapDevEndpoints(RouteGroupBuilder dev)
    {
        dev.MapPost("/push/test", async (DailyCheck check) =>
        {
            var sent = await check.SendToAllAsync(new { title = "EatUp test 🥚", body = "Notifications are working!", url = "/" });
            return new { sent };
        });

        dev.MapPost("/run-daily-check", async (DailyCheck check) =>
        {
            var result = await check.RunAsync(force: true);
            return new { sent = result.Sent, title = result.Message?.Title, body = result.Message?.Body };
        });

        dev.MapPost("/reset-demo-data", async (EatUpDb db, TimeProvider time) =>
        {
            await DemoData.ResetItemsAsync(db, time);
            return Results.NoContent();
        });
    }

    private static IResult? Apply(FoodItem item, ItemInput input, Catalog catalog, DateOnly today)
    {
        if (string.IsNullOrWhiteSpace(input.Name)) return Results.ValidationProblem(new Dictionary<string, string[]> { ["name"] = ["Name is required."] });

        var ingredient = catalog.Find(input.IngredientKey) ?? catalog.Match(input.Name);
        var expiry = input.ExpiryDate ?? (ingredient is null ? null : Expiry.DefaultExpiry(ingredient, input.Location, today));
        if (expiry is null) return Results.ValidationProblem(new Dictionary<string, string[]> { ["expiryDate"] = ["Expiry date is required."] });

        item.Name = input.Name.Trim();
        item.IngredientKey = ingredient?.Key;
        item.Quantity = input.Quantity > 0 ? input.Quantity : 1;
        item.Unit = string.IsNullOrWhiteSpace(input.Unit) ? "pcs" : input.Unit.Trim();
        item.Location = input.Location;
        item.ExpiryDate = expiry.Value;
        item.OpenedDate = input.OpenedDate;
        return null;
    }

    private static ItemDto ToDto(FoodItem i, Catalog catalog, DateOnly today)
    {
        var effective = Expiry.EffectiveExpiry(i, catalog.Find(i.IngredientKey));
        var days = Expiry.DaysLeft(effective, today);
        return new ItemDto(i.Id, i.Name, i.IngredientKey, i.Quantity, i.Unit, i.Location, i.ExpiryDate, i.OpenedDate, effective, days, Expiry.UrgencyFor(days), i.Status);
    }

    private static ProfileDto ToDto(UserProfile p) => new(p.Diet, p.Allergens, p.NotificationTime, p.TimeZone, p.NotificationsEnabled);
}
