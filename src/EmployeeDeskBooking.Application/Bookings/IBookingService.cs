using EmployeeDeskBooking.Domain.Bookings;

namespace EmployeeDeskBooking.Application.Bookings;

public interface IBookingService
{
    BookingDateValidationError? ValidateBookingDate(DateOnly date);

    Task<AvailabilityResult> GetAvailabilityAsync(Guid userId, DateOnly date, CancellationToken cancellationToken = default);

    Task<CreateBookingResult> CreateBookingAsync(
        Guid userId,
        Guid deskId,
        DateOnly date,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<MyBookingItem>> GetMyBookingsAsync(
        Guid userId,
        CancellationToken cancellationToken = default);

    Task<CancelBookingResult> CancelBookingAsync(
        Guid userId,
        Guid bookingId,
        Guid cancelledById,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<AdminBookingItem>> GetAllBookingsAsync(
        AdminBookingFilters? filters = null,
        CancellationToken cancellationToken = default);

    Task<CancelBookingResult> AdminCancelBookingAsync(
        Guid bookingId,
        Guid adminId,
        CancellationToken cancellationToken = default);
}
