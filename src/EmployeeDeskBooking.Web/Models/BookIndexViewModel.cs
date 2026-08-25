using System.Globalization;
using EmployeeDeskBooking.Application.Bookings;
using EmployeeDeskBooking.Application.Desks;

namespace EmployeeDeskBooking.Web.Models;

public class BookIndexViewModel
{
    public DateOnly SelectedDate { get; set; }

    public DateOnly MinBookingDate { get; set; }

    public DateOnly MaxBookingDate { get; set; }

    public bool AvailabilityRequested { get; set; }

    public string? DateError { get; set; }

    public string? BookingError { get; set; }

    public string? SuccessMessage { get; set; }

    public string? EmailWarning { get; set; }

    public ExistingBookingRowViewModel? ExistingBooking { get; set; }

    public bool HasExistingBooking => ExistingBooking is not null;

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

    public static string BookingConfirmedMessage(string deskNumber, DateOnly bookingDate) =>
        BookingConfirmedMessage(deskNumber, location: null, bookingDate);

    public static string BookingConfirmedMessage(string deskNumber, string? location, DateOnly bookingDate)
    {
        var deskLabel = DeskLocationFormatter.FormatDeskWithLocation(deskNumber, location);
        var dateLabel = bookingDate.ToString("dddd, MMMM d, yyyy", CultureInfo.GetCultureInfo("en-US"));
        return $"Your desk booking is confirmed for {deskLabel} on {dateLabel}.";
    }
}

public sealed class DeskRowViewModel
{
    public Guid DeskId { get; init; }

    public required string DeskNumber { get; init; }

    public required string Location { get; init; }

    public bool IsAvailable { get; init; }
}

public sealed class ExistingBookingRowViewModel
{
    public required string DeskNumber { get; init; }

    public required string Location { get; init; }

    public DateOnly BookingDate { get; init; }

    public string DeskLabel => DeskLocationFormatter.FormatDeskWithLocation(DeskNumber, Location);

    public string BookingDateLabel =>
        BookingDate.ToString("dddd, MMMM d, yyyy", CultureInfo.GetCultureInfo("en-US"));
}
