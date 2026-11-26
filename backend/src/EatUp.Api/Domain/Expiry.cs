namespace EatUp.Api.Domain;

/// <summary>An item's expiry resolved against today: when it runs out, how urgent it is and what to do first.</summary>
public record Freshness(DateOnly EffectiveExpiry, int DaysLeft, Urgency Urgency, ExpiryAdvice Advice);

public static class Expiry
{
    /// <summary>Items expiring within this many days count as urgent.</summary>
    public const int SoonDays = 2;

    /// <summary>Days past its date that best-before food without a catalogue window (unknown, or switched from use by) is worth checking.</summary>
    public const int FallbackCheckDays = 2;

    /// <summary>An opened item expires at whichever comes first: the printed date or opened date + opened shelf life.</summary>
    public static DateOnly EffectiveExpiry(FoodItem item, Ingredient? ingredient)
    {
        if (item.OpenedDate is { } opened && ingredient?.OpenedShelfLifeDays is { } days)
        {
            var openedExpiry = opened.AddDays(days);
            return openedExpiry < item.ExpiryDate ? openedExpiry : item.ExpiryDate;
        }
        return item.ExpiryDate;
    }

    public static int DaysLeft(DateOnly expiry, DateOnly today) => expiry.DayNumber - today.DayNumber;

    /// <summary>
    /// Best-before food past its date but inside its window is to eat today after a look, smell and taste; anything else
    /// past its date is to throw out, and food the catalogue doesn't know asks for a look at the package first.
    /// </summary>
    public static Freshness FreshnessOf(FoodItem item, Ingredient? ingredient, DateOnly today)
    {
        var expiry = EffectiveExpiry(item, ingredient);
        var daysLeft = DaysLeft(expiry, today);
        if (daysLeft >= 0) return new(expiry, daysLeft, UrgencyFor(daysLeft), ExpiryAdvice.None);
        if (-daysLeft <= DaysWorthChecking(item, ingredient, expiry)) return new(expiry, daysLeft, Urgency.Today, ExpiryAdvice.LookSmellTaste);
        return new(expiry, daysLeft, Urgency.Expired, AdviceBeforeThrowingOut(item, ingredient));
    }

    private static int DaysWorthChecking(FoodItem item, Ingredient? ingredient, DateOnly effectiveExpiry)
    {
        if (item.ExpiryKind != ExpiryKind.BestBefore) return 0;
        // The window is for the printed date on a closed package, not for "use within N days of opening".
        if (effectiveExpiry < item.ExpiryDate) return 0;
        if (ingredient?.CheckDaysPastBestBefore is not { } byLocation) return FallbackCheckDays;
        return byLocation.GetValueOrDefault(item.Location);
    }

    // Unknown food only counts as use by until someone reads the package, which may say best before.
    private static ExpiryAdvice AdviceBeforeThrowingOut(FoodItem item, Ingredient? ingredient) =>
        ingredient is null && item.ExpiryKind == ExpiryKind.UseBy ? ExpiryAdvice.CheckPackage : ExpiryAdvice.None;

    public static Urgency UrgencyFor(int daysLeft) => daysLeft switch
    {
        < 0 => Urgency.Expired,
        0 => Urgency.Today,
        <= SoonDays => Urgency.Soon,
        _ => Urgency.Later,
    };

    public static bool IsUrgent(int daysLeft) => daysLeft is >= 0 and <= SoonDays;

    /// <summary>Default expiry for a catalogue ingredient stored in a location, or null if unknown.</summary>
    public static DateOnly? DefaultExpiry(Ingredient ingredient, StorageLocation location, DateOnly today) =>
        ingredient.ShelfLifeDays.TryGetValue(location, out var days) ? today.AddDays(days) : null;

    public static TimeZoneInfo ResolveTimeZone(string id)
    {
        try { return TimeZoneInfo.FindSystemTimeZoneById(id); }
        catch (Exception e) when (e is TimeZoneNotFoundException or InvalidTimeZoneException) { return TimeZoneInfo.Utc; }
    }

    public static DateTime LocalNow(DateTimeOffset utcNow, string timeZone) =>
        TimeZoneInfo.ConvertTime(utcNow, ResolveTimeZone(timeZone)).DateTime;

    public static DateOnly LocalToday(DateTimeOffset utcNow, string timeZone) =>
        LocalToday(utcNow, ResolveTimeZone(timeZone));

    public static DateOnly LocalToday(DateTimeOffset utcNow, TimeZoneInfo zone) =>
        DateOnly.FromDateTime(TimeZoneInfo.ConvertTime(utcNow, zone).DateTime);
}
