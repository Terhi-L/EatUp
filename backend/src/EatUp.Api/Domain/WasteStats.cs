namespace EatUp.Api.Domain;

/// <summary>Items used and thrown away in one calendar month of the user's time zone.</summary>
public record MonthCounts(int Year, int Month, int Used, int Discarded);

public record WasteSummary(MonthCounts ThisMonth, MonthCounts LastMonth, bool HasHistory);

public static class WasteStats
{
    /// <summary>
    /// The oldest consume time the summary can need. Two days of slack before last month's first day (UTC midnight)
    /// covers every UTC offset without converting a local midnight that a DST change may skip.
    /// </summary>
    public static DateTimeOffset EarliestNeeded(DateTimeOffset utcNow, string timeZone)
    {
        var lastMonth = MonthOf(Expiry.LocalToday(utcNow, Expiry.ResolveTimeZone(timeZone))).AddMonths(-1);
        return new DateTimeOffset(lastMonth.AddDays(-2).ToDateTime(TimeOnly.MinValue), TimeSpan.Zero);
    }

    /// <param name="recentEvents">At least every event since <see cref="EarliestNeeded"/>; older ones are ignored.</param>
    /// <param name="hasHistory">Whether any event exists at all, including ones older than the two months.</param>
    public static WasteSummary Summarize(IReadOnlyCollection<ConsumeEvent> recentEvents, bool hasHistory, DateTimeOffset utcNow, string timeZone)
    {
        var zone = Expiry.ResolveTimeZone(timeZone);
        var thisMonth = MonthOf(Expiry.LocalToday(utcNow, zone));
        var lastMonth = thisMonth.AddMonths(-1);
        var byMonth = recentEvents.ToLookup(e => MonthOf(Expiry.LocalToday(e.ConsumedAt, zone)));
        return new WasteSummary(Count(byMonth[thisMonth], thisMonth), Count(byMonth[lastMonth], lastMonth), hasHistory);
    }

    private static DateOnly MonthOf(DateOnly date) => new(date.Year, date.Month, 1);

    private static MonthCounts Count(IEnumerable<ConsumeEvent> events, DateOnly month) =>
        new(month.Year, month.Month, ItemsTouched(events, ConsumeOutcome.Used), ItemsTouched(events, ConsumeOutcome.Discarded));

    // An item consumed in parts counts once. Events whose item id was cleared by a delete before ids were kept
    // can't be told apart, so each of those counts alone.
    private static int ItemsTouched(IEnumerable<ConsumeEvent> events, ConsumeOutcome outcome) =>
        events.Where(e => e.Outcome == outcome).Select(e => e.FoodItemId is { } id ? (id, false) : (e.Id, true)).Distinct().Count();
}
