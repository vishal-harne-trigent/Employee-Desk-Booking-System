namespace EmployeeDeskBooking.Api.Contracts.Admin;

public sealed class AdminBookingResponse
{
    public required Guid BookingId { get; init; }

    public required DateOnly BookingDate { get; init; }

    public required string DeskNumber { get; init; }

    public required string EmployeeEmail { get; init; }

    public required string EmployeeName { get; init; }

    public required string Status { get; init; }

    public required bool CanCancel { get; init; }
}

public sealed class AdminBookingsListResponse
{
    public IReadOnlyList<AdminBookingResponse> Bookings { get; init; } = Array.Empty<AdminBookingResponse>();
}

public sealed class AdminCancelBookingResponse
{
    public required Guid BookingId { get; init; }

    public required string Status { get; init; }
}
