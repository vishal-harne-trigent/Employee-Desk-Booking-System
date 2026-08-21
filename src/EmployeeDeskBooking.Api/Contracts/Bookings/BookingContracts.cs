namespace EmployeeDeskBooking.Api.Contracts.Bookings;

public sealed class AvailabilityResponse
{
    public required DateOnly Date { get; init; }

    public string? DateError { get; init; }

    public string? ExistingBookingDeskNumber { get; init; }

    public IReadOnlyList<DeskAvailabilityResponse> Desks { get; init; } = Array.Empty<DeskAvailabilityResponse>();
}

public sealed class DeskAvailabilityResponse
{
    public required Guid DeskId { get; init; }

    public required string DeskNumber { get; init; }

    public required bool IsAvailable { get; init; }
}

public sealed class CreateBookingRequest
{
    public Guid DeskId { get; set; }

    public DateOnly Date { get; set; }
}

public sealed class CreateBookingResponse
{
    public required Guid BookingId { get; init; }
}

public sealed class MyBookingResponse
{
    public required Guid BookingId { get; init; }

    public required DateOnly BookingDate { get; init; }

    public required string DeskNumber { get; init; }

    public required string Status { get; init; }

    public required bool CanCancel { get; init; }
}

public sealed class MyBookingsResponse
{
    public IReadOnlyList<MyBookingResponse> Bookings { get; init; } = Array.Empty<MyBookingResponse>();
}

public sealed class CancelBookingResponse
{
    public required Guid BookingId { get; init; }

    public required string Status { get; init; }
}
