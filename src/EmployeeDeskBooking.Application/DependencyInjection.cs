using EmployeeDeskBooking.Application.Auth;
using EmployeeDeskBooking.Application.Bookings;
using EmployeeDeskBooking.Application.Desks;
using EmployeeDeskBooking.Application.Notifications;
using EmployeeDeskBooking.Application.Users;
using Microsoft.Extensions.DependencyInjection;

namespace EmployeeDeskBooking.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IBookingService, BookingService>();
        services.AddScoped<IDeskService, DeskService>();
        services.AddScoped<IUserAdminService, UserAdminService>();
        services.AddScoped<IBookingEmailService, BookingEmailService>();
        services.AddScoped<IReminderEmailService, ReminderEmailService>();
        services.AddScoped<INotificationPreferenceService, NotificationPreferenceService>();
        services.AddScoped<IBookingPushService, BookingPushService>();
        services.AddScoped<IBookingCompletionService, BookingCompletionService>();
        return services;
    }
}
