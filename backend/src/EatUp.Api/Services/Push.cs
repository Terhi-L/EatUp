using System.Text.Json;
using EatUp.Api.Domain;
using WebPush;

namespace EatUp.Api.Services;

public record VapidKeys(string PublicKey, string PrivateKey);

public interface IPushSender
{
    /// <returns>false when the push service says the subscription is gone and should be deleted.</returns>
    Task<bool> SendAsync(PushSubscriptionRecord subscription, object payload, CancellationToken ct = default);
}

public class WebPushSender(VapidKeys keys, ILogger<WebPushSender> logger) : IPushSender
{
    private readonly WebPushClient _client = new();
    private readonly VapidDetails _vapid = new("mailto:dev@eatup.local", keys.PublicKey, keys.PrivateKey);

    public async Task<bool> SendAsync(PushSubscriptionRecord s, object payload, CancellationToken ct = default)
    {
        try
        {
            var json = JsonSerializer.Serialize(payload, JsonSerializerOptions.Web);
            await _client.SendNotificationAsync(new PushSubscription(s.Endpoint, s.P256dh, s.Auth), json, _vapid, ct);
            return true;
        }
        catch (WebPushException e) when (e.StatusCode is System.Net.HttpStatusCode.NotFound or System.Net.HttpStatusCode.Gone)
        {
            logger.LogInformation("Push subscription {Id} expired; removing it", s.Id);
            return false;
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
