using System.Globalization;
using EmployeeDeskBooking.Application.Notifications;

namespace EmployeeDeskBooking.Tests;

public class BookingEmailTemplateTests
{
    private static readonly CultureInfo EmailCulture = CultureInfo.GetCultureInfo("en-US");

    private static BookingEmailDetails SampleDetails =>
        new()
        {
            BookingId = Guid.NewGuid(),
            UserId = Guid.NewGuid(),
            RecipientEmail = "vishal_h@trigent.com",
            EmployeeName = "Vishal Harne",
            DeskNumber = "A-01",
            BookingDate = new DateOnly(2026, 8, 24),
        };

    [Fact(DisplayName = "Confirmation email uses approved subject and body template")]
    public void Confirmation_template_matches_spec()
    {
        var details = SampleDetails;

        Assert.Equal(
            "Desk booking confirmed — A-01 — Floor 1, Zone C on 2026-08-24",
            BookingEmailTemplates.ConfirmationSubject(details));

        var body = BookingEmailTemplates.ConfirmationBody(details);
        Assert.Contains("Hello Vishal Harne,", body, StringComparison.Ordinal);
        Assert.Contains("Your desk reservation is confirmed.", body, StringComparison.Ordinal);
        Assert.Contains("Desk number: A-01 — Floor 1, Zone C", body, StringComparison.Ordinal);
        Assert.Contains("Monday, August 24, 2026", body, StringComparison.Ordinal);
        Assert.Contains("Action: Booking confirmed", body, StringComparison.Ordinal);
        Assert.Contains("Employee Desk Booking System", body, StringComparison.Ordinal);
    }

    [Fact(DisplayName = "Cancellation email uses approved subject and body template")]
    public void Cancellation_template_matches_spec()
    {
        var details = new BookingEmailDetails
        {
            BookingId = SampleDetails.BookingId,
            UserId = SampleDetails.UserId,
            RecipientEmail = SampleDetails.RecipientEmail,
            EmployeeName = SampleDetails.EmployeeName,
            DeskNumber = "C-99",
            BookingDate = new DateOnly(2026, 8, 6),
        };

        Assert.Equal(
            "Desk booking cancelled — C-99 on 2026-08-06",
            BookingEmailTemplates.CancellationSubject(details));

        var body = BookingEmailTemplates.CancellationBody(details);
        Assert.Contains("Your desk reservation has been cancelled.", body, StringComparison.Ordinal);
        Assert.Contains("Desk number: C-99", body, StringComparison.Ordinal);
        Assert.Contains("Thursday, August 6, 2026", body, StringComparison.Ordinal);
        Assert.Contains("Action: Booking cancelled", body, StringComparison.Ordinal);
    }

    [Fact(DisplayName = "Reminder email uses approved subject and body template")]
    public void Reminder_template_matches_spec()
    {
        var details = new BookingEmailDetails
        {
            BookingId = SampleDetails.BookingId,
            UserId = SampleDetails.UserId,
            RecipientEmail = SampleDetails.RecipientEmail,
            EmployeeName = SampleDetails.EmployeeName,
            DeskNumber = "A-03",
            BookingDate = new DateOnly(2026, 8, 6),
        };

        Assert.Equal(
            "Reminder: office desk tomorrow — A-03 on 2026-08-06",
            BookingEmailTemplates.ReminderSubject(details));

        var body = BookingEmailTemplates.ReminderBody(details);
        Assert.Contains("friendly reminder that you have a desk booked for tomorrow.", body, StringComparison.Ordinal);
        Assert.Contains("Desk number: A-03", body, StringComparison.Ordinal);
        Assert.Contains("Thursday, August 6, 2026", body, StringComparison.Ordinal);
        Assert.Contains("Action: Day-before reminder", body, StringComparison.Ordinal);
    }
}
