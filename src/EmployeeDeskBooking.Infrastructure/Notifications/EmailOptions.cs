namespace EmployeeDeskBooking.Infrastructure.Notifications;

public sealed class EmailOptions
{
    public const string DeliveryModeSmtp = "Smtp";

    public const string DeliveryModeFileDrop = "FileDrop";

    public bool Enabled { get; set; }

    public string DeliveryMode { get; set; } = DeliveryModeSmtp;

    public string FileDropPath { get; set; } = "App_Data/sent-emails";

    public string FromAddress { get; set; } = "noreply@deskbooking.local";

    public string FromName { get; set; } = "Desk Booking";

    public string SmtpHost { get; set; } = "localhost";

    public int SmtpPort { get; set; } = 25;

    public bool UseStartTls { get; set; }

    public string? Username { get; set; }

    public string? Password { get; set; }

    public int ReminderHourLocal { get; set; } = 8;

    public bool UsesFileDrop => string.Equals(DeliveryMode, DeliveryModeFileDrop, StringComparison.OrdinalIgnoreCase);

    public bool HasConfiguredSmtpCredentials =>
        !string.IsNullOrWhiteSpace(Username)
        && !string.IsNullOrWhiteSpace(Password)
        && !Password.Contains("REPLACE_WITH", StringComparison.OrdinalIgnoreCase)
        && !Password.Contains("PASTE_", StringComparison.OrdinalIgnoreCase);
}
