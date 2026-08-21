namespace EmployeeDeskBooking.Application.Bookings;

public sealed class DeskAvailabilityItem
{
    public required Guid DeskId { get; init; }

    public required string DeskNumber { get; init; }

    public required bool IsAvailable { get; init; }
}

public sealed class AvailabilityResult
{
    private AvailabilityResult(
        IReadOnlyList<DeskAvailabilityItem>? desks,
        BookingDateValidationError? dateError,
        string? existingBookingDeskNumber)
    {
        Desks = desks ?? Array.Empty<DeskAvailabilityItem>();
        DateError = dateError;
        ExistingBookingDeskNumber = existingBookingDeskNumber;
    }

    public IReadOnlyList<DeskAvailabilityItem> Desks { get; }

    public BookingDateValidationError? DateError { get; }

    public string? ExistingBookingDeskNumber { get; }

    public bool HasDateError => DateError is not null;

    public bool UserAlreadyBooked => ExistingBookingDeskNumber is not null;

    public static AvailabilityResult DateInvalid(BookingDateValidationError error) =>
        new(null, error, null);

    public static AvailabilityResult Success(
        IReadOnlyList<DeskAvailabilityItem> desks,
        string? existingBookingDeskNumber = null) =>
        new(desks, null, existingBookingDeskNumber);
}

public sealed class CreateBookingResult
{
    private CreateBookingResult(Guid? bookingId, CreateBookingFailureReason? failureReason)
    {
        BookingId = bookingId;
        FailureReason = failureReason;
    }

    public Guid? BookingId { get; }

    public CreateBookingFailureReason? FailureReason { get; }

    public bool Succeeded => BookingId is not null;

    public static CreateBookingResult Success(Guid bookingId) => new(bookingId, null);

    public static CreateBookingResult Failure(CreateBookingFailureReason reason) => new(null, reason);
}
