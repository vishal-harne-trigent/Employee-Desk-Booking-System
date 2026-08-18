using EmployeeDeskBooking.Application.Auth;
using EmployeeDeskBooking.Application.Bookings;
using Microsoft.Extensions.DependencyInjection;

namespace EmployeeDeskBooking.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IBookingService, BookingService>();
        return services;
    }
}
