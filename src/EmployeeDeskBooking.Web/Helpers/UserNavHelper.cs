namespace EmployeeDeskBooking.Web.Helpers;

public static class UserNavHelper
{
    public static string GetInitials(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return "?";
        }

        var parts = name.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length == 1)
        {
            return parts[0][..Math.Min(2, parts[0].Length)].ToUpperInvariant();
        }

        return string.Concat(parts[0][0], parts[^1][0]).ToUpperInvariant();
    }
}
