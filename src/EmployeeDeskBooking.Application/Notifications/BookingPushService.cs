using EmployeeDeskBooking.Application.Bookings;
using EmployeeDeskBooking.Application.Desks;

namespace EmployeeDeskBooking.Application.Notifications;

public sealed class BookingPushService(
    IBookingRepository bookings,
    INotificationPreferenceRepository preferences,
    IPushNotificationSender pushSender) : IBookingPushService
{
    public Task SendConfirmationAsync(Guid bookingId, CancellationToken cancellationToken = default) =>
        SendAsync(
            bookingId,
            details => $"Desk booking confirmed — {FormatDesk(details)}",
            details => $"Desk {FormatDesk(details)} on {FormatDate(details.BookingDate)}",
            cancellationToken);

    public Task SendCancellationAsync(Guid bookingId, CancellationToken cancellationToken = default) =>
        SendAsync(
            bookingId,
            details => $"Desk booking cancelled — {FormatDesk(details)}",
            details => $"Desk {FormatDesk(details)} on {FormatDate(details.BookingDate)} was cancelled.",
            cancellationToken);

    private async Task SendAsync(
        Guid bookingId,
        Func<BookingEmailDetails, string> titleFactory,
        Func<BookingEmailDetails, string> bodyFactory,
        CancellationToken cancellationToken)
    {
        var details = await bookings.GetBookingEmailDetailsAsync(bookingId, cancellationToken);
        if (details is null)
        {
            return;
        }

        var preference = await preferences.GetByUserIdAsync(details.UserId, cancellationToken);
        if (preference is null || !preference.PushOptIn || string.IsNullOrWhiteSpace(preference.PushSubscription))
        {
            return;
        }

        var message = new PushNotificationMessage
        {
            SubscriptionJson = preference.PushSubscription,
            Title = titleFactory(details),
            Body = bodyFactory(details),
        };

        try
        {
            await pushSender.SendAsync(message, cancellationToken);
        }
        catch (Exception)
        {
            // Push is optional; booking already committed.
        }
    }

    private static string FormatDate(DateOnly date) =>
        date.ToString("dd MMM yyyy");

    private static string FormatDesk(BookingEmailDetails details) =>
        DeskLocationFormatter.FormatDeskWithLocation(details.DeskNumber, details.DeskLocation);
}
