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
}
