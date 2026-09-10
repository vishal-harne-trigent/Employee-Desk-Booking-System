namespace EmployeeDeskBooking.Api.Contracts.Notifications;

public sealed class NotificationPreferencesResponse
{
    public bool PushOptIn { get; init; }

    public bool HasSubscription { get; init; }
}

public sealed class UpdateNotificationPreferencesRequest
{
    public bool PushOptIn { get; init; }
}

public sealed class SavePushSubscriptionRequest
{
    public required PushSubscriptionPayload Subscription { get; init; }
}

public sealed class PushSubscriptionPayload
{
    public required string Endpoint { get; init; }

    public required PushSubscriptionKeys Keys { get; init; }
}

public sealed class PushSubscriptionKeys
{
    public required string P256dh { get; init; }

    public required string Auth { get; init; }
}
