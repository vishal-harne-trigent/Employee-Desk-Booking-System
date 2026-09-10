using EmployeeDeskBooking.Domain.Bookings;
using EmployeeDeskBooking.Domain.Desks;
using EmployeeDeskBooking.Infrastructure.Data;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace EmployeeDeskBooking.Tests;

public class AdminDesksTests(CustomWebApplicationFactory factory) : IClassFixture<CustomWebApplicationFactory>
{
    private static readonly DateOnly Today = BookDeskTestClient.FixedToday;
    private static readonly DateOnly FutureDate = Today.AddDays(1);

    private async Task ResetAsync() => await factory.ResetBookingsAsync();

    [Fact(DisplayName = "Admin adds desk with unique number (US-005/AC-01)")]
    public async Task Admin_adds_desk_US_005_AC_01()
    {
        var client = new AdminDesksTestClient(factory);
        await client.LoginAsAdminAsync();

        var response = await client.CreateDeskAsync("C-01");
        var body = await response.Content.ReadAsStringAsync();

        response.EnsureSuccessStatusCode();
        Assert.Contains("C-01", body, StringComparison.Ordinal);
        Assert.Contains("Active", body, StringComparison.Ordinal);
        Assert.Contains("Floor", body, StringComparison.Ordinal);

        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var desk = await db.Desks.SingleAsync(d => d.DeskNumber == "C-01");
        Assert.Equal(DeskStatus.Active, desk.Status);
    }

    [Fact(DisplayName = "Admin cannot save duplicate desk number (US-005/AC-02)")]
    public async Task Admin_rejects_duplicate_desk_US_005_AC_02()
    {
        var client = new AdminDesksTestClient(factory);
        await client.LoginAsAdminAsync();

        var response = await client.CreateDeskAsync("A-01");
        var body = await response.Content.ReadAsStringAsync();

        response.EnsureSuccessStatusCode();
        Assert.Contains("already in use", body, StringComparison.OrdinalIgnoreCase);
    }

    [Fact(DisplayName = "Admin edits desk number to unique value (US-005/AC-03)")]
    public async Task Admin_edits_desk_number_US_005_AC_03()
    {
        Guid deskId;
        using (var scope = factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            deskId = db.Desks.Single(d => d.DeskNumber == "A-02").Id;
        }

        var client = new AdminDesksTestClient(factory);
        await client.LoginAsAdminAsync();

        var response = await client.EditDeskAsync(deskId, "A-20");
        var body = await response.Content.ReadAsStringAsync();

        response.EnsureSuccessStatusCode();
        Assert.Contains("A-20", body, StringComparison.Ordinal);
        Assert.DoesNotContain("A-02", body, StringComparison.Ordinal);
    }

    [Fact(DisplayName = "Admin deactivates desk without future bookings (US-005/AC-04)")]
    public async Task Admin_deactivates_desk_US_005_AC_04()
    {
        await ResetAsync();
        Guid deskId;
        var client = new AdminDesksTestClient(factory);
        await client.LoginAsAdminAsync();

        await client.CreateDeskAsync("C-02");
        using (var scope = factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            deskId = db.Desks.Single(d => d.DeskNumber == "C-02").Id;
        }

        var deactivateResponse = await client.DeactivateDeskAsync(deskId);
        deactivateResponse.EnsureSuccessStatusCode();

        using (var verifyScope = factory.Services.CreateScope())
        {
            var verifyDb = verifyScope.ServiceProvider.GetRequiredService<AppDbContext>();
            var desk = await verifyDb.Desks.SingleAsync(d => d.Id == deskId);
            Assert.Equal(DeskStatus.Inactive, desk.Status);
        }

        var bookClient = new BookDeskTestClient(factory);
        await bookClient.LoginAsEmployeeAsync();
        var availability = await bookClient.CheckAvailabilityAsync(FutureDate);
        var availabilityBody = await availability.Content.ReadAsStringAsync();
        Assert.DoesNotContain("C-02", availabilityBody, StringComparison.Ordinal);
    }

    [Fact(DisplayName = "Admin cannot deactivate desk with future bookings (US-005/AC-05)")]
    public async Task Admin_blocked_deactivate_with_bookings_US_005_AC_05()
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

        var client = new AdminDesksTestClient(factory);
        await client.LoginAsAdminAsync();

        var response = await client.DeactivateDeskAsync(deskId);
        var body = await response.Content.ReadAsStringAsync();

        response.EnsureSuccessStatusCode();
        Assert.Contains("confirmed bookings", body, StringComparison.OrdinalIgnoreCase);

        using var verifyScope = factory.Services.CreateScope();
        var verifyDb = verifyScope.ServiceProvider.GetRequiredService<AppDbContext>();
        var desk = await verifyDb.Desks.SingleAsync(d => d.Id == deskId);
        Assert.Equal(DeskStatus.Active, desk.Status);
    }

    [Fact(DisplayName = "Admin can set custom desk location on add and edit")]
    public async Task Admin_sets_custom_desk_location()
    {
        var client = new AdminDesksTestClient(factory);
        await client.LoginAsAdminAsync();

        const string customLocation = "Building B, Floor 2";
        var createResponse = await client.CreateDeskAsync("D-01", customLocation);
        createResponse.EnsureSuccessStatusCode();
        var createBody = await createResponse.Content.ReadAsStringAsync();
        Assert.Contains(customLocation, createBody, StringComparison.Ordinal);

        Guid deskId;
        using (var scope = factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var desk = await db.Desks.SingleAsync(d => d.DeskNumber == "D-01");
            deskId = desk.Id;
            Assert.Equal(customLocation, desk.Location);
        }

        const string updatedLocation = "Building C, Open plan";
        var editResponse = await client.EditDeskAsync(deskId, "D-01", updatedLocation);
        editResponse.EnsureSuccessStatusCode();
        var editBody = await editResponse.Content.ReadAsStringAsync();
        Assert.Contains(updatedLocation, editBody, StringComparison.Ordinal);

        using var verifyScope = factory.Services.CreateScope();
        var verifyDb = verifyScope.ServiceProvider.GetRequiredService<AppDbContext>();
        var updatedDesk = await verifyDb.Desks.SingleAsync(d => d.Id == deskId);
        Assert.Equal(updatedLocation, updatedDesk.Location);
    }

    [Fact(DisplayName = "Employee cannot access manage desks page (US-005/V-07)")]
    public async Task Employee_cannot_access_admin_desks_V_07()
    {
        var client = new BookDeskTestClient(factory);
        await client.LoginAsEmployeeAsync();

        var response = await client.Login.Client.GetAsync("/Admin/AdminDesks");
        Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);
        Assert.Contains("/Account/Login", response.Headers.Location?.OriginalString, StringComparison.Ordinal);
    }
}
