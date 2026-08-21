namespace EmployeeDeskBooking.Application.Bookings;

public enum BookingDateValidationError
{
    BeforeToday,
    BeyondWindow,
    Weekend,
}
