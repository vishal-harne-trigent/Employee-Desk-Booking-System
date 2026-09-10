using EmployeeDeskBooking.Application.Desks;

namespace EmployeeDeskBooking.Api.Desks;

public static class DeskApiMessages
{
    public static string Failure(DeskOperationFailureReason reason) =>
        reason switch
        {
            DeskOperationFailureReason.NotFound => "Desk was not found.",
            DeskOperationFailureReason.DuplicateDeskNumber =>
                "Desk number is already in use. Choose a unique desk number.",
            DeskOperationFailureReason.HasFutureBookings =>
                "This desk has confirmed bookings for today or future dates. Cancel those bookings before deactivating.",
            _ => "Unable to complete the desk operation.",
        };
}
