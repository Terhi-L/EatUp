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

public record ProfileDto(DietType Diet, List<Allergen> Allergens, TimeOnly NotificationTime, string TimeZone, bool NotificationsEnabled, Language? Language);

/// <summary>Only the fields that are set are changed, so separate edits (and the time-zone sync) can't overwrite each other.</summary>
public record ProfileUpdate(
    DietType? Diet = null, List<Allergen>? Allergens = null, TimeOnly? NotificationTime = null, string? TimeZone = null,
    bool? NotificationsEnabled = null, Language? Language = null);

public record SuggestionDto(
    string Key, string Title, string Description, int Minutes, string[] Steps,
    IReadOnlyList<PantryItem> UsesItems, IReadOnlyList<string> MissingIngredients, bool IsPlantBased);

/// <summary>A scanned product, prefilled into the add form. Null fields are for the user to fill in.</summary>
public record ProductDto(string? Name, string? ProductName, double? Quantity, string? Unit);

public record ShoppingInput(string Name, bool Done = false);

public record PushSubscriptionInput(string Endpoint, PushKeys Keys);
public record PushKeys(string P256dh, string Auth);
public record EndpointInput(string Endpoint);
public record VapidPublicKeyDto(string PublicKey);
public record HealthDto(string Status);

public record PushTestResult(int Sent);
public record DailyCheckResultDto(int Sent, string? Title, string? Body);

public static class Endpoints
{
    public const string ClientHeader = "X-EatUp";

    public static void MapEatUpApi(this WebApplication app)
    {
        // No auth in the MVP: require a custom header on writes so other websites can't trigger them
        // (a cross-origin request with a custom header needs a CORS preflight, which this API never allows).
        var api = app.MapGroup("/api").AddEndpointFilter(async (context, next) =>
        {
            var request = context.HttpContext.Request;
            return HttpMethods.IsGet(request.Method) || request.Headers.ContainsKey(ClientHeader)
                ? await next(context)
                : Results.Problem("Missing " + ClientHeader + " header.", statusCode: StatusCodes.Status403Forbidden);
        });

        api.MapGet("/health", () => new HealthDto("ok"));

        // Profile (single user in the MVP)
        api.MapGet("/profile", async (Pantry pantry) => ToDto(await pantry.GetProfileAsync()));
        api.MapPatch("/profile", async (ProfileUpdate input, EatUpDb db) =>
        {
            if (input.Language is { } requested && !Enum.IsDefined(requested))
                return Results.ValidationProblem(new Dictionary<string, string[]> { ["language"] = ["Unknown language."] });
            var p = await db.Profiles.SingleAsync();
            if ((input.TimeZone is not null && input.TimeZone != p.TimeZone) ||
                (input.NotificationTime is { } t && t != p.NotificationTime)) p.LastNotifiedOn = null;
            if (input.Diet is { } diet) p.Diet = diet;
            if (input.Allergens is { } allergens) p.Allergens = allergens.Distinct().ToList();
            if (input.NotificationTime is { } time) p.NotificationTime = time;
            if (input.TimeZone is { Length: > 0 } zone) p.TimeZone = zone;
            if (input.NotificationsEnabled is { } enabled) p.NotificationsEnabled = enabled;
            if (input.Language is { } language) p.Language = language;
            await db.SaveChangesAsync();
            return Results.Ok(ToDto(p));
        }).Produces<ProfileDto>();

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
                : Results.NotFound()).Produces<ItemDto>();

        api.MapPost("/items", async (ItemInput input, EatUpDb db, Pantry pantry, Catalog catalog, TimeProvider time) =>
        {
            var today = pantry.Today(await pantry.GetProfileAsync());
            var item = new FoodItem { Name = "", CreatedAt = time.GetUtcNow() };
            if (Apply(item, input, catalog, today) is { } error) return error;
            db.Items.Add(item);
            await db.SaveChangesAsync();
            return Results.Created($"/api/items/{item.Id}", ToDto(item, catalog, today));
        }).Produces<ItemDto>(StatusCodes.Status201Created);

        api.MapPut("/items/{id:int}", async (int id, ItemInput input, EatUpDb db, Pantry pantry, Catalog catalog) =>
        {
            if (await db.Items.FindAsync(id) is not { } item) return Results.NotFound();
            var today = pantry.Today(await pantry.GetProfileAsync());
            if (Apply(item, input, catalog, today) is { } error) return error;
            await db.SaveChangesAsync();
            return Results.Ok(ToDto(item, catalog, today));
        }).Produces<ItemDto>();

        api.MapDelete("/items/{id:int}", async (int id, EatUpDb db) =>
            await db.Items.Where(i => i.Id == id).ExecuteDeleteAsync() > 0 ? Results.NoContent() : Results.NotFound()).Produces(StatusCodes.Status204NoContent);

        // Used or thrown away. A partial amount reduces the quantity and keeps the item active.
        api.MapPost("/items/{id:int}/consume", async (int id, ConsumeInput input, EatUpDb db, Pantry pantry, Catalog catalog, TimeProvider time) =>
        {
            if (await db.Items.FindAsync(id) is not { } item) return Results.NotFound();
            if (input.Amount is { } amount && (!double.IsFinite(amount) || amount <= 0))
                return Results.ValidationProblem(new Dictionary<string, string[]> { ["amount"] = ["Amount must be more than zero."] });
            if (!Consumption.CanConsume(item))
                return Results.Problem($"This item is already {item.Status.ToString().ToLowerInvariant()}.", statusCode: StatusCodes.Status409Conflict);
            db.ConsumeEvents.Add(Consumption.Consume(item, input.Outcome, input.Amount, time.GetUtcNow()));
            await db.SaveChangesAsync();
            return Results.Ok(ToDto(item, catalog, pantry.Today(await pantry.GetProfileAsync())));
        }).Produces<ItemDto>().ProducesProblem(StatusCodes.Status409Conflict);

