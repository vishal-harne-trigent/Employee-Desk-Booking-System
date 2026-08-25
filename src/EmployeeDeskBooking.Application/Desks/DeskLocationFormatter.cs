namespace EmployeeDeskBooking.Application.Desks;

public static class DeskLocationFormatter
{
    public static string ResolveLocation(string deskNumber, string? storedLocation = null)
    {
        if (!string.IsNullOrWhiteSpace(storedLocation))
        {
            return storedLocation.Trim();
        }

        return FormatLocation(deskNumber);
    }

    public static string FormatLocation(string deskNumber)
    {
        if (string.IsNullOrWhiteSpace(deskNumber))
        {
            return string.Empty;
        }

        var prefix = char.ToUpperInvariant(deskNumber.Trim()[0]);
        if (prefix is < 'A' or > 'Z')
        {
            return "Main office";
        }

        var floor = prefix - 'A' + 1;
        var zone = (char)(prefix + 2);
        return $"Floor {floor}, Zone {zone}";
    }

    public static string FormatDeskWithLocation(string deskNumber, string? storedLocation = null)
    {
        var location = ResolveLocation(deskNumber, storedLocation);
        return string.IsNullOrWhiteSpace(location)
            ? deskNumber
            : $"{deskNumber} — {location}";
    }

    public static string NormalizeStoredLocation(string? location) =>
        string.IsNullOrWhiteSpace(location) ? string.Empty : location.Trim();
}
