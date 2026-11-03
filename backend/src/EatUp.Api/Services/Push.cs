using System.Net;
using System.Text.Json;
using EatUp.Api.Domain;
using WebPush;

namespace EatUp.Api.Services;

public record VapidKeys(string PublicKey, string PrivateKey);

public enum PushResult
{
    Sent,
    /// <summary>The push service says the subscription is invalid (expired, or tied to other VAPID keys); delete it.</summary>
    Gone,
    /// <summary>Delivery failed for another reason; keep the subscription.</summary>
    Failed,
}

public interface IPushSender
{
    Task<PushResult> SendAsync(PushSubscriptionRecord subscription, object payload, CancellationToken ct = default);
}

public class WebPushSender(VapidKeys keys, ILogger<WebPushSender> logger) : IPushSender
{
    private readonly WebPushClient _client = new();
    private readonly VapidDetails _vapid = new("mailto:dev@eatup.local", keys.PublicKey, keys.PrivateKey);

    public async Task<PushResult> SendAsync(PushSubscriptionRecord s, object payload, CancellationToken ct = default)
    {
        try
        {
            var json = JsonSerializer.Serialize(payload, JsonSerializerOptions.Web);
            await _client.SendNotificationAsync(new PushSubscription(s.Endpoint, s.P256dh, s.Auth), json, _vapid, ct);
            return PushResult.Sent;
        }
        catch (WebPushException e) when (e.StatusCode is HttpStatusCode.NotFound or HttpStatusCode.Gone
                                              or HttpStatusCode.Unauthorized or HttpStatusCode.Forbidden)
        {
            logger.LogInformation("Push subscription {Id} is no longer valid ({Status}); removing it", s.Id, (int)e.StatusCode);
            return PushResult.Gone;
        }
        catch (Exception e) when (e is WebPushException or HttpRequestException or ArgumentException)
        {
            logger.LogWarning(e, "Push to subscription {Id} failed", s.Id);
            return PushResult.Failed;
        }
    }
}

public static class VapidKeyStore
{
    /// <summary>Uses Push:PublicKey/PrivateKey from config if set, otherwise generates keys once into a git-ignored file.</summary>
    public static VapidKeys LoadOrCreate(IConfiguration config, string dataDir, ILogger logger)
    {
        if (config["Push:PublicKey"] is { Length: > 0 } pub && config["Push:PrivateKey"] is { Length: > 0 } priv)
            return new VapidKeys(pub, priv);

        var path = Path.Combine(dataDir, "vapid.json");
        if (File.Exists(path))
            return JsonSerializer.Deserialize<VapidKeys>(File.ReadAllText(path))!;

        var generated = VapidHelper.GenerateVapidKeys();
        var keys = new VapidKeys(generated.PublicKey, generated.PrivateKey);
        File.WriteAllText(path, JsonSerializer.Serialize(keys));
        logger.LogInformation("Generated Web Push VAPID keys in {Path}", path);
        return keys;
    }
}
