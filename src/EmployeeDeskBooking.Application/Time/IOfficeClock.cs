namespace EmployeeDeskBooking.Application.Time;

public interface IOfficeClock
{
    DateOnly Today { get; }

    bool IsWorkingDay(DateOnly date);

    bool IsWithinBookingWindow(DateOnly date);
}
