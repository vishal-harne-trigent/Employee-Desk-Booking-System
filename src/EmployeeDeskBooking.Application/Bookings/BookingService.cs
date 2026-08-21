using EmployeeDeskBooking.Application.Time;
using EmployeeDeskBooking.Domain.Bookings;

namespace EmployeeDeskBooking.Application.Bookings;

public sealed class BookingService(IBookingRepository bookings, IOfficeClock officeClock) : IBookingService
{
    public BookingDateValidationError? ValidateBookingDate(DateOnly date)
    {
        if (date < officeClock.Today)
        {
            return BookingDateValidationError.BeforeToday;
        }

        if (date > officeClock.Today.AddDays(30))
        {
            return BookingDateValidationError.BeyondWindow;
        }

        if (!officeClock.IsWorkingDay(date))
        {
            return BookingDateValidationError.Weekend;
        }

        return null;
    }

    public async Task<AvailabilityResult> GetAvailabilityAsync(
        Guid userId,
        DateOnly date,
        CancellationToken cancellationToken = default)
    {
        var dateError = ValidateBookingDate(date);
        if (dateError is not null)
        {
            return AvailabilityResult.DateInvalid(dateError.Value);
        }

        var existing = await bookings.GetConfirmedBookingForUserOnDateAsync(userId, date, cancellationToken);
        string? existingDeskNumber = null;
        if (existing is not null)
        {
            var bookedDesk = await bookings.GetDeskByIdAsync(existing.DeskId, cancellationToken);
            existingDeskNumber = bookedDesk?.DeskNumber;
        }

        var activeDesks = await bookings.GetActiveDesksAsync(cancellationToken);
        var deskIds = activeDesks.Select(d => d.Id).ToList();
        var confirmedByDesk = await bookings.GetConfirmedBookingsByDeskIdsAsync(deskIds, date, cancellationToken);

        var items = activeDesks
            .OrderBy(d => d.DeskNumberNormalized)
            .Select(desk => new DeskAvailabilityItem
            {
                DeskId = desk.Id,
                DeskNumber = desk.DeskNumber,
                IsAvailable = !confirmedByDesk.ContainsKey(desk.Id),
            })
            .ToList();

        return AvailabilityResult.Success(items, existingDeskNumber);
    }

    public async Task<CreateBookingResult> CreateBookingAsync(
        Guid userId,
        Guid deskId,
        DateOnly date,
        CancellationToken cancellationToken = default)
    {
        if (ValidateBookingDate(date) is not null)
        {
            return CreateBookingResult.Failure(CreateBookingFailureReason.InvalidDate);
        }

        if (await bookings.GetConfirmedBookingForUserOnDateAsync(userId, date, cancellationToken) is not null)
        {
            return CreateBookingResult.Failure(CreateBookingFailureReason.UserAlreadyBooked);
        }

        var desk = await bookings.GetDeskByIdAsync(deskId, cancellationToken);
        if (desk is null)
        {
            return CreateBookingResult.Failure(CreateBookingFailureReason.DeskNotFound);
        }

        if (desk.Status != Domain.Desks.DeskStatus.Active)
        {
            return CreateBookingResult.Failure(CreateBookingFailureReason.DeskInactive);
        }

        if (await bookings.GetConfirmedBookingForDeskOnDateAsync(deskId, date, cancellationToken) is not null)
        {
            return CreateBookingResult.Failure(CreateBookingFailureReason.DeskAlreadyBooked);
        }

        var now = DateTimeOffset.UtcNow;
        var booking = new Booking
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            DeskId = deskId,
            BookingDate = date,
            Status = BookingStatus.Confirmed,
            CreatedAt = now,
            UpdatedAt = now,
        };

        try
        {
            await bookings.AddBookingAsync(booking, cancellationToken);
            await bookings.SaveChangesAsync(cancellationToken);
            return CreateBookingResult.Success(booking.Id);
        }
        catch (Exception ex) when (IsUniqueConstraintViolation(ex))
        {
            return CreateBookingResult.Failure(CreateBookingFailureReason.ConcurrencyConflict);
        }
    }

    public async Task<IReadOnlyList<MyBookingItem>> GetMyBookingsAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        var rows = await bookings.GetBookingsForUserAsync(userId, cancellationToken);
        var today = officeClock.Today;

        return rows
            .Select(row => new MyBookingItem
            {
                BookingId = row.Booking.Id,
                BookingDate = row.Booking.BookingDate,
                DeskNumber = row.DeskNumber,
                Status = row.Booking.Status,
                CanCancel = row.Booking.Status == BookingStatus.Confirmed
                    && row.Booking.BookingDate >= today,
            })
            .OrderByDescending(b => b.BookingDate)
            .ToList();
    }

    public async Task<CancelBookingResult> CancelBookingAsync(
        Guid userId,
        Guid bookingId,
        Guid cancelledById,
        CancellationToken cancellationToken = default)
    {
        var booking = await bookings.GetBookingByIdForUserAsync(userId, bookingId, cancellationToken);
        if (booking is null)
        {
            return CancelBookingResult.Failure(CancelBookingFailureReason.NotFound);
        }

        return await CancelConfirmedBookingAsync(booking, cancelledById, cancellationToken);
    }

    public async Task<IReadOnlyList<AdminBookingItem>> GetAllBookingsAsync(
        AdminBookingFilters? filters = null,
        CancellationToken cancellationToken = default)
    {
        var rows = await bookings.GetAllBookingsAsync(
            filters?.Date,
            filters?.Status,
            cancellationToken);

        var today = officeClock.Today;

        return rows
            .Select(row => new AdminBookingItem
            {
                BookingId = row.Booking.Id,
                BookingDate = row.Booking.BookingDate,
                DeskNumber = row.DeskNumber,
                EmployeeEmail = row.EmployeeEmail,
                EmployeeName = row.EmployeeName,
                Status = row.Booking.Status,
                CanCancel = CanCancel(row.Booking, today),
            })
            .OrderByDescending(b => b.BookingDate)
            .ToList();
    }

    public async Task<CancelBookingResult> AdminCancelBookingAsync(
        Guid bookingId,
        Guid adminId,
        CancellationToken cancellationToken = default)
    {
        var booking = await bookings.GetBookingByIdAsync(bookingId, cancellationToken);
        if (booking is null)
        {
            return CancelBookingResult.Failure(CancelBookingFailureReason.NotFound);
        }

        return await CancelConfirmedBookingAsync(booking, adminId, cancellationToken);
    }

    private async Task<CancelBookingResult> CancelConfirmedBookingAsync(
        Booking booking,
        Guid cancelledById,
        CancellationToken cancellationToken)
    {
        if (!CanCancel(booking, officeClock.Today))
        {
            return CancelBookingResult.Failure(CancelBookingFailureReason.NotCancellable);
        }

        var now = DateTimeOffset.UtcNow;
        booking.Status = BookingStatus.Cancelled;
        booking.CancelledAt = now;
        booking.CancelledById = cancelledById;
        booking.UpdatedAt = now;

        await bookings.SaveChangesAsync(cancellationToken);
        return CancelBookingResult.Success();
    }

    private static bool CanCancel(Booking booking, DateOnly today) =>
        booking.Status == BookingStatus.Confirmed && booking.BookingDate >= today;

    private static bool IsUniqueConstraintViolation(Exception ex)
    {
        var message = ex.ToString();
        return message.Contains("UNIQUE", StringComparison.OrdinalIgnoreCase)
            || message.Contains("duplicate", StringComparison.OrdinalIgnoreCase)
            || message.Contains("IX_Bookings", StringComparison.OrdinalIgnoreCase);
    }
}
