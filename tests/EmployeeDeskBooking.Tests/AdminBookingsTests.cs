using EmployeeDeskBooking.Domain.Bookings;
using EmployeeDeskBooking.Infrastructure.Data;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace EmployeeDeskBooking.Tests;

public class AdminBookingsTests(CustomWebApplicationFactory factory) : IClassFixture<CustomWebApplicationFactory>
{
    private static readonly DateOnly Today = BookDeskTestClient.FixedToday;
    private static readonly DateOnly FutureDate = Today.AddDays(1);
    private static readonly DateOnly OtherDate = Today.AddDays(2);

    private async Task ResetAsync() => await factory.ResetBookingsAsync();

    [Fact(DisplayName = "Admin views all bookings with employee details (US-004/AC-01)")]
    public async Task Admin_views_all_bookings_US_004_AC_01()
    {
        await ResetAsync();
        using (var scope = factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var employee = db.Users.Single(u => u.Email == "employee@test.com");
            var admin = db.Users.Single(u => u.Email == "admin@test.com");
            await BookDeskTestFactoryExtensions.SeedBookingAsync(db, employee.Id, "A-01", FutureDate, BookingStatus.Confirmed);
            await BookDeskTestFactoryExtensions.SeedBookingAsync(db, admin.Id, "A-02", OtherDate, BookingStatus.Cancelled);
        }

        var client = new AdminBookingsTestClient(factory);
        await client.LoginAsAdminAsync();

        var response = await client.GetAdminBookingsPageAsync();
        var body = await response.Content.ReadAsStringAsync();

        response.EnsureSuccessStatusCode();
        Assert.Contains("employee@test.com", body, StringComparison.Ordinal);
        Assert.Contains("admin@test.com", body, StringComparison.Ordinal);
        Assert.Contains("Confirmed", body, StringComparison.Ordinal);
        Assert.Contains("Cancelled", body, StringComparison.Ordinal);
    }

    [Fact(DisplayName = "Admin filters bookings by date (US-004/AC-02)")]
    public async Task Admin_filters_by_date_US_004_AC_02()
    {
        await ResetAsync();
        using (var scope = factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var employee = db.Users.Single(u => u.Email == "employee@test.com");
            await BookDeskTestFactoryExtensions.SeedBookingAsync(db, employee.Id, "A-01", FutureDate, BookingStatus.Confirmed);
            await BookDeskTestFactoryExtensions.SeedBookingAsync(db, employee.Id, "A-02", OtherDate, BookingStatus.Confirmed);
        }

        var client = new AdminBookingsTestClient(factory);
        await client.LoginAsAdminAsync();

        var response = await client.ApplyFiltersAsync(FutureDate, null);
        var body = await response.Content.ReadAsStringAsync();

        response.EnsureSuccessStatusCode();
        Assert.Contains(FutureDate.ToString("dd MMM yyyy"), body, StringComparison.Ordinal);
        Assert.DoesNotContain(OtherDate.ToString("dd MMM yyyy"), body, StringComparison.Ordinal);
    }

    [Fact(DisplayName = "Admin filters bookings by status (US-004/AC-03)")]
    public async Task Admin_filters_by_status_US_004_AC_03()
    {
        await ResetAsync();
        using (var scope = factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var employee = db.Users.Single(u => u.Email == "employee@test.com");
            await BookDeskTestFactoryExtensions.SeedBookingAsync(db, employee.Id, "A-01", FutureDate, BookingStatus.Confirmed);
            await BookDeskTestFactoryExtensions.SeedBookingAsync(db, employee.Id, "A-02", OtherDate, BookingStatus.Completed);
        }

        var client = new AdminBookingsTestClient(factory);
        await client.LoginAsAdminAsync();

        var response = await client.ApplyFiltersAsync(null, BookingStatus.Completed);
        var body = await response.Content.ReadAsStringAsync();

        response.EnsureSuccessStatusCode();
        Assert.Contains("A-02", body, StringComparison.Ordinal);
        Assert.DoesNotContain("A-01", body, StringComparison.Ordinal);
    }

    [Fact(DisplayName = "Admin cancels confirmed future booking (US-004/AC-04)")]
    public async Task Admin_cancels_booking_US_004_AC_04()
    {
        await ResetAsync();
        Guid bookingId;
        using (var scope = factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var employee = db.Users.Single(u => u.Email == "employee@test.com");
            await BookDeskTestFactoryExtensions.SeedBookingAsync(db, employee.Id, "A-01", FutureDate, BookingStatus.Confirmed);
            bookingId = db.Bookings.Single(b => b.UserId == employee.Id).Id;
        }

        var client = new AdminBookingsTestClient(factory);
        await client.LoginAsAdminAsync();

        var response = await client.CancelBookingAsync(bookingId, null, null);
        var body = await response.Content.ReadAsStringAsync();

        response.EnsureSuccessStatusCode();
        Assert.Contains("cancelled", body, StringComparison.OrdinalIgnoreCase);

        using var verifyScope = factory.Services.CreateScope();
        var verifyDb = verifyScope.ServiceProvider.GetRequiredService<AppDbContext>();
        var booking = await verifyDb.Bookings.SingleAsync(b => b.Id == bookingId);
        Assert.Equal(BookingStatus.Cancelled, booking.Status);
    }

    [Fact(DisplayName = "Employee cannot access admin bookings page (US-004/V-07)")]
    public async Task Employee_cannot_access_admin_bookings_V_07()
    {
        var client = new BookDeskTestClient(factory);
        await client.LoginAsEmployeeAsync();

        var response = await client.Login.Client.GetAsync("/Admin/AdminBookings");
        Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);
        Assert.Contains("/Account/Login", response.Headers.Location?.OriginalString, StringComparison.Ordinal);
    }
}
