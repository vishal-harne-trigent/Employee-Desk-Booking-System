namespace EmployeeDeskBooking.Domain.Bookings;

public class Booking
{
    public Guid Id { get; set; }

    public Guid UserId { get; set; }

    public Guid DeskId { get; set; }

    public DateOnly BookingDate { get; set; }

    public BookingStatus Status { get; set; }

    public DateTimeOffset? CancelledAt { get; set; }

    public Guid? CancelledById { get; set; }

    public DateTimeOffset? CompletedAt { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset UpdatedAt { get; set; }
}
