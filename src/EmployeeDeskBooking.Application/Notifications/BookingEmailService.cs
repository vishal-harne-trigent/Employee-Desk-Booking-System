using EmployeeDeskBooking.Application.Bookings;
using EmployeeDeskBooking.Domain.Notifications;

namespace EmployeeDeskBooking.Application.Notifications;

public sealed class BookingEmailService(
    IBookingRepository bookings,
    IEmailSender emailSender,
    IEmailDeliveryLogRepository deliveryLogs) : IBookingEmailService
{
    public Task<bool> SendConfirmationAsync(Guid bookingId, CancellationToken cancellationToken = default) =>
        SendAsync(bookingId, EmailType.Confirmation, BookingEmailTemplates.ConfirmationSubject, BookingEmailTemplates.ConfirmationBody, cancellationToken);

    public Task<bool> SendCancellationAsync(Guid bookingId, CancellationToken cancellationToken = default) =>
        SendAsync(bookingId, EmailType.Cancellation, BookingEmailTemplates.CancellationSubject, BookingEmailTemplates.CancellationBody, cancellationToken);

    private async Task<bool> SendAsync(
        Guid bookingId,
        EmailType emailType,
        Func<BookingEmailDetails, string> subjectFactory,
        Func<BookingEmailDetails, string> bodyFactory,
        CancellationToken cancellationToken)
    {
        var details = await bookings.GetBookingEmailDetailsAsync(bookingId, cancellationToken);
        if (details is null)
        {
            return false;
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
        return log.Status == EmailDeliveryStatus.Sent;
    }
}
