using EmployeeDeskBooking.Api.Bookings;
using EmployeeDeskBooking.Domain.Bookings;
using EmployeeDeskBooking.Infrastructure.Data;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Net.Http.Json;

namespace EmployeeDeskBooking.Tests;

public class ApiMyBookingsTests(CustomApiApplicationFactory factory) : IClassFixture<CustomApiApplicationFactory>
{
    private static readonly DateOnly Today = BookDeskTestClient.FixedToday;
    private static readonly DateOnly FutureDate = Today.AddDays(1);
    private static readonly DateOnly PastDate = Today.AddDays(-5);

    private async Task ResetAsync() => await factory.ResetBookingsAsync();

    [Fact(DisplayName = "API lists employee bookings (US-003/AC-01)")]
    public async Task Api_lists_employee_bookings_US_003_AC_01()
    {
        await ResetAsync();
        using (var scope = factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var employee = db.Users.Single(u => u.Email == "employee@test.com");
            await BookDeskTestFactoryExtensions.SeedBookingAsync(db, employee.Id, "A-01", FutureDate, BookingStatus.Confirmed);
            await BookDeskTestFactoryExtensions.SeedBookingAsync(db, employee.Id, "A-02", PastDate, BookingStatus.Cancelled);
        }

        var client = new ApiTestClient(factory);
        await client.AuthorizeAsEmployeeAsync();

        var response = await client.GetMyBookingsAsync();
        response.EnsureSuccessStatusCode();

        var body = await response.Content.ReadFromJsonAsync<MyBookingsApiResponse>();
        Assert.NotNull(body);
        Assert.Equal(2, body!.Bookings.Count);
        Assert.Contains(body.Bookings, b => b.Status == "Confirmed" && b.DeskNumber == "A-01");
        Assert.Contains(body.Bookings, b => b.Status == "Cancelled");
    }

    [Fact(DisplayName = "API cancels confirmed future booking (US-003/AC-02)")]
    public async Task Api_cancels_confirmed_future_booking_US_003_AC_02()
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
        await client.AuthorizeAsEmployeeAsync();

        var response = await client.CancelBookingAsync(bookingId);
        Assert.Equal(StatusCodes.Status200OK, (int)response.StatusCode);

        using var verifyScope = factory.Services.CreateScope();
        var verifyDb = verifyScope.ServiceProvider.GetRequiredService<AppDbContext>();
        var booking = await verifyDb.Bookings.SingleAsync(b => b.Id == bookingId);
        Assert.Equal(BookingStatus.Cancelled, booking.Status);
    }

    [Fact(DisplayName = "API marks past bookings as not cancellable (US-003/AC-03)")]
    public async Task Api_past_bookings_not_cancellable_US_003_AC_03()
    {
        await ResetAsync();
        Guid bookingId;
        using (var scope = factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var employee = db.Users.Single(u => u.Email == "employee@test.com");
            await BookDeskTestFactoryExtensions.SeedBookingAsync(db, employee.Id, "A-01", PastDate, BookingStatus.Confirmed);
            bookingId = db.Bookings.Single(b => b.UserId == employee.Id).Id;
        }

        var client = new ApiTestClient(factory);
        await client.AuthorizeAsEmployeeAsync();

        var listResponse = await client.GetMyBookingsAsync();
        var list = await listResponse.Content.ReadFromJsonAsync<MyBookingsApiResponse>();
        Assert.NotNull(list);
        Assert.Contains(list!.Bookings, b => b.BookingId == bookingId && !b.CanCancel);

        var cancelResponse = await client.CancelBookingAsync(bookingId);
        Assert.Equal(StatusCodes.Status409Conflict, (int)cancelResponse.StatusCode);
        var detail = await ApiTestClient.ReadProblemDetailAsync(cancelResponse);
        Assert.Contains("cannot be cancelled", detail, StringComparison.OrdinalIgnoreCase);
    }

    [Fact(DisplayName = "API returns empty bookings list (US-003/AC-04)")]
    public async Task Api_returns_empty_bookings_list_US_003_AC_04()
    {
        await ResetAsync();
        var client = new ApiTestClient(factory);
        await client.AuthorizeAsEmployeeAsync();

        var response = await client.GetMyBookingsAsync();
        response.EnsureSuccessStatusCode();

        var body = await response.Content.ReadFromJsonAsync<MyBookingsApiResponse>();
        Assert.NotNull(body);
        Assert.Empty(body!.Bookings);
    }

    private sealed class MyBookingsApiResponse
    {
        public List<MyBookingApiItem> Bookings { get; set; } = [];
    }

    private sealed class MyBookingApiItem
    {
        public Guid BookingId { get; set; }

        public string DeskNumber { get; set; } = string.Empty;

        public string Status { get; set; } = string.Empty;

        public bool CanCancel { get; set; }
    }
}
