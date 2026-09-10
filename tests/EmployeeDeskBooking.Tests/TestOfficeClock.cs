using EmployeeDeskBooking.Application.Time;

namespace EmployeeDeskBooking.Tests;

public sealed class TestOfficeClock : IOfficeClock
{
    public TestOfficeClock(DateOnly today, TimeOnly? localTime = null)
    {
        Today = today;
        LocalTime = localTime ?? new TimeOnly(8, 0);
    }

    public DateOnly Today { get; set; }

    public TimeOnly LocalTime { get; set; }

    public bool IsWorkingDay(DateOnly date) =>
        date.DayOfWeek is not DayOfWeek.Saturday and not DayOfWeek.Sunday;

    public bool IsWithinBookingWindow(DateOnly date) =>
        date >= Today && date <= Today.AddDays(30) && IsWorkingDay(date);
}
