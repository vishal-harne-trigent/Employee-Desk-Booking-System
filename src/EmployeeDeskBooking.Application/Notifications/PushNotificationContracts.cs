namespace EmployeeDeskBooking.Application.Notifications;

public sealed class PushNotificationMessage
{
    public required string SubscriptionJson { get; init; }

    public required string Title { get; init; }

    public required string Body { get; init; }
}

public interface IPushNotificationSender
{
    Task SendAsync(PushNotificationMessage message, CancellationToken cancellationToken = default);
}

public sealed class NotificationPreferenceState
{
    public required bool PushOptIn { get; init; }

    public required bool HasSubscription { get; init; }
}

public interface INotificationPreferenceRepository
{
    Task<Domain.Notifications.NotificationPreference?> GetByUserIdAsync(
        Guid userId,
        CancellationToken cancellationToken = default);

    Task UpsertAsync(
        Domain.Notifications.NotificationPreference preference,
        CancellationToken cancellationToken = default);

    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}

public interface INotificationPreferenceService
{
    Task<NotificationPreferenceState> GetPreferencesAsync(
        Guid userId,
        CancellationToken cancellationToken = default);

    Task OptInAsync(
        Guid userId,
        string pushSubscriptionJson,
        CancellationToken cancellationToken = default);

    Task OptOutAsync(Guid userId, CancellationToken cancellationToken = default);
}

public interface IBookingPushService
{
    Task SendConfirmationAsync(Guid bookingId, CancellationToken cancellationToken = default);

    Task SendCancellationAsync(Guid bookingId, CancellationToken cancellationToken = default);
}
