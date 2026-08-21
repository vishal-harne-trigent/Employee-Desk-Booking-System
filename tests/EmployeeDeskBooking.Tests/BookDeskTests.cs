using EmployeeDeskBooking.Domain.Bookings;
using EmployeeDeskBooking.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace EmployeeDeskBooking.Tests;

public class BookDeskTests(CustomWebApplicationFactory factory) : IClassFixture<CustomWebApplicationFactory>
{
    private static readonly DateOnly ValidDate = BookDeskTestClient.FixedToday.AddDays(1);

    private async Task ResetAsync() => await factory.ResetBookingsAsync();

    [Fact(DisplayName = "Select a date within the booking window loads availability (US-002/AC-01)")]
    public async Task Select_valid_date_loads_availability_US_002_AC_01()
    {
        await ResetAsync();
        var client = new BookDeskTestClient(factory);
        await client.LoginAsEmployeeAsync();

        var response = await client.CheckAvailabilityAsync(ValidDate);
        var body = await response.Content.ReadAsStringAsync();

        response.EnsureSuccessStatusCode();
        Assert.Contains("A-01", body, StringComparison.Ordinal);
        Assert.Contains("Check availability", body, StringComparison.Ordinal);
    }

    [Fact(DisplayName = "Invalid dates are rejected (US-002/AC-02)")]
    public async Task Invalid_dates_rejected_US_002_AC_02()
    {
        await ResetAsync();
        var client = new BookDeskTestClient(factory);
        await client.LoginAsEmployeeAsync();

        var weekend = BookDeskTestClient.FixedToday.AddDays(4);
        var weekendResponse = await client.CheckAvailabilityAsync(weekend);
        var weekendBody = await weekendResponse.Content.ReadAsStringAsync();
        Assert.Contains("working days", weekendBody, StringComparison.OrdinalIgnoreCase);

        var past = BookDeskTestClient.FixedToday.AddDays(-1);
        var pastResponse = await client.CheckAvailabilityAsync(past);
        var pastBody = await pastResponse.Content.ReadAsStringAsync();
        Assert.Contains("future date", pastBody, StringComparison.OrdinalIgnoreCase);

        var beyond = BookDeskTestClient.FixedToday.AddDays(31);
        var beyondResponse = await client.CheckAvailabilityAsync(beyond);
        var beyondBody = await beyondResponse.Content.ReadAsStringAsync();
        Assert.Contains("30 calendar days", beyondBody, StringComparison.Ordinal);
    }

    [Fact(DisplayName = "Active desks show availability status (US-002/AC-03)")]
    public async Task Active_desks_show_availability_status_US_002_AC_03()
    {
        await ResetAsync();
        using (var scope = factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var admin = db.Users.Single(u => u.Email == "admin@test.com");
            await BookDeskTestFactoryExtensions.SeedConfirmedBookingAsync(db, admin.Id, "A-02", ValidDate);
        }

        var client = new BookDeskTestClient(factory);
        await client.LoginAsEmployeeAsync();

        var response = await client.CheckAvailabilityAsync(ValidDate);
        var body = await response.Content.ReadAsStringAsync();

        response.EnsureSuccessStatusCode();
        Assert.Contains("Available", body, StringComparison.Ordinal);
        Assert.Contains("Booked", body, StringComparison.Ordinal);
        Assert.Contains("A-01", body, StringComparison.Ordinal);
        Assert.Contains("A-02", body, StringComparison.Ordinal);
    }

    [Fact(DisplayName = "Employee books one available desk (US-002/AC-04)")]
    public async Task Employee_books_available_desk_US_002_AC_04()
    {
        await ResetAsync();
        var client = new BookDeskTestClient(factory);
        await client.LoginAsEmployeeAsync();

        var deskId = client.GetDeskIdByNumber("A-01");
        var response = await client.BookDeskAsync(deskId, ValidDate);
        var body = await response.Content.ReadAsStringAsync();

        response.EnsureSuccessStatusCode();
        Assert.Contains("confirmed", body, StringComparison.OrdinalIgnoreCase);

        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var employee = db.Users.Single(u => u.Email == "employee@test.com");
        Assert.True(db.Bookings.Any(b =>
            b.UserId == employee.Id &&
            b.BookingDate == ValidDate &&
            b.Status == BookingStatus.Confirmed));
    }

    [Fact(DisplayName = "Double booking same day is rejected (US-002/AC-05)")]
    public async Task Double_booking_same_day_rejected_US_002_AC_05()
    {
        await ResetAsync();
        var client = new BookDeskTestClient(factory);
        await client.LoginAsEmployeeAsync();

        var firstDesk = client.GetDeskIdByNumber("A-01");
        await client.BookDeskAsync(firstDesk, ValidDate);

        var secondDesk = client.GetDeskIdByNumber("A-02");
        var response = await client.BookDeskAsync(secondDesk, ValidDate);
        var body = await response.Content.ReadAsStringAsync();

        response.EnsureSuccessStatusCode();
        Assert.Contains("already have a desk booked", body, StringComparison.OrdinalIgnoreCase);
    }

    [Fact(DisplayName = "Inactive or taken desks are not bookable (US-002/AC-06)")]
    public async Task Inactive_or_taken_desks_not_bookable_US_002_AC_06()
    {
        await ResetAsync();
        var client = new BookDeskTestClient(factory);
        await client.LoginAsEmployeeAsync();

        var inactiveDesk = client.GetDeskIdByNumber("B-01");
        var inactiveResponse = await client.BookDeskAsync(inactiveDesk, ValidDate);
        var inactiveBody = await inactiveResponse.Content.ReadAsStringAsync();
        Assert.Contains("not available", inactiveBody, StringComparison.OrdinalIgnoreCase);

        using (var scope = factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var admin = db.Users.Single(u => u.Email == "admin@test.com");
            await BookDeskTestFactoryExtensions.SeedConfirmedBookingAsync(db, admin.Id, "A-01", ValidDate.AddDays(2));
        }

        var takenDesk = client.GetDeskIdByNumber("A-01");
        var takenResponse = await client.BookDeskAsync(takenDesk, ValidDate.AddDays(2));
        var takenBody = await takenResponse.Content.ReadAsStringAsync();
        Assert.Contains("already booked", takenBody, StringComparison.OrdinalIgnoreCase);
    }
}
