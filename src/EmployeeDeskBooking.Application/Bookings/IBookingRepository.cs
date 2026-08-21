using EmployeeDeskBooking.Application.Bookings;
using EmployeeDeskBooking.Domain.Bookings;
using EmployeeDeskBooking.Domain.Desks;

namespace EmployeeDeskBooking.Application.Bookings;

public interface IBookingRepository
{
    Task<IReadOnlyList<Desk>> GetActiveDesksAsync(CancellationToken cancellationToken = default);

    Task<Desk?> GetDeskByIdAsync(Guid deskId, CancellationToken cancellationToken = default);

    Task<Booking?> GetConfirmedBookingForUserOnDateAsync(
        Guid userId,
        DateOnly date,
        CancellationToken cancellationToken = default);

    Task<Booking?> GetConfirmedBookingForDeskOnDateAsync(
        Guid deskId,
        DateOnly date,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyDictionary<Guid, Booking>> GetConfirmedBookingsByDeskIdsAsync(
        IEnumerable<Guid> deskIds,
        DateOnly date,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<(Booking Booking, string DeskNumber)>> GetBookingsForUserAsync(
        Guid userId,
        CancellationToken cancellationToken = default);

    Task<Booking?> GetBookingByIdForUserAsync(
        Guid userId,
        Guid bookingId,
        CancellationToken cancellationToken = default);

    Task AddBookingAsync(Booking booking, CancellationToken cancellationToken = default);

    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
