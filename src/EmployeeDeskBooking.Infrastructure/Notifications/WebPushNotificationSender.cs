using System.Text.Json;
using EmployeeDeskBooking.Application.Notifications;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using WebPush;

namespace EmployeeDeskBooking.Infrastructure.Notifications;

public sealed class WebPushNotificationSender(
    IOptions<VapidOptions> options,
    ILogger<WebPushNotificationSender> logger) : IPushNotificationSender
{
    public async Task SendAsync(PushNotificationMessage message, CancellationToken cancellationToken = default)
    {
        var settings = options.Value;
        using var document = JsonDocument.Parse(message.SubscriptionJson);
        var root = document.RootElement;
        var endpoint = root.GetProperty("endpoint").GetString()
            ?? throw new InvalidOperationException("Push subscription endpoint was missing.");
        var keys = root.GetProperty("keys");
        var p256dh = keys.GetProperty("p256dh").GetString()
            ?? throw new InvalidOperationException("Push subscription p256dh key was missing.");
        var auth = keys.GetProperty("auth").GetString()
            ?? throw new InvalidOperationException("Push subscription auth key was missing.");

        var subscription = new PushSubscription(endpoint, p256dh, auth);
        var payload = JsonSerializer.Serialize(new { title = message.Title, body = message.Body });
        var vapid = new VapidDetails(settings.Subject, settings.PublicKey, settings.PrivateKey);
        var client = new WebPushClient();

        await client.SendNotificationAsync(subscription, payload, vapid);
        logger.LogInformation("Push notification sent to {Endpoint}", endpoint);
    }
}

public sealed class NoOpPushNotificationSender(ILogger<NoOpPushNotificationSender> logger) : IPushNotificationSender
{
    public Task SendAsync(PushNotificationMessage message, CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Push disabled — skipped send with title {Title}", message.Title);
        return Task.CompletedTask;
    }
}
