using EatUp.Api.Domain;

namespace EatUp.Tests;

public class WasteStatsTests
{
    private const string Oslo = "Europe/Oslo";
    // 2026-12-01 00:30 in Oslo, still November 30 in UTC.
    private static readonly DateTimeOffset JustAfterMidnightInOslo = new(2026, 11, 30, 23, 30, 0, TimeSpan.Zero);
    private int _nextEventId = 1;

    [Fact]
    public void Months_follow_the_users_time_zone_not_UTC()
    {
        var events = new[]
        {
            Event(1, ConsumeOutcome.Used, new DateTimeOffset(2026, 11, 30, 23, 10, 0, TimeSpan.Zero)), // Dec 1 00:10 in Oslo
            Event(2, ConsumeOutcome.Used, new DateTimeOffset(2026, 11, 30, 22, 50, 0, TimeSpan.Zero)), // Nov 30 23:50 in Oslo
        };

        var inOslo = Summarize(events, JustAfterMidnightInOslo, Oslo);
        var inUtc = Summarize(events, JustAfterMidnightInOslo, "UTC");

        Assert.Equal((2026, 12, 1), (inOslo.ThisMonth.Year, inOslo.ThisMonth.Month, inOslo.ThisMonth.Used));
        Assert.Equal((2026, 11, 1), (inOslo.LastMonth.Year, inOslo.LastMonth.Month, inOslo.LastMonth.Used));
        Assert.Equal((2026, 11, 2), (inUtc.ThisMonth.Year, inUtc.ThisMonth.Month, inUtc.ThisMonth.Used));
    }

    [Fact]
    public void In_January_last_month_is_December_of_the_previous_year()
    {
        var january = new DateTimeOffset(2027, 1, 15, 12, 0, 0, TimeSpan.Zero);
        var events = new[] { Event(1, ConsumeOutcome.Discarded, new DateTimeOffset(2026, 12, 31, 12, 0, 0, TimeSpan.Zero)) };

        var summary = Summarize(events, january, Oslo);

        Assert.Equal((2026, 12, 1), (summary.LastMonth.Year, summary.LastMonth.Month, summary.LastMonth.Discarded));
    }

    [Fact]
    public void An_item_used_in_several_parts_counts_once()
    {
        var milkPours = Enumerable.Range(0, 3).Select(_ => Event(7, ConsumeOutcome.Used, JustAfterMidnightInOslo)).ToList();

        Assert.Equal(1, Summarize(milkPours, JustAfterMidnightInOslo, Oslo).ThisMonth.Used);
    }

    [Fact]
    public void An_item_partly_used_and_partly_thrown_away_counts_in_both()
    {
        var events = new[] { Event(7, ConsumeOutcome.Used, JustAfterMidnightInOslo), Event(7, ConsumeOutcome.Discarded, JustAfterMidnightInOslo) };

        var month = Summarize(events, JustAfterMidnightInOslo, Oslo).ThisMonth;

        Assert.Equal((1, 1), (month.Used, month.Discarded));
    }

    [Fact]
    public void Events_without_an_item_id_count_one_each()
    {
        var events = new[] { Event(null, ConsumeOutcome.Discarded, JustAfterMidnightInOslo), Event(null, ConsumeOutcome.Discarded, JustAfterMidnightInOslo) };

        Assert.Equal(2, Summarize(events, JustAfterMidnightInOslo, Oslo).ThisMonth.Discarded);
    }

    [Fact]
    public void Without_any_events_there_is_no_history()
    {
        var summary = Summarize([], JustAfterMidnightInOslo, Oslo);

        Assert.False(summary.HasHistory);
        Assert.Equal((0, 0, 0, 0), (summary.ThisMonth.Used, summary.ThisMonth.Discarded, summary.LastMonth.Used, summary.LastMonth.Discarded));
    }

    [Theory]
    [InlineData("Europe/Oslo")]
    [InlineData("Pacific/Kiritimati")] // UTC+14
    [InlineData("Pacific/Pago_Pago")] // UTC-11
    public void Loading_starts_no_later_than_the_first_moment_of_last_month(string timeZone)
    {
        var zone = Expiry.ResolveTimeZone(timeZone);
        var firstOfLastMonth = new DateTime(2026, 11, 1, 0, 0, 0, DateTimeKind.Unspecified);
        var firstMomentUtc = TimeZoneInfo.ConvertTimeToUtc(firstOfLastMonth, zone);

        var earliest = WasteStats.EarliestNeeded(JustAfterMidnightInOslo.AddDays(1), timeZone);

        Assert.True(earliest.UtcDateTime <= firstMomentUtc, $"{earliest:u} is after {firstMomentUtc:u}");
        Assert.True(earliest.UtcDateTime >= firstMomentUtc.AddDays(-3), "loads far more history than the two months need");
    }

    // Callers load only recent events, so whether older history exists comes from the caller.
    private static WasteSummary Summarize(IReadOnlyCollection<ConsumeEvent> events, DateTimeOffset utcNow, string timeZone) =>
        WasteStats.Summarize(events, events.Count > 0, utcNow, timeZone);

    private ConsumeEvent Event(int? itemId, ConsumeOutcome outcome, DateTimeOffset at) => new()
    {
        Id = _nextEventId++, FoodItemId = itemId, ItemName = "Milk", Amount = 1, Unit = "l", Outcome = outcome, ConsumedAt = at,
    };
}
