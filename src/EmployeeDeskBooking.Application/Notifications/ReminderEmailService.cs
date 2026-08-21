using EmployeeDeskBooking.Application.Bookings;
using EmployeeDeskBooking.Application.Time;
using EmployeeDeskBooking.Domain.Notifications;

namespace EmployeeDeskBooking.Application.Notifications;

public sealed class ReminderEmailService(
    IBookingRepository bookings,
    IOfficeClock officeClock,
    IEmailSender emailSender,
    IEmailDeliveryLogRepository deliveryLogs,
    IBookingReminderRepository reminders) : IReminderEmailService
{
    public async Task ProcessDueRemindersAsync(CancellationToken cancellationToken = default)
    {
        var today = officeClock.Today;
        var tomorrow = today.AddDays(1);

        if (!officeClock.IsWorkingDay(tomorrow))
        {
            return;
        }

        var dueBookings = await bookings.GetConfirmedBookingEmailDetailsForDateAsync(tomorrow, cancellationToken);

        foreach (var details in dueBookings)
        {
            if (await reminders.ExistsAsync(details.BookingId, cancellationToken))
            {
                continue;
            }

            var message = new EmailMessage
            {
                To = details.RecipientEmail,
                Subject = BookingEmailTemplates.ReminderSubject(details),
                HtmlBody = BookingEmailTemplates.ReminderBody(details),
            };

            var log = new EmailDeliveryLog
            {
                Id = Guid.NewGuid(),
                BookingId = details.BookingId,
                UserId = details.UserId,
                EmailType = EmailType.Reminder,
                Recipient = details.RecipientEmail,
                CreatedAt = DateTimeOffset.UtcNow,
            };

            try
            {
                await emailSender.SendAsync(message, cancellationToken);
                log.Status = EmailDeliveryStatus.Sent;

                var now = DateTimeOffset.UtcNow;
                await reminders.AddAsync(new BookingReminder
                {
                    BookingId = details.BookingId,
                    SentAt = now,
                    CreatedAt = now,
                }, cancellationToken);
                await reminders.SaveChangesAsync(cancellationToken);
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
}
