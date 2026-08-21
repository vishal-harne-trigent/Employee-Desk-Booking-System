using EmployeeDeskBooking.Application.Bookings;
using EmployeeDeskBooking.Application.Desks;
using EmployeeDeskBooking.Application.Notifications;
using EmployeeDeskBooking.Application.Security;
using EmployeeDeskBooking.Application.Time;
using EmployeeDeskBooking.Application.Users;
using EmployeeDeskBooking.Infrastructure.Bookings;
using EmployeeDeskBooking.Infrastructure.Data;
using EmployeeDeskBooking.Infrastructure.Desks;
using EmployeeDeskBooking.Infrastructure.Notifications;
using EmployeeDeskBooking.Infrastructure.Security;
using EmployeeDeskBooking.Infrastructure.Time;
using EmployeeDeskBooking.Infrastructure.Users;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace EmployeeDeskBooking.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration,
        bool enableReminderJob = false)
    {
        services.AddDbContext<AppDbContext>(options =>
            options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));

        services.AddScoped<IUserRepository, EfUserRepository>();
        services.AddScoped<IBookingRepository, EfBookingRepository>();
        services.AddScoped<IDeskRepository, EfDeskRepository>();
        services.AddScoped<IEmailDeliveryLogRepository, EfEmailDeliveryLogRepository>();
        services.AddScoped<IBookingReminderRepository, EfBookingReminderRepository>();
        services.AddSingleton<IPasswordVerifier, AspNetPasswordVerifier>();
        services.AddSingleton<IOfficeClock, OfficeClock>();

        services.Configure<EmailOptions>(configuration.GetSection("Email"));

        if (configuration.GetValue("Email:Enabled", false))
        {
            services.AddScoped<IEmailSender, MailKitEmailSender>();
        }
        else
        {
            services.AddScoped<IEmailSender, NoOpEmailSender>();
        }

        if (enableReminderJob)
        {
            services.AddHostedService<ReminderEmailHostedService>();
        }

        return services;
    }

    public static async Task InitializeDatabaseAsync(IServiceProvider services, bool isDevelopment = false)
    {
        using var scope = services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        await dbContext.Database.MigrateAsync();
        await DbInitializer.SeedAsync(scope.ServiceProvider, isDevelopment);
    }
}
