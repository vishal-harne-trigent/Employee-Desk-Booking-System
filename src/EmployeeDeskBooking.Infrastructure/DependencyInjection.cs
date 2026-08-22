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
        bool enableReminderJob = false,
        bool enableCompletionJob = false)
    {
        services.AddDbContext<AppDbContext>(options =>
            options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));

        services.AddScoped<IUserRepository, EfUserRepository>();
        services.AddScoped<IBookingRepository, EfBookingRepository>();
        services.AddScoped<IDeskRepository, EfDeskRepository>();
        services.AddScoped<IEmailDeliveryLogRepository, EfEmailDeliveryLogRepository>();
        services.AddScoped<IBookingReminderRepository, EfBookingReminderRepository>();
        services.AddScoped<INotificationPreferenceRepository, EfNotificationPreferenceRepository>();
        services.AddSingleton<IPasswordVerifier, AspNetPasswordVerifier>();
        services.AddSingleton<IOfficeClock, OfficeClock>();

        services.Configure<EmailOptions>(options =>
        {
            configuration.GetSection("Email").Bind(options);
            BindSmtpOverrides(configuration.GetSection("Smtp"), options);
        });

        services.Configure<ReminderEmailSettings>(options =>
        {
            options.ReminderHourLocal = configuration.GetValue("Email:ReminderHourLocal", 8);
            var runAt = configuration["ReminderJob:RunAtLocalTime"];
            if (!string.IsNullOrWhiteSpace(runAt) && TimeOnly.TryParse(runAt, out var parsed))
            {
                options.ReminderHourLocal = parsed.Hour;
            }
        });

        var emailEnabled = configuration.GetValue("Email:Enabled", false)
            || configuration.GetValue("Smtp:Enabled", false);
        if (emailEnabled)
        {
            var deliveryMode = configuration["Smtp:Mode"]
                ?? configuration["Email:DeliveryMode"]
                ?? EmailOptions.DeliveryModeSmtp;
            if (string.Equals(deliveryMode, EmailOptions.DeliveryModeFileDrop, StringComparison.OrdinalIgnoreCase))
            {
                services.AddScoped<IEmailSender, FileDropEmailSender>();
            }
            else
            {
                services.AddScoped<IEmailSender, MailKitEmailSender>();
            }
        }
        else
        {
            services.AddScoped<IEmailSender, NoOpEmailSender>();
        }

        if (enableReminderJob)
        {
            services.AddHostedService<ReminderEmailHostedService>();
        }

        if (enableCompletionJob)
        {
            services.AddHostedService<CompletePastBookingsHostedService>();
        }

        services.Configure<VapidOptions>(options =>
        {
            configuration.GetSection("Push").Bind(options);
            BindVapidOverrides(configuration.GetSection("Vapid"), options);
        });

        var pushEnabled = configuration.GetValue("Push:Enabled", false)
            || configuration.GetValue("Vapid:Enabled", false);
        if (pushEnabled)
        {
            services.AddScoped<IPushNotificationSender, WebPushNotificationSender>();
        }
        else
        {
            services.AddScoped<IPushNotificationSender, NoOpPushNotificationSender>();
        }

        return services;
    }

    private static void BindSmtpOverrides(IConfigurationSection smtp, EmailOptions options)
    {
        if (!smtp.Exists())
        {
            return;
        }

        options.Enabled = smtp.GetValue("Enabled", options.Enabled);
        options.DeliveryMode = smtp["Mode"] ?? options.DeliveryMode;
        options.FileDropPath = smtp["FileDropPath"] ?? options.FileDropPath;
        options.SmtpHost = smtp["Host"] ?? options.SmtpHost;
        options.SmtpPort = smtp.GetValue("Port", options.SmtpPort);
        options.UseStartTls = smtp.GetValue("UseSsl", options.UseStartTls);
        options.FromName = smtp["FromName"] ?? options.FromName;
        options.FromAddress = smtp["FromAddress"] ?? options.FromAddress;
        options.Username = smtp["Username"] ?? options.Username;
        options.Password = smtp["Password"] ?? options.Password;
    }

    private static void BindVapidOverrides(IConfigurationSection vapid, VapidOptions options)
    {
        if (!vapid.Exists())
        {
            return;
        }

        options.Enabled = vapid.GetValue("Enabled", options.Enabled);
        options.Subject = vapid["Subject"] ?? options.Subject;
        options.PublicKey = vapid["PublicKey"] ?? options.PublicKey;
        options.PrivateKey = vapid["PrivateKey"] ?? options.PrivateKey;
    }

    public static async Task InitializeDatabaseAsync(IServiceProvider services, bool isDevelopment = false)
    {
        using var scope = services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        await dbContext.Database.MigrateAsync();
        await DbInitializer.SeedAsync(scope.ServiceProvider, isDevelopment);
    }
}
