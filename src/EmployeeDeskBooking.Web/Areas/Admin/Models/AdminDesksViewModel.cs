using EmployeeDeskBooking.Application.Desks;
using EmployeeDeskBooking.Domain.Desks;

namespace EmployeeDeskBooking.Web.Areas.Admin.Models;

public class AdminDesksViewModel
{
    public IReadOnlyList<AdminDeskRowViewModel> Desks { get; set; } = Array.Empty<AdminDeskRowViewModel>();

    public string? SuccessMessage { get; set; }

    public string? ErrorMessage { get; set; }

    public static string ErrorMessageFor(DeskOperationFailureReason reason) =>
        reason switch
        {
            DeskOperationFailureReason.NotFound => "That desk was not found.",
            DeskOperationFailureReason.DuplicateDeskNumber =>
                "Desk number is already in use. Choose a unique desk number.",
            DeskOperationFailureReason.HasFutureBookings =>
                "This desk has confirmed bookings for today or future dates. Cancel those bookings in All Bookings before deactivating.",
            _ => "Unable to complete the desk operation. Please try again.",
        };
}

public sealed class AdminDeskRowViewModel
{
    public required Guid DeskId { get; init; }

    public required string DeskNumber { get; init; }

    public required DeskStatus Status { get; init; }

    public required bool CanDeactivate { get; init; }

    public string StatusLabel => Status.ToString();
}
