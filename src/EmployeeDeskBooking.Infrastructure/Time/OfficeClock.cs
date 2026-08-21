using EmployeeDeskBooking.Application.Time;
using Microsoft.Extensions.Configuration;

namespace EmployeeDeskBooking.Infrastructure.Time;

public sealed class OfficeClock(IConfiguration configuration) : IOfficeClock
{
    private TimeZoneInfo OfficeTimeZone =>
        TimeZoneInfo.FindSystemTimeZoneById(
            configuration["Office:TimeZone"] ?? "India Standard Time");

    public DateOnly Today =>
        DateOnly.FromDateTime(TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, OfficeTimeZone));

    public bool IsWorkingDay(DateOnly date) =>
        date.DayOfWeek is not DayOfWeek.Saturday and not DayOfWeek.Sunday;

    public bool IsWithinBookingWindow(DateOnly date) =>
        date >= Today && date <= Today.AddDays(30) && IsWorkingDay(date);
}
