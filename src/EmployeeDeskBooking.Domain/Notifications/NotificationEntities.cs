namespace EmployeeDeskBooking.Domain.Notifications;

public enum EmailType : byte
{
    Confirmation = 0,
    Cancellation = 1,
    Reminder = 2,
}

public enum EmailDeliveryStatus : byte
{
    Sent = 0,
    Failed = 1,
}

public class EmailDeliveryLog
{
    public Guid Id { get; set; }

    public Guid? BookingId { get; set; }

    public Guid? UserId { get; set; }

    public EmailType EmailType { get; set; }

    public string Recipient { get; set; } = string.Empty;

    public EmailDeliveryStatus Status { get; set; }

    public string? ErrorMessage { get; set; }

    public DateTimeOffset CreatedAt { get; set; }
}

public class BookingReminder
{
    public Guid BookingId { get; set; }

    public DateTimeOffset SentAt { get; set; }

    public DateTimeOffset CreatedAt { get; set; }
}
