using EmployeeDeskBooking.Domain.Notifications;

namespace EmployeeDeskBooking.Application.Notifications;

public sealed class NotificationPreferenceService(
    INotificationPreferenceRepository preferences) : INotificationPreferenceService
{
    public async Task<NotificationPreferenceState> GetPreferencesAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        var preference = await preferences.GetByUserIdAsync(userId, cancellationToken);
        if (preference is null)
        {
            return new NotificationPreferenceState
            {
                PushOptIn = false,
                HasSubscription = false,
            };
        }

        return new NotificationPreferenceState
        {
            PushOptIn = preference.PushOptIn,
            HasSubscription = !string.IsNullOrWhiteSpace(preference.PushSubscription),
        };
    }

    public async Task OptInAsync(
        Guid userId,
        string pushSubscriptionJson,
        CancellationToken cancellationToken = default)
    {
        if (!PushSubscriptionValidator.IsValid(pushSubscriptionJson))
        {
            throw new ArgumentException("Push subscription JSON is invalid.", nameof(pushSubscriptionJson));
        }

        var now = DateTimeOffset.UtcNow;
        await preferences.UpsertAsync(new NotificationPreference
        {
            UserId = userId,
            PushOptIn = true,
            PushSubscription = pushSubscriptionJson,
            UpdatedAt = now,
        }, cancellationToken);
        await preferences.SaveChangesAsync(cancellationToken);
    }

    public async Task OptOutAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var now = DateTimeOffset.UtcNow;
        await preferences.UpsertAsync(new NotificationPreference
        {
            UserId = userId,
            PushOptIn = false,
            PushSubscription = null,
            UpdatedAt = now,
        }, cancellationToken);
        await preferences.SaveChangesAsync(cancellationToken);
    }
}

internal static class PushSubscriptionValidator
{
    public static bool IsValid(string? json)
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            return false;
        }

        try
        {
            using var document = System.Text.Json.JsonDocument.Parse(json);
            var root = document.RootElement;
            if (!root.TryGetProperty("endpoint", out var endpoint) || endpoint.GetString() is not { Length: > 0 })
            {
                return false;
            }

            if (!root.TryGetProperty("keys", out var keys))
            {
                return false;
            }

            return keys.TryGetProperty("p256dh", out var p256dh) && p256dh.GetString() is { Length: > 0 }
                && keys.TryGetProperty("auth", out var auth) && auth.GetString() is { Length: > 0 };
        }
        catch (System.Text.Json.JsonException)
        {
            return false;
        }
    }
}
