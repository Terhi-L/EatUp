using EatUp.Api.Data;
using EatUp.Api.Domain;
using Microsoft.EntityFrameworkCore;

namespace EatUp.Api.Services;

/// <summary>Loads active items and resolves their effective expiry against the user's local today.</summary>
public class Pantry(EatUpDb db, Catalog catalog, TimeProvider time)
{
    public async Task<UserProfile> GetProfileAsync() => await db.Profiles.SingleAsync();

    public DateOnly Today(UserProfile profile) => Expiry.LocalToday(time.GetUtcNow(), profile.TimeZone);

    public async Task<IReadOnlyList<PantryItem>> GetActiveAsync(UserProfile profile)
    {
        var today = Today(profile);
        var items = await db.Items.Where(i => i.Status == ItemStatus.Active).ToListAsync();
        return items
            .Select(i => new PantryItem(i.Id, i.Name, i.IngredientKey, Expiry.DaysLeft(Expiry.EffectiveExpiry(i, catalog.Find(i.IngredientKey)), today)))
            .OrderBy(i => i.DaysLeft)
            .ToList();
    }

    public async Task<IReadOnlyList<Suggestion>> SuggestAsync(UserProfile profile, int limit = 5) =>
        SuggestionEngine.Suggest(catalog, profile, await GetActiveAsync(profile), limit);
}
