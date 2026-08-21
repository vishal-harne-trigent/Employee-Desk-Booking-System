using EmployeeDeskBooking.Application.Security;
using EmployeeDeskBooking.Application.Users;
using EmployeeDeskBooking.Domain.Users;
using EmployeeDeskBooking.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace EmployeeDeskBooking.Infrastructure;

public static class DbInitializer
{
    public const string DefaultAdminEmail = "admin@trigent.com";
    public const string DefaultEmployeeEmail = "vishal_h@trigent.com";
    public const string DefaultPassword = "Password1!";

    public static async Task SeedAsync(IServiceProvider services)
    {
        var dbContext = services.GetRequiredService<AppDbContext>();
        var users = services.GetRequiredService<IUserRepository>();
        var passwordVerifier = services.GetRequiredService<IPasswordVerifier>();

        if (await dbContext.Users.AnyAsync())
        {
            return;
        }

        var now = DateTimeOffset.UtcNow;

        await users.AddAsync(CreateUser(
            DefaultEmployeeEmail,
            "Sample Employee",
            UserRole.Employee,
            isActive: true,
            passwordVerifier,
            now));

        await users.AddAsync(CreateUser(
            DefaultAdminEmail,
            "Sample Admin",
            UserRole.Admin,
            isActive: true,
            passwordVerifier,
            now));

        await users.SaveChangesAsync();
    }

    public static User CreateUser(
        string email,
        string name,
        UserRole role,
        bool isActive,
        IPasswordVerifier passwordVerifier,
        DateTimeOffset now)
    {
        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = email,
            Name = name,
            Role = role,
            IsActive = isActive,
            CreatedAt = now,
            UpdatedAt = now,
        };

        user.PasswordHash = passwordVerifier.HashPassword(user, DefaultPassword);
        return user;
    }
}
