namespace EatUp.Api.Domain;

public static class Expiry
{
    /// <summary>Items expiring within this many days count as urgent.</summary>
    public const int SoonDays = 2;

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
        DateOnly.FromDateTime(LocalNow(utcNow, timeZone));
}
