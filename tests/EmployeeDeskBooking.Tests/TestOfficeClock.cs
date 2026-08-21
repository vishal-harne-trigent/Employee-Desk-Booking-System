using EmployeeDeskBooking.Application.Time;

namespace EmployeeDeskBooking.Tests;

public sealed class TestOfficeClock(DateOnly today) : IOfficeClock
{
    public DateOnly Today { get; } = today;

    public bool IsWorkingDay(DateOnly date) =>
        date.DayOfWeek is not DayOfWeek.Saturday and not DayOfWeek.Sunday;

    public bool IsWithinBookingWindow(DateOnly date) =>
        date >= Today && date <= Today.AddDays(30) && IsWorkingDay(date);
}
