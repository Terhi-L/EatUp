using EatUp.Api.Data;
using EatUp.Api.Domain;
using Microsoft.EntityFrameworkCore;

namespace EatUp.Api.Services;

public record DailyCheckResult(NotificationMessage? Message, int Sent);

/// <summary>Sends the daily "use it up" notification to every subscribed device when it's due.</summary>
public class DailyCheck(EatUpDb db, Pantry pantry, Catalog catalog, IPushSender push, TimeProvider time)
{
    public async Task<DailyCheckResult> RunAsync(bool force = false, CancellationToken ct = default)
    {
        var profile = await pantry.GetProfileAsync();
        var items = await pantry.GetActiveAsync(profile);
        var top = SuggestionEngine.Suggest(catalog, profile, items, limit: 1).FirstOrDefault();

        var message = DailyNotification.Decide(profile, items, top?.Recipe.Title, time.GetUtcNow(), force);
        if (message is null) return new DailyCheckResult(null, 0);

        var sent = await SendToAllAsync(new { title = message.Title, body = message.Body, url = message.Url }, ct);
        profile.LastNotifiedOn = message.LocalDate;
        await db.SaveChangesAsync(ct);
        return new DailyCheckResult(message, sent);
    }

    public async Task<int> SendToAllAsync(object payload, CancellationToken ct = default)
    {
        var sent = 0;
        foreach (var sub in await db.PushSubscriptions.ToListAsync(ct))
        {
            if (await push.SendAsync(sub, payload, ct)) sent++;
            else db.PushSubscriptions.Remove(sub);
        }
        await db.SaveChangesAsync(ct);
        return sent;
    }
}

public class DailyNotificationJob(IServiceScopeFactory scopes, IConfiguration config, ILogger<DailyNotificationJob> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken ct)
    {
        var interval = TimeSpan.FromMinutes(config.GetValue("Push:CheckIntervalMinutes", 5));
        using var timer = new PeriodicTimer(interval);
        do
        {
            try
            {
                using var scope = scopes.CreateScope();
                var result = await scope.ServiceProvider.GetRequiredService<DailyCheck>().RunAsync(ct: ct);
                if (result.Message is not null)
                    logger.LogInformation("Daily notification sent to {Count} device(s): {Title}", result.Sent, result.Message.Title);
            }
            catch (Exception e) when (e is not OperationCanceledException)
            {
                logger.LogError(e, "Daily notification check failed");
            }
        } while (await timer.WaitForNextTickAsync(ct));
    }
}
