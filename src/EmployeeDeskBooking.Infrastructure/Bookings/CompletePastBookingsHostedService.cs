using EmployeeDeskBooking.Application.Bookings;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace EmployeeDeskBooking.Infrastructure.Bookings;

public sealed class CompletePastBookingsHostedService(
    IServiceScopeFactory scopeFactory,
    ILogger<CompletePastBookingsHostedService> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var timer = new PeriodicTimer(TimeSpan.FromHours(1));

        while (await timer.WaitForNextTickAsync(stoppingToken))
        {
            try
            {
                using var scope = scopeFactory.CreateScope();
                var completionService = scope.ServiceProvider.GetRequiredService<IBookingCompletionService>();
                var completed = await completionService.CompletePastBookingsAsync(stoppingToken);
                if (completed > 0)
                {
                    logger.LogInformation("Completed {Count} past bookings.", completed);
                }
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                logger.LogError(ex, "Booking completion job failed.");
            }
        }
    }
}
