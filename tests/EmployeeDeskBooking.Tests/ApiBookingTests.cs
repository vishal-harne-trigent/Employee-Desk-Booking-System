using System.Net.Http.Json;
using EmployeeDeskBooking.Api.Contracts.Bookings;
using EmployeeDeskBooking.Domain.Bookings;
using EmployeeDeskBooking.Infrastructure.Data;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace EmployeeDeskBooking.Tests;

public class ApiBookingTests(CustomApiApplicationFactory factory) : IClassFixture<CustomApiApplicationFactory>
{
    private static readonly DateOnly ValidDate = ApiTestClient.FixedToday.AddDays(1);

    private async Task ResetAsync() => await factory.ResetBookingsAsync();

    [Fact(DisplayName = "API loads availability for valid date (US-002/AC-01)")]
    public async Task Api_loads_availability_for_valid_date_US_002_AC_01()
    {
        await ResetAsync();
        var client = new ApiTestClient(factory);
        await client.AuthorizeAsEmployeeAsync();

        var response = await client.GetAvailabilityAsync(ValidDate);
        var body = await response.Content.ReadFromJsonAsync<AvailabilityResponse>();

        response.EnsureSuccessStatusCode();
        Assert.NotNull(body);
        Assert.Contains(body.Desks, d => d.DeskNumber == "A-01");
    }

    [Fact(DisplayName = "API rejects invalid dates (US-002/AC-02)")]
    public async Task Api_rejects_invalid_dates_US_002_AC_02()
    {
        await ResetAsync();
        var client = new ApiTestClient(factory);
        await client.AuthorizeAsEmployeeAsync();

        var weekend = ApiTestClient.FixedToday.AddDays(4);
        var weekendResponse = await client.GetAvailabilityAsync(weekend);
        Assert.Equal(StatusCodes.Status422UnprocessableEntity, (int)weekendResponse.StatusCode);
        var weekendDetail = await ApiTestClient.ReadProblemDetailAsync(weekendResponse);
        Assert.Contains("working days", weekendDetail, StringComparison.OrdinalIgnoreCase);

        var past = ApiTestClient.FixedToday.AddDays(-1);
        var pastResponse = await client.GetAvailabilityAsync(past);
        var pastDetail = await ApiTestClient.ReadProblemDetailAsync(pastResponse);
        Assert.Contains("future date", pastDetail, StringComparison.OrdinalIgnoreCase);

        var beyond = ApiTestClient.FixedToday.AddDays(31);
        var beyondResponse = await client.GetAvailabilityAsync(beyond);
        var beyondDetail = await ApiTestClient.ReadProblemDetailAsync(beyondResponse);
        Assert.Contains("30 calendar days", beyondDetail, StringComparison.Ordinal);
    }

    [Fact(DisplayName = "API shows desk availability status (US-002/AC-03)")]
    public async Task Api_shows_desk_availability_status_US_002_AC_03()
    {
        await ResetAsync();
        using (var scope = factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var admin = db.Users.Single(u => u.Email == "admin@test.com");
            await BookDeskTestFactoryExtensions.SeedConfirmedBookingAsync(db, admin.Id, "A-02", ValidDate);
        }

        var client = new ApiTestClient(factory);
        await client.AuthorizeAsEmployeeAsync();

        var response = await client.GetAvailabilityAsync(ValidDate);
        var body = await response.Content.ReadFromJsonAsync<AvailabilityResponse>();

        response.EnsureSuccessStatusCode();
        Assert.NotNull(body);
        Assert.Contains(body.Desks, d => d.DeskNumber == "A-01" && d.IsAvailable);
        Assert.Contains(body.Desks, d => d.DeskNumber == "A-02" && !d.IsAvailable);
    }

    [Fact(DisplayName = "API creates confirmed booking (US-002/AC-04)")]
    public async Task Api_creates_confirmed_booking_US_002_AC_04()
    {
        await ResetAsync();
        var client = new ApiTestClient(factory);
        await client.AuthorizeAsEmployeeAsync();

        var deskId = client.GetDeskIdByNumber("A-01");
        var response = await client.CreateBookingAsync(deskId, ValidDate);
        var body = await response.Content.ReadFromJsonAsync<CreateBookingResponse>();

        Assert.Equal(StatusCodes.Status201Created, (int)response.StatusCode);
        Assert.NotNull(body);

        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var employee = db.Users.Single(u => u.Email == "employee@test.com");
        Assert.True(db.Bookings.Any(b =>
            b.UserId == employee.Id &&
            b.BookingDate == ValidDate &&
            b.Status == BookingStatus.Confirmed));
    }

    [Fact(DisplayName = "API rejects double booking same day (US-002/AC-05)")]
    public async Task Api_rejects_double_booking_same_day_US_002_AC_05()
    {
        await ResetAsync();
        var client = new ApiTestClient(factory);
        await client.AuthorizeAsEmployeeAsync();

        var firstDesk = client.GetDeskIdByNumber("A-01");
        await client.CreateBookingAsync(firstDesk, ValidDate);

        var secondDesk = client.GetDeskIdByNumber("A-02");
        var response = await client.CreateBookingAsync(secondDesk, ValidDate);
        var detail = await ApiTestClient.ReadProblemDetailAsync(response);

        Assert.Equal(StatusCodes.Status409Conflict, (int)response.StatusCode);
        Assert.Contains("already have a desk booked", detail, StringComparison.OrdinalIgnoreCase);
    }

    [Fact(DisplayName = "API rejects inactive or taken desks (US-002/AC-06)")]
    public async Task Api_rejects_inactive_or_taken_desks_US_002_AC_06()
    {
        await ResetAsync();
        var client = new ApiTestClient(factory);
        await client.AuthorizeAsEmployeeAsync();

        var inactiveDesk = client.GetDeskIdByNumber("B-01");
        var inactiveResponse = await client.CreateBookingAsync(inactiveDesk, ValidDate);
        var inactiveDetail = await ApiTestClient.ReadProblemDetailAsync(inactiveResponse);
        Assert.Contains("not available", inactiveDetail, StringComparison.OrdinalIgnoreCase);

        using (var scope = factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var admin = db.Users.Single(u => u.Email == "admin@test.com");
            await BookDeskTestFactoryExtensions.SeedConfirmedBookingAsync(db, admin.Id, "A-01", ValidDate.AddDays(2));
        }

        var takenDesk = client.GetDeskIdByNumber("A-01");
        var takenResponse = await client.CreateBookingAsync(takenDesk, ValidDate.AddDays(2));
        var takenDetail = await ApiTestClient.ReadProblemDetailAsync(takenResponse);
        Assert.Contains("already booked", takenDetail, StringComparison.OrdinalIgnoreCase);
    }
}
