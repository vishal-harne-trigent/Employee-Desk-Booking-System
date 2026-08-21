using EmployeeDeskBooking.Application.Bookings;
using EmployeeDeskBooking.Domain.Bookings;
using EmployeeDeskBooking.Domain.Desks;
using EmployeeDeskBooking.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace EmployeeDeskBooking.Infrastructure.Bookings;

public sealed class EfBookingRepository(AppDbContext dbContext) : IBookingRepository
{
    public async Task<IReadOnlyList<Desk>> GetActiveDesksAsync(CancellationToken cancellationToken = default)
    {
        return await dbContext.Desks
            .AsNoTracking()
            .Where(d => d.Status == DeskStatus.Active)
            .OrderBy(d => d.DeskNumberNormalized)
            .ToListAsync(cancellationToken);
    }

    public Task<Desk?> GetDeskByIdAsync(Guid deskId, CancellationToken cancellationToken = default) =>
        dbContext.Desks.AsNoTracking().FirstOrDefaultAsync(d => d.Id == deskId, cancellationToken);

    public Task<Booking?> GetConfirmedBookingForUserOnDateAsync(
        Guid userId,
        DateOnly date,
        CancellationToken cancellationToken = default) =>
        dbContext.Bookings.AsNoTracking()
            .FirstOrDefaultAsync(
                b => b.UserId == userId && b.BookingDate == date && b.Status == BookingStatus.Confirmed,
                cancellationToken);

    public Task<Booking?> GetConfirmedBookingForDeskOnDateAsync(
        Guid deskId,
        DateOnly date,
        CancellationToken cancellationToken = default) =>
        dbContext.Bookings.AsNoTracking()
            .FirstOrDefaultAsync(
                b => b.DeskId == deskId && b.BookingDate == date && b.Status == BookingStatus.Confirmed,
                cancellationToken);

    public async Task<IReadOnlyDictionary<Guid, Booking>> GetConfirmedBookingsByDeskIdsAsync(
        IEnumerable<Guid> deskIds,
        DateOnly date,
        CancellationToken cancellationToken = default)
    {
        var ids = deskIds.ToList();
        if (ids.Count == 0)
        {
            return new Dictionary<Guid, Booking>();
        }

        var bookings = await dbContext.Bookings.AsNoTracking()
            .Where(b => ids.Contains(b.DeskId) && b.BookingDate == date && b.Status == BookingStatus.Confirmed)
            .ToListAsync(cancellationToken);

        return bookings.ToDictionary(b => b.DeskId);
    }

    public async Task AddBookingAsync(Booking booking, CancellationToken cancellationToken = default) =>
        await dbContext.Bookings.AddAsync(booking, cancellationToken);

    public Task SaveChangesAsync(CancellationToken cancellationToken = default) =>
        dbContext.SaveChangesAsync(cancellationToken);
}
