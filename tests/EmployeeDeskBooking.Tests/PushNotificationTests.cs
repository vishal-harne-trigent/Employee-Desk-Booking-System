using EmployeeDeskBooking.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace EmployeeDeskBooking.Tests;

public class PushNotificationTests(CustomWebApplicationFactory factory) : IClassFixture<CustomWebApplicationFactory>
{
    private static readonly DateOnly BookingDate = BookDeskTestClient.FixedToday.AddDays(1);

    private async Task ResetAsync() => await factory.ResetBookingsAsync();

    [Fact(DisplayName = "Default opt-out sends no push on book (US-008/AC-01)")]
    public async Task Default_opt_out_no_push_on_book_US_008_AC_01()
    {
        await ResetAsync();
        var client = new BookDeskTestClient(factory);
        await client.LoginAsEmployeeAsync();

        var deskId = client.GetDeskIdByNumber("A-01");
        var response = await client.BookDeskAsync(deskId, BookingDate);
        response.EnsureSuccessStatusCode();

        Assert.Empty(factory.GetPushSender().Sent);
    }

    [Fact(DisplayName = "Push on book when opted in (US-008/AC-03)")]
    public async Task Push_on_book_when_opted_in_US_008_AC_03()
    {
        await ResetAsync();
        using (var scope = factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var employee = db.Users.Single(u => u.Email == "employee@test.com");
            await BookDeskTestFactoryExtensions.SeedPushOptInAsync(db, employee.Id);
        }

        var client = new BookDeskTestClient(factory);
        await client.LoginAsEmployeeAsync();

        var deskId = client.GetDeskIdByNumber("A-01");
        await client.BookDeskAsync(deskId, BookingDate);

        var push = Assert.Single(factory.GetPushSender().Sent);
        Assert.Contains("confirmed", push.Title, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("A-01", push.Body, StringComparison.Ordinal);
        Assert.Contains("Floor 1, Zone C", push.Body, StringComparison.Ordinal);
        Assert.Contains(BookingDate.ToString("dd MMM yyyy"), push.Body, StringComparison.Ordinal);
    }

    [Fact(DisplayName = "Push on admin cancel when employee opted in (US-008/AC-03)")]
    public async Task Push_on_admin_cancel_when_opted_in_US_008_AC_03_admin()
    {
        await ResetAsync();
        Guid bookingId;
        using (var scope = factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var employee = db.Users.Single(u => u.Email == "employee@test.com");
            await BookDeskTestFactoryExtensions.SeedPushOptInAsync(db, employee.Id);
            await BookDeskTestFactoryExtensions.SeedConfirmedBookingAsync(db, employee.Id, "A-01", BookingDate);
            bookingId = db.Bookings.Single().Id;
        }

        factory.GetPushSender().Reset();

        var adminClient = new AdminBookingsTestClient(factory);
        await adminClient.LoginAsAdminAsync();
        var response = await adminClient.CancelBookingAsync(bookingId, null, null);
        response.EnsureSuccessStatusCode();

        var push = Assert.Single(factory.GetPushSender().Sent);
        Assert.Contains("cancelled", push.Title, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("Floor 1, Zone C", push.Body, StringComparison.Ordinal);
    }

    [Fact(DisplayName = "Opt out stops subsequent push (US-008/AC-04)")]
    public async Task Opt_out_stops_push_US_008_AC_04()
    {
        await ResetAsync();
        Guid employeeId;
        using (var scope = factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var employee = db.Users.Single(u => u.Email == "employee@test.com");
            employeeId = employee.Id;
            await BookDeskTestFactoryExtensions.SeedPushOptInAsync(db, employee.Id);
        }

        using (var scope = factory.Services.CreateScope())
        {
            var preferenceService = scope.ServiceProvider.GetRequiredService<Application.Notifications.INotificationPreferenceService>();
            await preferenceService.OptOutAsync(employeeId);
        }

        var client = new BookDeskTestClient(factory);
        await client.LoginAsEmployeeAsync();
        await client.BookDeskAsync(client.GetDeskIdByNumber("A-01"), BookingDate);

        Assert.Empty(factory.GetPushSender().Sent);
    }

    [Fact(DisplayName = "Reminder sends no push when opted in (US-008/AC-05)")]
    public async Task Reminder_sends_no_push_when_opted_in_US_008_AC_05()
    {
        await ResetAsync();
        var tomorrow = BookDeskTestClient.FixedToday.AddDays(1);
        using (var scope = factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var employee = db.Users.Single(u => u.Email == "employee@test.com");
            await BookDeskTestFactoryExtensions.SeedPushOptInAsync(db, employee.Id);
            await BookDeskTestFactoryExtensions.SeedConfirmedBookingAsync(db, employee.Id, "A-01", tomorrow);
        }

        using (var scope = factory.Services.CreateScope())
        {
            var reminderService = scope.ServiceProvider.GetRequiredService<Application.Notifications.IReminderEmailService>();
            await reminderService.ProcessDueRemindersAsync();
        }

        Assert.Single(factory.GetEmailSender().Sent);
        Assert.Empty(factory.GetPushSender().Sent);
    }
}
