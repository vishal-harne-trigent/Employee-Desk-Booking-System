using EmployeeDeskBooking.Application.Bookings;
using EmployeeDeskBooking.Application.Desks;
using EmployeeDeskBooking.Application.Security;
using EmployeeDeskBooking.Application.Time;
using EmployeeDeskBooking.Application.Users;
using EmployeeDeskBooking.Infrastructure.Bookings;
using EmployeeDeskBooking.Infrastructure.Data;
using EmployeeDeskBooking.Infrastructure.Desks;
using EmployeeDeskBooking.Infrastructure.Security;
using EmployeeDeskBooking.Infrastructure.Time;
using EmployeeDeskBooking.Infrastructure.Users;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace EmployeeDeskBooking.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddDbContext<AppDbContext>(options =>
            options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));

        services.AddScoped<IUserRepository, EfUserRepository>();
        services.AddScoped<IBookingRepository, EfBookingRepository>();
        services.AddScoped<IDeskRepository, EfDeskRepository>();
        services.AddSingleton<IPasswordVerifier, AspNetPasswordVerifier>();
        services.AddSingleton<IOfficeClock, OfficeClock>();

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
