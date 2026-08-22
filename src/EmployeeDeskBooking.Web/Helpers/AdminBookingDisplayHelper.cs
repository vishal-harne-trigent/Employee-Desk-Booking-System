using System.Globalization;

namespace EmployeeDeskBooking.Web.Helpers;

public static class AdminBookingDisplayHelper
{
    public static TimeZoneInfo GetOfficeTimeZone(IConfiguration configuration) =>
        TimeZoneInfo.FindSystemTimeZoneById(
            configuration["Office:TimeZone"] ?? "India Standard Time");

    public static string FormatOfficeDate(DateOnly date) =>
        date.ToString("ddd, MMM d, yyyy", CultureInfo.InvariantCulture);

    public static string FormatTimestamp(DateTimeOffset timestamp, TimeZoneInfo timeZone) =>
        TimeZoneInfo.ConvertTime(timestamp, timeZone).ToString("MMM d, HH:mm", CultureInfo.InvariantCulture);
}
