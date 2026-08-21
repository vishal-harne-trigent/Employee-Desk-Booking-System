using EmployeeDeskBooking.Application.Bookings;
using EmployeeDeskBooking.Domain.Bookings;

namespace EmployeeDeskBooking.Web.Areas.Admin.Models;

public class AdminBookingsViewModel
{
    public DateOnly? FilterDate { get; set; }

    public BookingStatus? FilterStatus { get; set; }

    public bool FiltersApplied { get; set; }

    public IReadOnlyList<AdminBookingRowViewModel> Bookings { get; set; } = Array.Empty<AdminBookingRowViewModel>();

    public string? SuccessMessage { get; set; }

    public string? ErrorMessage { get; set; }

    public static string CancelErrorMessage(CancelBookingFailureReason reason) =>
        reason switch
        {
            CancelBookingFailureReason.NotFound => "That booking was not found.",
            CancelBookingFailureReason.NotCancellable =>
                "This booking cannot be cancelled. Only confirmed bookings for today or future dates can be cancelled.",
            _ => "Unable to cancel the booking. Please try again.",
        };
}

public sealed class AdminBookingRowViewModel
{
    public required Guid BookingId { get; init; }

    public required DateOnly BookingDate { get; init; }

    public required string DeskNumber { get; init; }

    public required string EmployeeEmail { get; init; }

    public required string EmployeeName { get; init; }

    public required BookingStatus Status { get; init; }

    public required bool CanCancel { get; init; }

    public string StatusLabel => Status.ToString();
}
