using EmployeeDeskBooking.Domain.Bookings;

namespace EmployeeDeskBooking.Application.Bookings;

public enum CancelBookingFailureReason
{
    NotFound,
    NotCancellable,
}

public sealed class CancelBookingResult
{
    public bool Succeeded { get; init; }

    public CancelBookingFailureReason? FailureReason { get; init; }

    public bool EmailNotificationSent { get; init; }

    public static CancelBookingResult Success(bool emailNotificationSent) =>
        new() { Succeeded = true, EmailNotificationSent = emailNotificationSent };

    public static CancelBookingResult Failure(CancelBookingFailureReason reason) =>
        new() { Succeeded = false, FailureReason = reason };
}

public sealed class MyBookingItem
{
    public required Guid BookingId { get; init; }

    public required DateOnly BookingDate { get; init; }

    public required string DeskNumber { get; init; }

    public required BookingStatus Status { get; init; }

    public required bool CanCancel { get; init; }
}
