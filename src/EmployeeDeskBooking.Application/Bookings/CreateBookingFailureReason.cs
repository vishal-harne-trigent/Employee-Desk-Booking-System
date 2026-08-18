namespace EmployeeDeskBooking.Application.Bookings;

public enum CreateBookingFailureReason
{
    InvalidDate,
    UserAlreadyBooked,
    DeskNotFound,
    DeskInactive,
    DeskAlreadyBooked,
    ConcurrencyConflict,
}
