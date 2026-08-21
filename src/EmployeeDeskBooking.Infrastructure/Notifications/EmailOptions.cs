namespace EmployeeDeskBooking.Infrastructure.Notifications;

public sealed class EmailOptions
{
    public bool Enabled { get; set; }

    public string FromAddress { get; set; } = "noreply@deskbooking.local";

    public string FromName { get; set; } = "Desk Booking";

    public string SmtpHost { get; set; } = "localhost";

    public int SmtpPort { get; set; } = 25;

    public bool UseStartTls { get; set; }

    public string? Username { get; set; }

    public string? Password { get; set; }

    public int ReminderHourLocal { get; set; } = 8;
}
