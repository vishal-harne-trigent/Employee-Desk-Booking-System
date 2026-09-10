namespace EmployeeDeskBooking.Application.Time;

public interface IOfficeClock
{
    DateOnly Today { get; }

    TimeOnly LocalTime { get; }

    bool IsWorkingDay(DateOnly date);

    bool IsWithinBookingWindow(DateOnly date);
}
