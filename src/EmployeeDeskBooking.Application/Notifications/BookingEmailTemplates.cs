using System.Globalization;
using EmployeeDeskBooking.Application.Desks;

namespace EmployeeDeskBooking.Application.Notifications;

public static class BookingEmailTemplates
{
    private static readonly CultureInfo EmailCulture = CultureInfo.GetCultureInfo("en-US");

    public static string ConfirmationSubject(BookingEmailDetails details) =>
        $"Desk booking confirmed — {FormatDesk(details)} on {FormatSubjectDate(details.BookingDate)}";

    public static string CancellationSubject(BookingEmailDetails details) =>
        $"Desk booking cancelled — {details.DeskNumber} on {FormatSubjectDate(details.BookingDate)}";

    public static string ReminderSubject(BookingEmailDetails details) =>
        $"Reminder: office desk tomorrow — {details.DeskNumber} on {FormatSubjectDate(details.BookingDate)}";

    public static string ConfirmationBody(BookingEmailDetails details) =>
        BuildBody(
            details,
            intro: "Your desk reservation is confirmed. We look forward to seeing you in the office.",
            deskNumber: FormatDesk(details),
            action: "Booking confirmed");

    public static string CancellationBody(BookingEmailDetails details) =>
        BuildBody(
            details,
            intro: "Your desk reservation has been cancelled. The desk is now available for others on this date.",
            deskNumber: details.DeskNumber,
            action: "Booking cancelled");

    public static string ReminderBody(BookingEmailDetails details) =>
        BuildBody(
            details,
            intro: "This is a friendly reminder that you have a desk booked for tomorrow.",
            deskNumber: details.DeskNumber,
            action: "Day-before reminder");

    private static string BuildBody(
        BookingEmailDetails details,
        string intro,
        string deskNumber,
        string action)
    {
        var employeeName = EscapeHtml(details.EmployeeName);
        var escapedDeskNumber = EscapeHtml(deskNumber);
        var bookingDate = EscapeHtml(FormatBodyDate(details.BookingDate));
        var escapedAction = EscapeHtml(action);

        return $"""
            <html><body>
            <p>Hello {employeeName},</p>
            <p>{EscapeHtml(intro)}</p>
            <p>
            Employee: {employeeName}<br />
            Desk number: {escapedDeskNumber}<br />
            Booking date: {bookingDate}<br />
            Action: {escapedAction}
            </p>
            <p>—<br />Employee Desk Booking System</p>
            </body></html>
            """;
    }

    private static string FormatDesk(BookingEmailDetails details) =>
        DeskLocationFormatter.FormatDeskWithLocation(details.DeskNumber, details.DeskLocation);

    private static string FormatSubjectDate(DateOnly date) =>
        date.ToString("yyyy-MM-dd", EmailCulture);

    private static string FormatBodyDate(DateOnly date) =>
        date.ToString("dddd, MMMM d, yyyy", EmailCulture);

    private static string EscapeHtml(string value) =>
        value
            .Replace("&", "&amp;", StringComparison.Ordinal)
            .Replace("<", "&lt;", StringComparison.Ordinal)
            .Replace(">", "&gt;", StringComparison.Ordinal);
}
