namespace EmployeeDeskBooking.Application.Notifications;

public sealed class BookingEmailDetails
{
    public required Guid BookingId { get; init; }

    public required Guid UserId { get; init; }

    public required string RecipientEmail { get; init; }

    public required string EmployeeName { get; init; }

    public required string DeskNumber { get; init; }

    public required DateOnly BookingDate { get; init; }
}

public interface IBookingEmailService
{
    Task<bool> SendConfirmationAsync(Guid bookingId, CancellationToken cancellationToken = default);

    Task<bool> SendCancellationAsync(Guid bookingId, CancellationToken cancellationToken = default);
}

public interface IReminderEmailService
{
    Task ProcessDueRemindersAsync(CancellationToken cancellationToken = default);
}
