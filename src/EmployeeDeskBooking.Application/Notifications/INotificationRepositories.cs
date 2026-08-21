using EmployeeDeskBooking.Application.Notifications;
using EmployeeDeskBooking.Domain.Notifications;

namespace EmployeeDeskBooking.Application.Notifications;

public interface IEmailDeliveryLogRepository
{
    Task AddAsync(EmailDeliveryLog log, CancellationToken cancellationToken = default);

    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}

public interface IBookingReminderRepository
{
    Task<bool> ExistsAsync(Guid bookingId, CancellationToken cancellationToken = default);

    Task AddAsync(BookingReminder reminder, CancellationToken cancellationToken = default);

    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
