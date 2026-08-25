using EmployeeDeskBooking.Application.Desks;

namespace EmployeeDeskBooking.Web.Helpers;

public static class DeskLocationHelper
{
    public static string FormatLocation(string deskNumber) =>
        DeskLocationFormatter.FormatLocation(deskNumber);

    public static string ResolveLocation(string deskNumber, string? storedLocation = null) =>
        DeskLocationFormatter.ResolveLocation(deskNumber, storedLocation);

    public static string FormatDeskWithLocation(string deskNumber, string? storedLocation = null) =>
        DeskLocationFormatter.FormatDeskWithLocation(deskNumber, storedLocation);
}
