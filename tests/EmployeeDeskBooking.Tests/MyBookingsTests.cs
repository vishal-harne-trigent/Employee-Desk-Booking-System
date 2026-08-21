using EmployeeDeskBooking.Domain.Bookings;
using EmployeeDeskBooking.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace EmployeeDeskBooking.Tests;

public class MyBookingsTests(CustomWebApplicationFactory factory) : IClassFixture<CustomWebApplicationFactory>
{
    private static readonly DateOnly Today = BookDeskTestClient.FixedToday;
    private static readonly DateOnly FutureDate = Today.AddDays(1);
    private static readonly DateOnly PastDate = Today.AddDays(-5);

    private async Task ResetAsync() => await factory.ResetBookingsAsync();

    [Fact(DisplayName = "List my bookings shows date desk and status (US-003/AC-01)")]
    public async Task List_my_bookings_shows_details_US_003_AC_01()
    {
        await ResetAsync();
        using (var scope = factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var employee = db.Users.Single(u => u.Email == "employee@test.com");
            await BookDeskTestFactoryExtensions.SeedBookingAsync(db, employee.Id, "A-01", FutureDate, BookingStatus.Confirmed);
            await BookDeskTestFactoryExtensions.SeedBookingAsync(db, employee.Id, "A-02", PastDate, BookingStatus.Cancelled);
            await BookDeskTestFactoryExtensions.SeedBookingAsync(db, employee.Id, "A-01", PastDate.AddDays(-2), BookingStatus.Completed);
        }

        var client = new MyBookingsTestClient(factory);
        await client.LoginAsEmployeeAsync();

        var response = await client.GetMyBookingsPageAsync();
        var body = await response.Content.ReadAsStringAsync();

        response.EnsureSuccessStatusCode();
        Assert.Contains("Confirmed", body, StringComparison.Ordinal);
        Assert.Contains("Cancelled", body, StringComparison.Ordinal);
        Assert.Contains("Completed", body, StringComparison.Ordinal);
        Assert.Contains("A-01", body, StringComparison.Ordinal);
    }

    [Fact(DisplayName = "Cancel confirmed future booking (US-003/AC-02)")]
    public async Task Cancel_confirmed_future_booking_US_003_AC_02()
    {
        await ResetAsync();
        using (var scope = factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var employee = db.Users.Single(u => u.Email == "employee@test.com");
            await BookDeskTestFactoryExtensions.SeedBookingAsync(db, employee.Id, "A-01", FutureDate, BookingStatus.Confirmed);
        }

        var client = new MyBookingsTestClient(factory);
        await client.LoginAsEmployeeAsync();

        var bookingId = client.GetBookingIdForEmployee("A-01", FutureDate);
        var response = await client.CancelBookingAsync(bookingId);
        var body = await response.Content.ReadAsStringAsync();

        response.EnsureSuccessStatusCode();
        Assert.Contains("cancelled", body, StringComparison.OrdinalIgnoreCase);

        using var verifyScope = factory.Services.CreateScope();
        var verifyDb = verifyScope.ServiceProvider.GetRequiredService<AppDbContext>();
        var booking = await verifyDb.Bookings.SingleAsync(b => b.Id == bookingId);
        Assert.Equal(BookingStatus.Cancelled, booking.Status);
    }

    [Fact(DisplayName = "Past or completed bookings offer no cancel action (US-003/AC-03)")]
    public async Task Past_or_completed_bookings_no_cancel_US_003_AC_03()
    {
        await ResetAsync();
        using (var scope = factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var employee = db.Users.Single(u => u.Email == "employee@test.com");
            await BookDeskTestFactoryExtensions.SeedBookingAsync(db, employee.Id, "A-01", PastDate, BookingStatus.Completed);
            await BookDeskTestFactoryExtensions.SeedBookingAsync(db, employee.Id, "A-02", PastDate, BookingStatus.Confirmed);
        }

        var client = new MyBookingsTestClient(factory);
        await client.LoginAsEmployeeAsync();

        var response = await client.GetMyBookingsPageAsync();
        var body = await response.Content.ReadAsStringAsync();

        response.EnsureSuccessStatusCode();
        Assert.DoesNotContain("name=\"bookingId\"", body, StringComparison.Ordinal);
        Assert.Contains("Completed", body, StringComparison.Ordinal);
    }

    [Fact(DisplayName = "Empty state links to Book Desk (US-003/AC-04)")]
    public async Task Empty_state_links_to_book_desk_US_003_AC_04()
    {
        await ResetAsync();
        var client = new MyBookingsTestClient(factory);
        await client.LoginAsEmployeeAsync();

        var response = await client.GetMyBookingsPageAsync();
        var body = await response.Content.ReadAsStringAsync();

        response.EnsureSuccessStatusCode();
        Assert.Contains("do not have any desk bookings", body, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("/Book/Index", body, StringComparison.Ordinal);
    }
}
