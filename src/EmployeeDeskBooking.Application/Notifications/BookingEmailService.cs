using EmployeeDeskBooking.Application.Bookings;
using EmployeeDeskBooking.Domain.Notifications;

namespace EmployeeDeskBooking.Application.Notifications;

public sealed class BookingEmailService(
    IBookingRepository bookings,
    IEmailSender emailSender,
    IEmailDeliveryLogRepository deliveryLogs) : IBookingEmailService
{
    public Task SendConfirmationAsync(Guid bookingId, CancellationToken cancellationToken = default) =>
        SendAsync(bookingId, EmailType.Confirmation, BookingEmailTemplates.ConfirmationSubject, BookingEmailTemplates.ConfirmationBody, cancellationToken);

    public Task SendCancellationAsync(Guid bookingId, CancellationToken cancellationToken = default) =>
        SendAsync(bookingId, EmailType.Cancellation, BookingEmailTemplates.CancellationSubject, BookingEmailTemplates.CancellationBody, cancellationToken);

    private async Task SendAsync(
        Guid bookingId,
        EmailType emailType,
        Func<BookingEmailDetails, string> subjectFactory,
        Func<BookingEmailDetails, string> bodyFactory,
        CancellationToken cancellationToken)
    {
        var details = await bookings.GetBookingEmailDetailsAsync(bookingId, cancellationToken);
        if (details is null)
        {
            return;
        }

        var message = new EmailMessage
        {
            To = details.RecipientEmail,
            Subject = subjectFactory(details),
            HtmlBody = bodyFactory(details),
        };

        var log = new EmailDeliveryLog
        {
            Id = Guid.NewGuid(),
            BookingId = bookingId,
            UserId = details.UserId,
            EmailType = emailType,
            Recipient = details.RecipientEmail,
            CreatedAt = DateTimeOffset.UtcNow,
        };

        try
        {
            await emailSender.SendAsync(message, cancellationToken);
            log.Status = EmailDeliveryStatus.Sent;
        }
        catch (Exception ex)
        {
            log.Status = EmailDeliveryStatus.Failed;
            log.ErrorMessage = ex.Message;
        }

        await deliveryLogs.AddAsync(log, cancellationToken);
        await deliveryLogs.SaveChangesAsync(cancellationToken);
    }
}

internal static class BookingEmailTemplates
{
    public static string ConfirmationSubject(BookingEmailDetails details) =>
        $"Desk booking confirmed — {details.DeskNumber} on {FormatDate(details.BookingDate)}";

    public static string CancellationSubject(BookingEmailDetails details) =>
        $"Desk booking cancelled — {details.DeskNumber} on {FormatDate(details.BookingDate)}";

    public static string ReminderSubject(BookingEmailDetails details) =>
        $"Reminder: desk {details.DeskNumber} tomorrow ({FormatDate(details.BookingDate)})";

    public static string ConfirmationBody(BookingEmailDetails details) =>
        WrapBody($"Your desk <strong>{details.DeskNumber}</strong> is confirmed for <strong>{FormatDate(details.BookingDate)}</strong>.");

    public static string CancellationBody(BookingEmailDetails details) =>
        WrapBody($"Your booking for desk <strong>{details.DeskNumber}</strong> on <strong>{FormatDate(details.BookingDate)}</strong> has been cancelled.");

    public static string ReminderBody(BookingEmailDetails details) =>
        WrapBody($"Reminder: you have desk <strong>{details.DeskNumber}</strong> booked for <strong>{FormatDate(details.BookingDate)}</strong>.");

    private static string FormatDate(DateOnly date) =>
        date.ToString("dd MMM yyyy");

    private static string WrapBody(string message) =>
        $"<html><body><p>{message}</p></body></html>";
}
