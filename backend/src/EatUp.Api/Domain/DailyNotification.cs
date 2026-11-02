namespace EatUp.Api.Domain;

public record NotificationMessage(string Title, string Body, string Url, DateOnly LocalDate);

public static class DailyNotification
{
    /// <summary>
    /// Decides whether the daily "use it up" notification should be sent now.
    /// Sends at most once per local day, at or after the user's chosen time, and only when something is urgent.
    /// </summary>
    /// <param name="force">Skip the enabled / time-of-day / once-per-day checks (used by the dev "run now" button).</param>
    public static NotificationMessage? Decide(
        UserProfile profile, IReadOnlyList<PantryItem> items, string? topSuggestion, DateTimeOffset utcNow, bool force = false)
    {
        var localNow = Expiry.LocalNow(utcNow, profile.TimeZone);
        var today = DateOnly.FromDateTime(localNow);

        if (!force)
        {
            if (!profile.NotificationsEnabled) return null;
            if (TimeOnly.FromDateTime(localNow) < profile.NotificationTime) return null;
            if (profile.LastNotifiedOn >= today) return null;
        }

        var urgent = items.Where(i => Expiry.IsUrgent(i.DaysLeft)).OrderBy(i => i.DaysLeft).ToList();
        if (urgent.Count == 0) return null;

        var first = urgent[0];
        var title = $"{first.Name} {(IsPlural(first.Name) ? "expire" : "expires")} {When(first.DaysLeft)}";
        if (urgent.Count > 1) title += $" (+{urgent.Count - 1} more)";

        var body = topSuggestion is null
            ? "Open EatUp to see what you can make."
            : $"Cook idea: {topSuggestion}";

        return new NotificationMessage(title, body, "/cook", today);
    }

    private static string When(int daysLeft) => daysLeft switch
    {
        0 => "today",
        1 => "tomorrow",
        _ => $"in {daysLeft} days",
    };

    // Good enough for food names: "Eggs" expire, "Milk" / "Hummus" / "Cress" expires.
    private static bool IsPlural(string name)
    {
        var n = name.Trim().ToLowerInvariant();
        return n.EndsWith('s') && !n.EndsWith("ss") && !n.EndsWith("us");
    }
}
