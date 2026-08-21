using EmployeeDeskBooking.Application.Bookings;
using EmployeeDeskBooking.Application.Notifications;
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

    public async Task<IReadOnlyList<(Booking Booking, string DeskNumber)>> GetBookingsForUserAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        var rows = await dbContext.Bookings.AsNoTracking()
            .Where(b => b.UserId == userId)
            .Join(
                dbContext.Desks.AsNoTracking(),
                b => b.DeskId,
                d => d.Id,
                (b, d) => new { Booking = b, d.DeskNumber })
            .OrderByDescending(x => x.Booking.BookingDate)
            .ToListAsync(cancellationToken);

        return rows.Select(x => (x.Booking, x.DeskNumber)).ToList();
    }

    public Task<Booking?> GetBookingByIdForUserAsync(
        Guid userId,
        Guid bookingId,
        CancellationToken cancellationToken = default) =>
        dbContext.Bookings.FirstOrDefaultAsync(
            b => b.Id == bookingId && b.UserId == userId,
            cancellationToken);

    public Task<Booking?> GetBookingByIdAsync(
        Guid bookingId,
        CancellationToken cancellationToken = default) =>
        dbContext.Bookings.FirstOrDefaultAsync(b => b.Id == bookingId, cancellationToken);

    public async Task<IReadOnlyList<(Booking Booking, string DeskNumber, string EmployeeEmail, string EmployeeName)>> GetAllBookingsAsync(
        DateOnly? date,
        BookingStatus? status,
        CancellationToken cancellationToken = default)
    {
        var query =
            from booking in dbContext.Bookings.AsNoTracking()
            join desk in dbContext.Desks.AsNoTracking() on booking.DeskId equals desk.Id
            join user in dbContext.Users.AsNoTracking() on booking.UserId equals user.Id
            select new { booking, desk.DeskNumber, user.Email, user.Name };

        if (date.HasValue)
        {
            query = query.Where(x => x.booking.BookingDate == date.Value);
        }

        if (status.HasValue)
        {
            query = query.Where(x => x.booking.Status == status.Value);
        }

        var rows = await query
            .OrderByDescending(x => x.booking.BookingDate)
            .ToListAsync(cancellationToken);

        return rows
            .Select(x => (x.booking, x.DeskNumber, x.Email, x.Name))
            .ToList();
    }

    public Task<bool> HasConfirmedBookingsForDeskOnOrAfterAsync(
        Guid deskId,
        DateOnly fromDate,
        CancellationToken cancellationToken = default) =>
        dbContext.Bookings.AsNoTracking().AnyAsync(
            b => b.DeskId == deskId
                && b.Status == BookingStatus.Confirmed
                && b.BookingDate >= fromDate,
            cancellationToken);

    public async Task<BookingEmailDetails?> GetBookingEmailDetailsAsync(
        Guid bookingId,
        CancellationToken cancellationToken = default)
    {
        var row = await (
            from booking in dbContext.Bookings.AsNoTracking()
            join desk in dbContext.Desks.AsNoTracking() on booking.DeskId equals desk.Id
            join user in dbContext.Users.AsNoTracking() on booking.UserId equals user.Id
            where booking.Id == bookingId
            select new { booking.Id, booking.UserId, user.Email, desk.DeskNumber, booking.BookingDate })
            .FirstOrDefaultAsync(cancellationToken);

        if (row is null)
        {
            return null;
        }

        return new BookingEmailDetails
        {
            BookingId = row.Id,
            UserId = row.UserId,
            RecipientEmail = row.Email,
            DeskNumber = row.DeskNumber,
            BookingDate = row.BookingDate,
        };
    }

    public async Task<IReadOnlyList<BookingEmailDetails>> GetConfirmedBookingEmailDetailsForDateAsync(
        DateOnly bookingDate,
        CancellationToken cancellationToken = default)
    {
        var rows = await (
            from booking in dbContext.Bookings.AsNoTracking()
            join desk in dbContext.Desks.AsNoTracking() on booking.DeskId equals desk.Id
            join user in dbContext.Users.AsNoTracking() on booking.UserId equals user.Id
            where booking.BookingDate == bookingDate && booking.Status == BookingStatus.Confirmed
            select new { booking.Id, booking.UserId, user.Email, desk.DeskNumber, booking.BookingDate })
            .ToListAsync(cancellationToken);

        return rows
            .Select(row => new BookingEmailDetails
            {
                BookingId = row.Id,
                UserId = row.UserId,
                RecipientEmail = row.Email,
                DeskNumber = row.DeskNumber,
                BookingDate = row.BookingDate,
            })
            .ToList();
    }

    public async Task<IReadOnlyList<Booking>> GetConfirmedBookingsBeforeDateAsync(
        DateOnly beforeDate,
        CancellationToken cancellationToken = default) =>
        await dbContext.Bookings
            .Where(b => b.Status == BookingStatus.Confirmed && b.BookingDate < beforeDate)
            .ToListAsync(cancellationToken);

    public Task SaveChangesAsync(CancellationToken cancellationToken = default) =>
        dbContext.SaveChangesAsync(cancellationToken);
}
