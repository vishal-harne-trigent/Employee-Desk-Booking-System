using EmployeeDeskBooking.Domain.Bookings;
using EmployeeDeskBooking.Infrastructure.Data;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Net.Http.Json;

namespace EmployeeDeskBooking.Tests;

public class ApiAdminBookingsTests(CustomApiApplicationFactory factory) : IClassFixture<CustomApiApplicationFactory>
{
    private static readonly DateOnly Today = BookDeskTestClient.FixedToday;
    private static readonly DateOnly FutureDate = Today.AddDays(1);
    private static readonly DateOnly OtherDate = Today.AddDays(2);

    private async Task ResetAsync() => await factory.ResetBookingsAsync();

    [Fact(DisplayName = "API admin lists all bookings (US-004/AC-01)")]
    public async Task Api_admin_lists_bookings_US_004_AC_01()
    {
        await ResetAsync();
        using (var scope = factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var employee = db.Users.Single(u => u.Email == "employee@test.com");
            await BookDeskTestFactoryExtensions.SeedBookingAsync(db, employee.Id, "A-01", FutureDate, BookingStatus.Confirmed);
        }

        var client = new ApiTestClient(factory);
        await client.AuthorizeAsAdminAsync();

        var response = await client.GetAdminBookingsAsync();
        response.EnsureSuccessStatusCode();

        var body = await response.Content.ReadFromJsonAsync<AdminBookingsApiResponse>();
        Assert.NotNull(body);
        Assert.NotEmpty(body!.Bookings);
        Assert.Contains(body.Bookings, b => b.EmployeeEmail == "employee@test.com");
    }

    [Fact(DisplayName = "API admin filters by date (US-004/AC-02)")]
    public async Task Api_admin_filters_by_date_US_004_AC_02()
    {
        await ResetAsync();
        using (var scope = factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var employee = db.Users.Single(u => u.Email == "employee@test.com");
            await BookDeskTestFactoryExtensions.SeedBookingAsync(db, employee.Id, "A-01", FutureDate, BookingStatus.Confirmed);
            await BookDeskTestFactoryExtensions.SeedBookingAsync(db, employee.Id, "A-02", OtherDate, BookingStatus.Confirmed);
        }

        var client = new ApiTestClient(factory);
        await client.AuthorizeAsAdminAsync();

        var response = await client.GetAdminBookingsAsync(FutureDate, null);
        var body = await response.Content.ReadFromJsonAsync<AdminBookingsApiResponse>();

        Assert.NotNull(body);
        Assert.Single(body!.Bookings);
        Assert.Equal(FutureDate, body.Bookings[0].BookingDate);
    }

    [Fact(DisplayName = "API admin filters by status (US-004/AC-03)")]
    public async Task Api_admin_filters_by_status_US_004_AC_03()
    {
        await ResetAsync();
        using (var scope = factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var employee = db.Users.Single(u => u.Email == "employee@test.com");
            await BookDeskTestFactoryExtensions.SeedBookingAsync(db, employee.Id, "A-01", FutureDate, BookingStatus.Confirmed);
            await BookDeskTestFactoryExtensions.SeedBookingAsync(db, employee.Id, "A-02", OtherDate, BookingStatus.Cancelled);
        }

        var client = new ApiTestClient(factory);
        await client.AuthorizeAsAdminAsync();

        var response = await client.GetAdminBookingsAsync(null, BookingStatus.Cancelled);
        var body = await response.Content.ReadFromJsonAsync<AdminBookingsApiResponse>();

        Assert.NotNull(body);
        Assert.Single(body!.Bookings);
        Assert.Equal("Cancelled", body.Bookings[0].Status);
    }

    [Fact(DisplayName = "API admin cancels booking on behalf of employee (US-004/AC-04)")]
    public async Task Api_admin_cancels_booking_US_004_AC_04()
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

        var client = new ApiTestClient(factory);
        await client.AuthorizeAsAdminAsync();

        var response = await client.AdminCancelBookingAsync(bookingId);
        Assert.Equal(StatusCodes.Status200OK, (int)response.StatusCode);

        using var verifyScope = factory.Services.CreateScope();
        var verifyDb = verifyScope.ServiceProvider.GetRequiredService<AppDbContext>();
        var booking = await verifyDb.Bookings.SingleAsync(b => b.Id == bookingId);
        Assert.Equal(BookingStatus.Cancelled, booking.Status);
    }

    [Fact(DisplayName = "API employee cannot access admin bookings (US-004/V-07)")]
    public async Task Api_employee_denied_admin_bookings_V_07()
    {
        var client = new ApiTestClient(factory);
        await client.AuthorizeAsEmployeeAsync();

        var response = await client.GetAdminBookingsAsync();
        Assert.Equal(StatusCodes.Status403Forbidden, (int)response.StatusCode);
    }

    private sealed class AdminBookingsApiResponse
    {
        public List<AdminBookingApiItem> Bookings { get; set; } = [];
    }

    private sealed class AdminBookingApiItem
    {
        public Guid BookingId { get; set; }

        public DateOnly BookingDate { get; set; }

        public string EmployeeEmail { get; set; } = string.Empty;

        public string Status { get; set; } = string.Empty;
    }
}
