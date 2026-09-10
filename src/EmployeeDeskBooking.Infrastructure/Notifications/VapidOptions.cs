namespace EmployeeDeskBooking.Infrastructure.Notifications;

public sealed class VapidOptions
{
    public bool Enabled { get; set; }

    public string Subject { get; set; } = "mailto:noreply@deskbooking.local";

    public string PublicKey { get; set; } = string.Empty;

    public string PrivateKey { get; set; } = string.Empty;
}
