using EmployeeDeskBooking.Application.Notifications;
using EmployeeDeskBooking.Domain.Notifications;
using EmployeeDeskBooking.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace EmployeeDeskBooking.Infrastructure.Notifications;

public sealed class EfNotificationPreferenceRepository(AppDbContext dbContext) : INotificationPreferenceRepository
{
    public Task<NotificationPreference?> GetByUserIdAsync(
        Guid userId,
        CancellationToken cancellationToken = default) =>
        dbContext.NotificationPreferences.AsNoTracking()
            .FirstOrDefaultAsync(preference => preference.UserId == userId, cancellationToken);

    public async Task UpsertAsync(
        NotificationPreference preference,
        CancellationToken cancellationToken = default)
    {
        var existing = await dbContext.NotificationPreferences
            .FirstOrDefaultAsync(p => p.UserId == preference.UserId, cancellationToken);

        if (existing is null)
        {
            dbContext.NotificationPreferences.Add(preference);
            return;
        }

        existing.PushOptIn = preference.PushOptIn;
        existing.PushSubscription = preference.PushSubscription;
        existing.UpdatedAt = preference.UpdatedAt;
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken = default) =>
        dbContext.SaveChangesAsync(cancellationToken);
}
