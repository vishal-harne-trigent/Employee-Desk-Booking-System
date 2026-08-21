using EmployeeDeskBooking.Domain.Bookings;

namespace EmployeeDeskBooking.Application.Bookings;

public sealed class AdminBookingFilters
{
    public DateOnly? Date { get; init; }

    public BookingStatus? Status { get; init; }
}

public sealed class AdminBookingItem
{
    public required Guid BookingId { get; init; }

    public required DateOnly BookingDate { get; init; }

    public required string DeskNumber { get; init; }

    public required string EmployeeEmail { get; init; }

    public required string EmployeeName { get; init; }

    public required BookingStatus Status { get; init; }

    public required bool CanCancel { get; init; }
}
