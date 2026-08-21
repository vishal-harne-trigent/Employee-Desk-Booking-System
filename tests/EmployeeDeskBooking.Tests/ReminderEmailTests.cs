using EmployeeDeskBooking.Application.Notifications;
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
        Assert.Contains(Tomorrow.ToString("dd MMM yyyy"), email.HtmlBody, StringComparison.Ordinal);

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

    private async Task ProcessRemindersAsync()
    {
        using var scope = factory.Services.CreateScope();
        var reminderService = scope.ServiceProvider.GetRequiredService<IReminderEmailService>();
        await reminderService.ProcessDueRemindersAsync();
    }
}
