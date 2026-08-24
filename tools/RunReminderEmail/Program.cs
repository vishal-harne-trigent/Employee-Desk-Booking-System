using EmployeeDeskBooking.Application;
using EmployeeDeskBooking.Application.Notifications;
using EmployeeDeskBooking.Application.Time;
using EmployeeDeskBooking.Infrastructure;
using EmployeeDeskBooking.Infrastructure.Time;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

var webProjectPath = ResolveWebProjectPath();

var configuration = new ConfigurationBuilder()
    .SetBasePath(webProjectPath)
    .AddJsonFile("appsettings.json", optional: true)
    .AddJsonFile("appsettings.Development.json", optional: true)
    .AddJsonFile("appsettings.Development.local.json", optional: true)
    .AddUserSecrets("EmployeeDeskBooking.Web-dev")
    .AddEnvironmentVariables()
    .Build();

var reminderHour = configuration.GetValue("Email:ReminderHourLocal", 8);
var runAt = configuration["ReminderJob:RunAtLocalTime"];
if (!string.IsNullOrWhiteSpace(runAt) && TimeOnly.TryParse(runAt, out var parsed))
{
    reminderHour = parsed.Hour;
}

var services = new ServiceCollection();
services.AddLogging(builder => builder.AddConsole().SetMinimumLevel(LogLevel.Information));
services.AddSingleton<IConfiguration>(configuration);
services.AddApplication();
services.AddInfrastructure(configuration, enableReminderJob: false, enableCompletionJob: false);

services.AddSingleton<IOfficeClock>(_ =>
    new ForcedReminderWindowClock(new OfficeClock(configuration), new TimeOnly(reminderHour, 0)));

var provider = services.BuildServiceProvider();
var reminderService = provider.GetRequiredService<IReminderEmailService>();
var officeClock = provider.GetRequiredService<IOfficeClock>();

Console.WriteLine($"Office today: {officeClock.Today}");
Console.WriteLine($"Reminder target date: {officeClock.Today.AddDays(1)} (tomorrow)");
Console.WriteLine($"Simulated local send window: {reminderHour:D2}:00");
Console.WriteLine("Processing due reminder emails...");

try
{
    await reminderService.ProcessDueRemindersAsync();
    Console.WriteLine("Done. Check inbox, App_Data/sent-emails, or EmailDeliveryLogs for results.");
    return 0;
}
catch (Exception ex)
{
    Console.Error.WriteLine($"Failed: {ex.Message}");
    return 1;
}

static string ResolveWebProjectPath()
{
    var candidates = new[]
    {
        Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), "..", "..", "src", "EmployeeDeskBooking.Web")),
        Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), "src", "EmployeeDeskBooking.Web")),
        Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "..", "src", "EmployeeDeskBooking.Web")),
    };

    foreach (var candidate in candidates)
    {
        if (Directory.Exists(candidate))
        {
            return candidate;
        }
    }

    throw new DirectoryNotFoundException("Could not locate src/EmployeeDeskBooking.Web.");
}

internal sealed class ForcedReminderWindowClock(OfficeClock inner, TimeOnly localTime) : IOfficeClock
{
    public DateOnly Today => inner.Today;

    public TimeOnly LocalTime => localTime;

    public bool IsWorkingDay(DateOnly date) => inner.IsWorkingDay(date);

    public bool IsWithinBookingWindow(DateOnly date) => inner.IsWithinBookingWindow(date);
}
