namespace EmployeeDeskBooking.Application.Bookings;

public interface IBookingCompletionService
{
    Task<int> CompletePastBookingsAsync(CancellationToken cancellationToken = default);
}
