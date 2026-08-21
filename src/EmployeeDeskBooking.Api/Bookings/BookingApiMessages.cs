using EmployeeDeskBooking.Application.Bookings;

namespace EmployeeDeskBooking.Api.Bookings;

public static class BookingApiMessages
{
    public static string DateError(BookingDateValidationError error) =>
        error switch
        {
            BookingDateValidationError.BeforeToday => "Choose today or a future date within the booking window.",
            BookingDateValidationError.BeyondWindow => "You can only book up to 30 calendar days ahead.",
            BookingDateValidationError.Weekend => "Desks can only be booked on working days (Monday–Friday).",
            _ => "The selected date is not valid.",
        };

    public static string CreateFailure(CreateBookingFailureReason reason) =>
        reason switch
        {
            CreateBookingFailureReason.InvalidDate => "The selected date is not valid for booking.",
            CreateBookingFailureReason.UserAlreadyBooked =>
                "You already have a desk booked for this date. Cancel it from My Bookings first.",
            CreateBookingFailureReason.DeskInactive => "That desk is not available for booking.",
            CreateBookingFailureReason.DeskAlreadyBooked => "That desk is already booked for this date.",
            CreateBookingFailureReason.ConcurrencyConflict =>
                "Someone else just booked that desk. Please choose another desk.",
            _ => "Unable to complete the booking. Please try again.",
        };

    public static string CancelFailure(CancelBookingFailureReason reason) =>
        reason switch
        {
            CancelBookingFailureReason.NotFound => "Booking was not found.",
            CancelBookingFailureReason.NotCancellable =>
                "This booking cannot be cancelled. Only confirmed bookings for today or future dates can be cancelled.",
            _ => "Unable to cancel the booking.",
        };
}
