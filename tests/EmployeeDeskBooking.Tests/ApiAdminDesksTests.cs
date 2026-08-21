using EmployeeDeskBooking.Domain.Bookings;
using EmployeeDeskBooking.Domain.Desks;
using EmployeeDeskBooking.Infrastructure.Data;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Net.Http.Json;

namespace EmployeeDeskBooking.Tests;

public class ApiAdminDesksTests(CustomApiApplicationFactory factory) : IClassFixture<CustomApiApplicationFactory>
{
    private static readonly DateOnly Today = BookDeskTestClient.FixedToday;
    private static readonly DateOnly FutureDate = Today.AddDays(1);

    private async Task ResetAsync() => await factory.ResetBookingsAsync();

    [Fact(DisplayName = "API admin creates desk with unique number (US-005/AC-01)")]
    public async Task Api_admin_creates_desk_US_005_AC_01()
    {
        var client = new ApiTestClient(factory);
        await client.AuthorizeAsAdminAsync();

        var response = await client.CreateAdminDeskAsync("C-10");
        Assert.Equal(StatusCodes.Status201Created, (int)response.StatusCode);

        var list = await client.GetAdminDesksAsync();
        var body = await list.Content.ReadFromJsonAsync<AdminDesksApiResponse>();
        Assert.NotNull(body);
        Assert.Contains(body!.Desks, d => d.DeskNumber == "C-10" && d.Status == "Active");
    }

    [Fact(DisplayName = "API admin rejects duplicate desk number (US-005/AC-02)")]
    public async Task Api_admin_rejects_duplicate_desk_US_005_AC_02()
    {
        var client = new ApiTestClient(factory);
        await client.AuthorizeAsAdminAsync();

        var response = await client.CreateAdminDeskAsync("A-01");
        Assert.Equal(StatusCodes.Status409Conflict, (int)response.StatusCode);
    }

    [Fact(DisplayName = "API admin updates desk number (US-005/AC-03)")]
    public async Task Api_admin_updates_desk_US_005_AC_03()
    {
        Guid deskId;
        using (var scope = factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            deskId = db.Desks.Single(d => d.DeskNumber == "A-02").Id;
        }

        var client = new ApiTestClient(factory);
        await client.AuthorizeAsAdminAsync();

        var response = await client.UpdateAdminDeskAsync(deskId, "A-21");
        Assert.Equal(StatusCodes.Status200OK, (int)response.StatusCode);

        var list = await client.GetAdminDesksAsync();
        var body = await list.Content.ReadFromJsonAsync<AdminDesksApiResponse>();
        Assert.Contains(body!.Desks, d => d.DeskNumber == "A-21");
    }

    [Fact(DisplayName = "API admin deactivates desk without bookings (US-005/AC-04)")]
    public async Task Api_admin_deactivates_desk_US_005_AC_04()
    {
        await ResetAsync();
        var client = new ApiTestClient(factory);
        await client.AuthorizeAsAdminAsync();

        var create = await client.CreateAdminDeskAsync("C-11");
        create.EnsureSuccessStatusCode();
        var created = await create.Content.ReadFromJsonAsync<AdminDeskMutationApiResponse>();

        var deactivate = await client.DeactivateAdminDeskAsync(created!.DeskId);
        Assert.Equal(StatusCodes.Status200OK, (int)deactivate.StatusCode);

        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var desk = await db.Desks.SingleAsync(d => d.Id == created.DeskId);
        Assert.Equal(DeskStatus.Inactive, desk.Status);

        await client.AuthorizeAsEmployeeAsync();
        var availability = await client.GetAvailabilityAsync(FutureDate);
        var availabilityBody = await availability.Content.ReadFromJsonAsync<AvailabilityApiResponse>();
        Assert.DoesNotContain(availabilityBody!.Desks, d => d.DeskNumber == "C-11");
    }

    [Fact(DisplayName = "API admin blocked from deactivating desk with bookings (US-005/AC-05)")]
    public async Task Api_admin_blocked_deactivate_US_005_AC_05()
    {
        await ResetAsync();
        Guid deskId;
        using (var scope = factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var employee = db.Users.Single(u => u.Email == "employee@test.com");
            deskId = db.Desks.Single(d => d.DeskNumber == "A-01").Id;
            await BookDeskTestFactoryExtensions.SeedBookingAsync(
                db,
                employee.Id,
                "A-01",
                FutureDate,
                BookingStatus.Confirmed);
        }

        var client = new ApiTestClient(factory);
        await client.AuthorizeAsAdminAsync();

        var response = await client.DeactivateAdminDeskAsync(deskId);
        Assert.Equal(StatusCodes.Status409Conflict, (int)response.StatusCode);
    }

    [Fact(DisplayName = "API employee cannot access admin desks (US-005/V-07)")]
    public async Task Api_employee_denied_admin_desks_V_07()
    {
        var client = new ApiTestClient(factory);
        await client.AuthorizeAsEmployeeAsync();

        var response = await client.GetAdminDesksAsync();
        Assert.Equal(StatusCodes.Status403Forbidden, (int)response.StatusCode);
    }

    private sealed class AdminDesksApiResponse
    {
        public List<AdminDeskApiItem> Desks { get; set; } = [];
    }

    private sealed class AdminDeskApiItem
    {
        public Guid DeskId { get; set; }

        public string DeskNumber { get; set; } = string.Empty;

        public string Status { get; set; } = string.Empty;
    }

    private sealed class AdminDeskMutationApiResponse
    {
        public Guid DeskId { get; set; }

        public string Status { get; set; } = string.Empty;
    }

    private sealed class AvailabilityApiResponse
    {
        public List<AvailabilityDeskApiItem> Desks { get; set; } = [];
    }

    private sealed class AvailabilityDeskApiItem
    {
        public string DeskNumber { get; set; } = string.Empty;
    }
}
