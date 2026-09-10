namespace EmployeeDeskBooking.Web.Models;

public sealed record NotificationSettingsViewModel
{
    public bool PushOptIn { get; init; }

    public bool HasSubscription { get; init; }

    public string? SuccessMessage { get; init; }

    public string? ErrorMessage { get; init; }

    public string VapidPublicKey { get; init; } = string.Empty;
}
