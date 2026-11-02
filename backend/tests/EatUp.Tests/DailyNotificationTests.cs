using EatUp.Api.Domain;
using static EatUp.Tests.TestCatalog;

namespace EatUp.Tests;

public class DailyNotificationTests
{
    private static UserProfile Oslo(DateOnly? lastNotified = null) => new()
    {
        TimeZone = "Europe/Oslo", NotificationTime = new TimeOnly(9, 0), LastNotifiedOn = lastNotified,
    };

    private static readonly PantryItem[] EggsTomorrow = [new(1, "Eggs", "egg", 1)];

    // 2 Nov 2026: Oslo is UTC+1, so 09:00 local = 08:00 UTC.
    private static DateTimeOffset Utc(int month, int day, int hour, int minute = 0) => new(2026, month, day, hour, minute, 0, TimeSpan.Zero);

    [Fact]
    public void Not_sent_before_the_users_notification_time() =>
        Assert.Null(DailyNotification.Decide(Oslo(), EggsTomorrow, "Omelette", Utc(11, 2, 7, 59)));

    [Fact]
    public void Sent_at_the_users_notification_time_with_a_suggestion()
    {
        var message = DailyNotification.Decide(Oslo(), EggsTomorrow, "Veggie omelette", Utc(11, 2, 8, 0));

        Assert.NotNull(message);
        Assert.Equal("Eggs expire tomorrow", message.Title);
        Assert.Equal("Cook idea: Veggie omelette", message.Body);
        Assert.Equal("/cook", message.Url);
        Assert.Equal(new DateOnly(2026, 11, 2), message.LocalDate);
    }

    [Fact]
    public void Sent_at_most_once_per_local_day()
    {
        Assert.Null(DailyNotification.Decide(Oslo(lastNotified: new DateOnly(2026, 11, 2)), EggsTomorrow, null, Utc(11, 2, 15)));
        Assert.NotNull(DailyNotification.Decide(Oslo(lastNotified: new DateOnly(2026, 11, 1)), EggsTomorrow, null, Utc(11, 2, 15)));
    }

    [Fact]
    public void Not_sent_when_nothing_is_urgent() =>
        Assert.Null(DailyNotification.Decide(Oslo(), [Item("egg", 3), Item("milk", -1)], null, Utc(11, 2, 12)));

    [Fact]
    public void Not_sent_when_notifications_are_disabled()
    {
        var profile = Oslo();
        profile.NotificationsEnabled = false;

        Assert.Null(DailyNotification.Decide(profile, EggsTomorrow, null, Utc(11, 2, 12)));
    }

    [Fact]
    public void Force_ignores_time_and_once_per_day_but_still_needs_something_urgent()
    {
        Assert.NotNull(DailyNotification.Decide(Oslo(lastNotified: new DateOnly(2026, 11, 2)), EggsTomorrow, null, Utc(11, 2, 3), force: true));
        Assert.Null(DailyNotification.Decide(Oslo(), [], null, Utc(11, 2, 12), force: true));
    }

    [Fact]
    public void Respects_daylight_saving_change()
    {
        // 29 Mar 2026 Oslo switches to UTC+2, so 09:00 local = 07:00 UTC.
        Assert.Null(DailyNotification.Decide(Oslo(), EggsTomorrow, null, Utc(3, 29, 6, 59)));
        Assert.NotNull(DailyNotification.Decide(Oslo(), EggsTomorrow, null, Utc(3, 29, 7, 0)));
    }

    [Fact]
    public void Different_time_zones_get_notified_at_their_own_local_time()
    {
        var tokyo = new UserProfile { TimeZone = "Asia/Tokyo", NotificationTime = new TimeOnly(9, 0) };

        // 00:00 UTC = 09:00 in Tokyo, but 01:00 in Oslo.
        Assert.NotNull(DailyNotification.Decide(tokyo, EggsTomorrow, null, Utc(11, 2, 0)));
        Assert.Null(DailyNotification.Decide(Oslo(), EggsTomorrow, null, Utc(11, 2, 0)));
    }

    [Fact]
    public void Title_names_the_most_urgent_item_and_counts_the_rest()
    {
        PantryItem[] items = [new(1, "Eggs", "egg", 2), new(2, "Milk", "milk", 0), new(3, "Spinach", "spinach", 1)];

        var message = DailyNotification.Decide(Oslo(), items, null, Utc(11, 2, 12));

        Assert.Equal("Milk expires today (+2 more)", message!.Title);
    }
}
