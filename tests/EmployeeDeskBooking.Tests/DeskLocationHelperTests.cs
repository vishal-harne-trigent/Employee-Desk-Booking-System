using EmployeeDeskBooking.Web.Helpers;

namespace EmployeeDeskBooking.Tests;

public class DeskLocationHelperTests
{
    [Theory(DisplayName = "Desk location label is derived from desk number prefix")]
    [InlineData("A-01", "Floor 1, Zone C")]
    [InlineData("A-02", "Floor 1, Zone C")]
    [InlineData("B-01", "Floor 2, Zone D")]
    public void FormatLocation_maps_desk_prefix_to_floor_and_zone(string deskNumber, string expected)
    {
        Assert.Equal(expected, DeskLocationHelper.FormatLocation(deskNumber));
    }
}
