using EmployeeDeskBooking.Application.Bookings;
using EmployeeDeskBooking.Domain.Notifications;
using EmployeeDeskBooking.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace EmployeeDeskBooking.Tests;

public class BookingEmailTests(CustomWebApplicationFactory factory) : IClassFixture<CustomWebApplicationFactory>
{
    private static readonly DateOnly BookingDate = BookDeskTestClient.FixedToday.AddDays(1);

    private async Task ResetAsync() => await factory.ResetBookingsAsync();

    [Fact(DisplayName = "Confirmation email on successful book (US-007/AC-01)")]
    public async Task Confirmation_email_on_book_US_007_AC_01()
    {
        await ResetAsync();
        var client = new BookDeskTestClient(factory);
        await client.LoginAsEmployeeAsync();

        var deskId = client.GetDeskIdByNumber("A-01");
        var response = await client.BookDeskAsync(deskId, BookingDate);
        response.EnsureSuccessStatusCode();

        var emailSender = factory.GetEmailSender();
        var email = Assert.Single(emailSender.Sent);
        Assert.Equal("employee@test.com", email.To);
        Assert.Contains("confirmed", email.Subject, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("A-01", email.HtmlBody, StringComparison.Ordinal);
        Assert.Contains("Test Employee", email.HtmlBody, StringComparison.Ordinal);
        Assert.Contains(BookingDate.ToString("dddd, MMMM d, yyyy", System.Globalization.CultureInfo.GetCultureInfo("en-US")), email.HtmlBody, StringComparison.Ordinal);
        Assert.Contains("Action: Booking confirmed", email.HtmlBody, StringComparison.Ordinal);
    }

    [Fact(DisplayName = "Cancellation email on employee cancel (US-007/AC-02)")]
    public async Task Cancellation_email_on_employee_cancel_US_007_AC_02()
    {
        await ResetAsync();
        var webClient = new BookDeskTestClient(factory);
        await webClient.LoginAsEmployeeAsync();

        var deskId = webClient.GetDeskIdByNumber("A-01");
        await webClient.BookDeskAsync(deskId, BookingDate);

        var emailSender = factory.GetEmailSender();
        emailSender.Reset();

        Guid bookingId;
        Guid employeeId;
        using (var scope = factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var employee = db.Users.Single(u => u.Email == "employee@test.com");
            employeeId = employee.Id;
            bookingId = db.Bookings.Single(b => b.BookingDate == BookingDate).Id;
        }

        using (var scope = factory.Services.CreateScope())
        {
            var bookingService = scope.ServiceProvider.GetRequiredService<IBookingService>();
            var result = await bookingService.CancelBookingAsync(employeeId, bookingId, employeeId);
            Assert.True(result.Succeeded);
        }

        var email = Assert.Single(emailSender.Sent);
        Assert.Equal("employee@test.com", email.To);
        Assert.Contains("cancelled", email.Subject, StringComparison.OrdinalIgnoreCase);
    }

    [Fact(DisplayName = "Cancellation email on admin cancel (US-007/AC-02)")]
    public async Task Cancellation_email_on_admin_cancel_US_007_AC_02_admin()
    {
        await ResetAsync();
        Guid bookingId;
        using (var scope = factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var employee = db.Users.Single(u => u.Email == "employee@test.com");
            await BookDeskTestFactoryExtensions.SeedConfirmedBookingAsync(db, employee.Id, "A-01", BookingDate);
            bookingId = db.Bookings.Single().Id;
        }

        var emailSender = factory.GetEmailSender();
        emailSender.Reset();

        var adminClient = new AdminBookingsTestClient(factory);
        await adminClient.LoginAsAdminAsync();
        var response = await adminClient.CancelBookingAsync(bookingId, null, null);
        response.EnsureSuccessStatusCode();

        var email = Assert.Single(emailSender.Sent);
        Assert.Equal("employee@test.com", email.To);
        Assert.Contains("cancelled", email.Subject, StringComparison.OrdinalIgnoreCase);
    }

    [Fact(DisplayName = "Email body includes desk number and date (US-007/AC-03)")]
    public async Task Email_includes_desk_and_date_US_007_AC_03()
    {
        await ResetAsync();
        var client = new BookDeskTestClient(factory);
        await client.LoginAsEmployeeAsync();

        var deskId = client.GetDeskIdByNumber("A-02");
        await client.BookDeskAsync(deskId, BookingDate);

        var email = Assert.Single(factory.GetEmailSender().Sent);
        Assert.Contains("A-02", email.HtmlBody, StringComparison.Ordinal);
        Assert.Contains(BookingDate.ToString("dddd, MMMM d, yyyy", System.Globalization.CultureInfo.GetCultureInfo("en-US")), email.HtmlBody, StringComparison.Ordinal);
        Assert.Contains("A-02 — Floor 1, Zone C", email.Subject, StringComparison.Ordinal);
    }

    [Fact(DisplayName = "SMTP failures logged without blocking booking (US-007/AC-05)")]
    public async Task Smtp_failure_logged_booking_succeeds_US_007_AC_05()
    {
        await ResetAsync();
        var emailSender = factory.GetEmailSender();
        emailSender.FailNext = true;

        var client = new BookDeskTestClient(factory);
        await client.LoginAsEmployeeAsync();

        var deskId = client.GetDeskIdByNumber("A-01");
        var response = await client.BookDeskAsync(deskId, BookingDate);
        response.EnsureSuccessStatusCode();

        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        Assert.Single(db.Bookings.Where(b => b.BookingDate == BookingDate));

        var log = Assert.Single(db.EmailDeliveryLogs);
        Assert.Equal(EmailDeliveryStatus.Failed, log.Status);
        Assert.Equal(EmailType.Confirmation, log.EmailType);
        Assert.False(string.IsNullOrWhiteSpace(log.ErrorMessage));
    }
}
