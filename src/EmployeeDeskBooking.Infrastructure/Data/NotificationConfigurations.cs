using EmployeeDeskBooking.Domain.Notifications;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EmployeeDeskBooking.Infrastructure.Data;

public class EmailDeliveryLogConfiguration : IEntityTypeConfiguration<EmailDeliveryLog>
{
    public void Configure(EntityTypeBuilder<EmailDeliveryLog> builder)
    {
        builder.ToTable("EmailDeliveryLogs");
        builder.HasKey(log => log.Id);
        builder.Property(log => log.Recipient).HasMaxLength(320).IsRequired();
        builder.Property(log => log.EmailType).HasConversion<byte>();
        builder.Property(log => log.Status).HasConversion<byte>();
        builder.Property(log => log.ErrorMessage);
        builder.Property(log => log.CreatedAt).IsRequired();
    }
}

public class BookingReminderConfiguration : IEntityTypeConfiguration<BookingReminder>
{
    public void Configure(EntityTypeBuilder<BookingReminder> builder)
    {
        builder.ToTable("BookingReminders");
        builder.HasKey(reminder => reminder.BookingId);
        builder.Property(reminder => reminder.SentAt).IsRequired();
        builder.Property(reminder => reminder.CreatedAt).IsRequired();
    }
}

public class NotificationPreferenceConfiguration : IEntityTypeConfiguration<NotificationPreference>
{
    public void Configure(EntityTypeBuilder<NotificationPreference> builder)
    {
        builder.ToTable("NotificationPreferences");
        builder.HasKey(preference => preference.UserId);
        builder.Property(preference => preference.PushOptIn).HasDefaultValue(false);
        builder.Property(preference => preference.PushSubscription);
        builder.Property(preference => preference.UpdatedAt).IsRequired();
    }
}
