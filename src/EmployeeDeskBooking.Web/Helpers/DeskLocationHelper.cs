using EmployeeDeskBooking.Application.Desks;

namespace EmployeeDeskBooking.Web.Helpers;

public static class DeskLocationHelper
{
    public static string FormatLocation(string deskNumber) =>
        DeskLocationFormatter.FormatLocation(deskNumber);
}
