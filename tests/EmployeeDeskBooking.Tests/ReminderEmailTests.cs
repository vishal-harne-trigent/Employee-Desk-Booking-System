using EmployeeDeskBooking.Application.Notifications;
using EmployeeDeskBooking.Application.Time;
using EmployeeDeskBooking.Domain.Bookings;
using EmployeeDeskBooking.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace EmployeeDeskBooking.Tests;

public class ReminderEmailTests(CustomWebApplicationFactory factory) : IClassFixture<CustomWebApplicationFactory>
{
    private static readonly DateOnly Today = BookDeskTestClient.FixedToday;
    private static readonly DateOnly Tomorrow = Today.AddDays(1);

    private async Task ResetAsync() => await factory.ResetBookingsAsync();

    [Fact(DisplayName = "Day-before reminder for confirmed future booking (US-007/AC-04)")]
    public async Task Day_before_reminder_US_007_AC_04()
    {
        await ResetAsync();
        using (var scope = factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var employee = db.Users.Single(u => u.Email == "employee@test.com");
            await BookDeskTestFactoryExtensions.SeedConfirmedBookingAsync(db, employee.Id, "A-01", Tomorrow);
        }

        await ProcessRemindersAsync();

        var email = Assert.Single(factory.GetEmailSender().Sent);
        Assert.Equal("employee@test.com", email.To);
        Assert.Contains("Reminder", email.Subject, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("A-01", email.HtmlBody, StringComparison.Ordinal);
        Assert.Contains(Tomorrow.ToString("dddd, MMMM d, yyyy", System.Globalization.CultureInfo.GetCultureInfo("en-US")), email.HtmlBody, StringComparison.Ordinal);
        Assert.Contains("Action: Day-before reminder", email.HtmlBody, StringComparison.Ordinal);

        using var verifyScope = factory.Services.CreateScope();
        var verifyDb = verifyScope.ServiceProvider.GetRequiredService<AppDbContext>();
        Assert.Single(verifyDb.BookingReminders);
    }

    [Fact(DisplayName = "Reminder skipped for cancelled booking (US-007/AC-04)")]
    public async Task Reminder_skipped_for_cancelled_US_007_AC_04()
    {
        await ResetAsync();
        using (var scope = factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var employee = db.Users.Single(u => u.Email == "employee@test.com");
            await BookDeskTestFactoryExtensions.SeedBookingAsync(db, employee.Id, "A-01", Tomorrow, BookingStatus.Cancelled);
        }

        await ProcessRemindersAsync();
        Assert.Empty(factory.GetEmailSender().Sent);
    }

    [Fact(DisplayName = "Reminder skipped for same-day booking (US-007/AC-04)")]
    public async Task Reminder_skipped_for_same_day_US_007_AC_04()
    {
        await ResetAsync();
        using (var scope = factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var employee = db.Users.Single(u => u.Email == "employee@test.com");
            await BookDeskTestFactoryExtensions.SeedConfirmedBookingAsync(db, employee.Id, "A-01", Today);
        }

        await ProcessRemindersAsync();
        Assert.Empty(factory.GetEmailSender().Sent);
    }

    [Fact(DisplayName = "Reminder sent only once per booking (US-007/AC-04)")]
    public async Task Reminder_idempotent_US_007_AC_04()
    {
        await ResetAsync();
        using (var scope = factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var employee = db.Users.Single(u => u.Email == "employee@test.com");
            await BookDeskTestFactoryExtensions.SeedConfirmedBookingAsync(db, employee.Id, "A-01", Tomorrow);
        }

        await ProcessRemindersAsync();
        await ProcessRemindersAsync();

        Assert.Single(factory.GetEmailSender().Sent);
    }

    [Fact(DisplayName = "Reminder skipped outside configured send window (US-007/AC-04)")]
    public async Task Reminder_skipped_outside_send_window_US_007_AC_04()
    {
        await ResetAsync();
        using (var scope = factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var employee = db.Users.Single(u => u.Email == "employee@test.com");
            await BookDeskTestFactoryExtensions.SeedConfirmedBookingAsync(db, employee.Id, "A-01", Tomorrow);
        }

        using (var scope = factory.Services.CreateScope())
        {
            var clock = scope.ServiceProvider.GetRequiredService<IOfficeClock>();
            if (clock is TestOfficeClock testClock)
            {
                testClock.LocalTime = new TimeOnly(10, 0);
            }

            scope.ServiceProvider.GetRequiredService<InMemoryEmailSender>().Reset();
            var reminderService = scope.ServiceProvider.GetRequiredService<IReminderEmailService>();
            await reminderService.ProcessDueRemindersAsync();
        }

        Assert.Empty(factory.GetEmailSender().Sent);
    }

    [Fact(DisplayName = "Cancellation email when cancelling from My Bookings (US-007/AC-02)")]
    public async Task Cancellation_email_from_my_bookings_US_007_AC_02()
    {
        await ResetAsync();
        using (var scope = factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var employee = db.Users.Single(u => u.Email == "employee@test.com");
            await BookDeskTestFactoryExtensions.SeedConfirmedBookingAsync(db, employee.Id, "A-01", Tomorrow);
        }

        var emailSender = factory.GetEmailSender();
        emailSender.Reset();

        var client = new MyBookingsTestClient(factory);
        await client.LoginAsEmployeeAsync();
        var bookingId = client.GetBookingIdForEmployee("A-01", Tomorrow);
        var response = await client.CancelBookingAsync(bookingId);
        response.EnsureSuccessStatusCode();

        var email = Assert.Single(emailSender.Sent);
        Assert.Equal("employee@test.com", email.To);
        Assert.Contains("cancelled", email.Subject, StringComparison.OrdinalIgnoreCase);
    }

    private async Task ProcessRemindersAsync()
    {
        using var scope = factory.Services.CreateScope();
        var clock = scope.ServiceProvider.GetRequiredService<IOfficeClock>();
        if (clock is TestOfficeClock testClock)
        {
            testClock.LocalTime = new TimeOnly(8, 0);
        }

        var reminderService = scope.ServiceProvider.GetRequiredService<IReminderEmailService>();
        await reminderService.ProcessDueRemindersAsync();
    }
}