        // Catalogue & suggestions
        api.MapGet("/ingredients", async (Catalog catalog, Pantry pantry, string? search) =>
            catalog.Search(search, (await pantry.GetProfileAsync()).LanguageOrDefault).Take(20));

        api.MapGet("/products/{barcode}", LookUpProductAsync).Produces<ProductDto>();

        api.MapGet("/suggestions", async (Pantry pantry, Catalog catalog, int? limit) =>
        {
            var profile = await pantry.GetProfileAsync();
            var suggestions = await pantry.SuggestAsync(profile, limit ?? 5);
            return suggestions.Select(s => ToDto(s, catalog, profile.LanguageOrDefault));
        });

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
        }).Produces<ShoppingItem>().Produces<ShoppingItem>(StatusCodes.Status201Created);

        api.MapPut("/shopping/{id:int}", async (int id, ShoppingInput input, EatUpDb db) =>
        {
            if (await db.Shopping.FindAsync(id) is not { } item) return Results.NotFound();
            if (!string.IsNullOrWhiteSpace(input.Name)) item.Name = input.Name.Trim();
            item.Done = input.Done;
            await db.SaveChangesAsync();
            return Results.Ok(item);
        }).Produces<ShoppingItem>();

        api.MapDelete("/shopping/{id:int}", async (int id, EatUpDb db) =>
            await db.Shopping.Where(s => s.Id == id).ExecuteDeleteAsync() > 0 ? Results.NoContent() : Results.NotFound()).Produces(StatusCodes.Status204NoContent);

        api.MapPost("/shopping/clear-done", async (EatUpDb db) =>
        {
            await db.Shopping.Where(s => s.Done).ExecuteDeleteAsync();
            return Results.NoContent();
        }).Produces(StatusCodes.Status204NoContent);

        // Web Push
        api.MapGet("/push/vapid-public-key", (VapidKeys keys) => new VapidPublicKeyDto(keys.PublicKey));

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
        }).Produces(StatusCodes.Status204NoContent);

        api.MapDelete("/push/subscriptions", async ([FromBody] EndpointInput input, EatUpDb db) =>
        {
            await db.PushSubscriptions.Where(s => s.Endpoint == input.Endpoint).ExecuteDeleteAsync();
            return Results.NoContent();
        }).Produces(StatusCodes.Status204NoContent);

        if (app.Environment.IsDevelopment()) MapDevEndpoints(api.MapGroup("/dev"));
    }

    private static void MapDevEndpoints(RouteGroupBuilder dev)
    {
        dev.MapPost("/push/test", async (DailyCheck check) =>
        {
            var sent = await check.SendToAllAsync(new { title = "EatUp test 🥚", body = "Notifications are working!", url = "/" });
            return new PushTestResult(sent);
        });

        dev.MapPost("/run-daily-check", async (DailyCheck check) =>
        {
            var result = await check.RunAsync(force: true);
            return new DailyCheckResultDto(result.Sent, result.Message?.Title, result.Message?.Body);
        });

        dev.MapPost("/reset-demo-data", async (EatUpDb db, TimeProvider time) =>
        {
            await DemoData.ResetItemsAsync(db, time);
            return Results.NoContent();
        }).Produces(StatusCodes.Status204NoContent);
    }

    private static async Task<IResult> LookUpProductAsync(
        string barcode, IProductLookup lookup, Catalog catalog, Pantry pantry, CancellationToken cancellationToken)
    {
        if (!IsProductBarcode(barcode))
            return Results.ValidationProblem(new Dictionary<string, string[]> { ["barcode"] = ["Barcode must be 8 to 14 digits."] });

        var language = (await pantry.GetProfileAsync()).LanguageOrDefault;
        ScannedProduct? product;
        try
        {
            product = await lookup.FindAsync(barcode, language, cancellationToken);
        }
        catch (Exception e) when (e is HttpRequestException or TaskCanceledException && !cancellationToken.IsCancellationRequested)
        {
            return Results.Problem("Couldn't reach Open Food Facts. Fill in the details yourself.", statusCode: StatusCodes.Status502BadGateway);
        }
        if (product is null) return Results.Problem("Product not found.", statusCode: StatusCodes.Status404NotFound);

        var recognised = ProductRecognition.Recognise(product, catalog, language);
        return Results.Ok(new ProductDto(recognised.Name, recognised.ProductName, recognised.Size?.Quantity, recognised.Size?.Unit));
    }

    private static SuggestionDto ToDto(Suggestion s, Catalog catalog, Language language)
    {
        var text = catalog.TextOf(s.Recipe, language);
        var missing = s.MissingIngredients.Select(i => catalog.NameOf(i, language)).ToList();
        return new SuggestionDto(s.Recipe.Key, text.Title, text.Description, s.Recipe.Minutes, text.Steps, s.UsesItems, missing, s.IsPlantBased);
    }

    // EAN-8, UPC-A, EAN-13 and GTIN-14.
    private static bool IsProductBarcode(string barcode) => barcode.Length is >= 8 and <= 14 && barcode.All(char.IsAsciiDigit);

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

    private static ProfileDto ToDto(UserProfile p) => new(p.Diet, p.Allergens, p.NotificationTime, p.TimeZone, p.NotificationsEnabled, p.Language);
}
