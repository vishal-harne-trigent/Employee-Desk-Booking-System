using EmployeeDeskBooking.Application.Notifications;

namespace EmployeeDeskBooking.Tests;

public sealed class InMemoryPushNotificationSender : IPushNotificationSender
{
    public List<PushNotificationMessage> Sent { get; } = [];

    public Task SendAsync(PushNotificationMessage message, CancellationToken cancellationToken = default)
    {
        Sent.Add(message);
        return Task.CompletedTask;
    }

    public void Reset() => Sent.Clear();
}

public static class PushTestData
{
    public const string SampleSubscriptionJson =
        """{"endpoint":"https://push.test/send/abc","keys":{"p256dh":"test-key","auth":"test-auth"}}""";
}
