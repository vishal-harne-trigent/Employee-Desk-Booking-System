using EmployeeDeskBooking.Application.Bookings;

namespace EmployeeDeskBooking.Web.Models;

public class BookIndexViewModel
{
    public DateOnly SelectedDate { get; set; }

    public bool AvailabilityRequested { get; set; }

    public string? DateError { get; set; }

    public string? BookingError { get; set; }

    public string? SuccessMessage { get; set; }

    public string? ExistingBookingDeskNumber { get; set; }

    public IReadOnlyList<DeskRowViewModel> Desks { get; set; } = Array.Empty<DeskRowViewModel>();

    public static string DateErrorMessage(BookingDateValidationError error) =>
        error switch
        {
            BookingDateValidationError.BeforeToday => "Choose today or a future date within the booking window.",
            BookingDateValidationError.BeyondWindow => "You can only book up to 30 calendar days ahead.",
            BookingDateValidationError.Weekend => "Desks can only be booked on working days (Monday–Friday).",
            _ => "The selected date is not valid.",
        };

    public static string BookingErrorMessage(CreateBookingFailureReason reason) =>
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
}

public sealed class DeskRowViewModel
{
    public Guid DeskId { get; init; }

    public required string DeskNumber { get; init; }

    public bool IsAvailable { get; init; }
}
