using EmployeeDeskBooking.Application.Notifications;
using EmployeeDeskBooking.Domain.Notifications;
using EmployeeDeskBooking.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace EmployeeDeskBooking.Infrastructure.Notifications;

public sealed class EfEmailDeliveryLogRepository(AppDbContext dbContext) : IEmailDeliveryLogRepository
{
    public Task AddAsync(EmailDeliveryLog log, CancellationToken cancellationToken = default)
    {
        dbContext.EmailDeliveryLogs.Add(log);
        return Task.CompletedTask;
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken = default) =>
        dbContext.SaveChangesAsync(cancellationToken);
}

public sealed class EfBookingReminderRepository(AppDbContext dbContext) : IBookingReminderRepository
{
    public Task<bool> ExistsAsync(Guid bookingId, CancellationToken cancellationToken = default) =>
        dbContext.BookingReminders.AsNoTracking().AnyAsync(r => r.BookingId == bookingId, cancellationToken);

    public Task AddAsync(BookingReminder reminder, CancellationToken cancellationToken = default)
    {
        dbContext.BookingReminders.Add(reminder);
        return Task.CompletedTask;
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken = default) =>
        dbContext.SaveChangesAsync(cancellationToken);
}
