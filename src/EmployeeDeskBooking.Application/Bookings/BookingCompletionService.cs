using EmployeeDeskBooking.Application.Time;
using EmployeeDeskBooking.Domain.Bookings;

namespace EmployeeDeskBooking.Application.Bookings;

public sealed class BookingCompletionService(
    IBookingRepository bookings,
    IOfficeClock officeClock) : IBookingCompletionService
{
    public async Task<int> CompletePastBookingsAsync(CancellationToken cancellationToken = default)
    {
        var today = officeClock.Today;
        var dueBookings = await bookings.GetConfirmedBookingsBeforeDateAsync(today, cancellationToken);

        if (dueBookings.Count == 0)
        {
            return 0;
        }

        var now = DateTimeOffset.UtcNow;
        foreach (var booking in dueBookings)
        {
            booking.Status = BookingStatus.Completed;
            booking.CompletedAt = now;
            booking.UpdatedAt = now;
        }

        await bookings.SaveChangesAsync(cancellationToken);
        return dueBookings.Count;
    }
}
