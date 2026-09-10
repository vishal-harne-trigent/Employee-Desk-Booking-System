using EmployeeDeskBooking.Web.Helpers;

namespace EmployeeDeskBooking.Tests;

public class DeskLocationHelperTests
{
    [Theory(DisplayName = "Stored desk location overrides prefix-derived default")]
    [InlineData("A-01", "Custom wing", "Custom wing")]
    [InlineData("A-01", null, "Floor 1, Zone C")]
    [InlineData("A-01", "   ", "Floor 1, Zone C")]
    public void ResolveLocation_prefers_stored_value(string deskNumber, string? stored, string expected)
    {
        Assert.Equal(expected, DeskLocationHelper.ResolveLocation(deskNumber, stored));
    }

    [Theory(DisplayName = "Desk location label is derived from desk number prefix")]
    [InlineData("A-01", "Floor 1, Zone C")]
    [InlineData("A-02", "Floor 1, Zone C")]
    [InlineData("B-01", "Floor 2, Zone D")]
    public void FormatLocation_maps_desk_prefix_to_floor_and_zone(string deskNumber, string expected)
    {
        Assert.Equal(expected, DeskLocationHelper.FormatLocation(deskNumber));
    }
}
