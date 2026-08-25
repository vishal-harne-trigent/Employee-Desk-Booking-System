using EmployeeDeskBooking.Application.Notifications;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace EmployeeDeskBooking.Infrastructure.Notifications;

public sealed class ReminderEmailHostedService(
    IServiceScopeFactory scopeFactory,
    ILogger<ReminderEmailHostedService> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var timer = new PeriodicTimer(TimeSpan.FromHours(1));

        while (await timer.WaitForNextTickAsync(stoppingToken))
        {
            try
            {
                using var scope = scopeFactory.CreateScope();
                var reminderService = scope.ServiceProvider.GetRequiredService<IReminderEmailService>();
                await reminderService.ProcessDueRemindersAsync(stoppingToken);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                logger.LogError(ex, "Reminder email job failed.");
            }
        }
    }
}
