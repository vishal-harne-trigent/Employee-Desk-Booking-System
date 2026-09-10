namespace EmployeeDeskBooking.Infrastructure.Notifications;

public sealed class ReminderJobOptions
{
    public const string SectionName = "ReminderJob";

    public bool Enabled { get; set; } = true;

    public string RunAtLocalTime { get; set; } = "08:00";
}
