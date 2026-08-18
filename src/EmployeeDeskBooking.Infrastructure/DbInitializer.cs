using EmployeeDeskBooking.Application.Security;
using EmployeeDeskBooking.Domain.Desks;
using EmployeeDeskBooking.Domain.Users;
using EmployeeDeskBooking.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace EmployeeDeskBooking.Infrastructure;

public static class DbInitializer
{
    public const string DefaultAdminEmail = "admin@trigent.com";
    public const string DefaultEmployeeEmail = "vishal_h@trigent.com";
    public const string DefaultEmployeeName = "Vishal Harne";
    public const string DefaultAdminName = "Super Admin";
    public const string DefaultPassword = "Password1!";

    public static async Task SeedAsync(IServiceProvider services, bool resetDefaultPasswordsInDevelopment = false)
    {
        var dbContext = services.GetRequiredService<AppDbContext>();
        var passwordVerifier = services.GetRequiredService<IPasswordVerifier>();
        var now = DateTimeOffset.UtcNow;

        await EnsureDefaultUserAsync(
            dbContext,
            passwordVerifier,
            DefaultEmployeeEmail,
            DefaultEmployeeName,
            UserRole.Employee,
            resetDefaultPasswordsInDevelopment,
            now);

        await EnsureDefaultUserAsync(
            dbContext,
            passwordVerifier,
            DefaultAdminEmail,
            DefaultAdminName,
            UserRole.Admin,
            resetDefaultPasswordsInDevelopment,
            now);

        await SeedDesksAsync(dbContext);
    }

    private static async Task EnsureDefaultUserAsync(
        AppDbContext dbContext,
        IPasswordVerifier passwordVerifier,
        string email,
        string name,
        UserRole role,
        bool resetPasswordInDevelopment,
        DateTimeOffset now)
    {
        var normalized = NormalizeEmail(email);
        var user = await dbContext.Users.FirstOrDefaultAsync(u => u.EmailNormalized == normalized);

        if (user is null)
        {
            dbContext.Users.Add(CreateUser(email, name, role, isActive: true, passwordVerifier, now));
            await dbContext.SaveChangesAsync();
            return;
        }

        if (resetPasswordInDevelopment)
        {
            user.Name = name;
            user.PasswordHash = passwordVerifier.HashPassword(user, DefaultPassword);
            user.UpdatedAt = now;
            await dbContext.SaveChangesAsync();
        }
    }

    private static string NormalizeEmail(string email) =>
        email.Trim().ToLowerInvariant();

    private static async Task SeedDesksAsync(AppDbContext dbContext)
    {
        if (await dbContext.Desks.AnyAsync())
        {
            return;
        }

        var now = DateTimeOffset.UtcNow;
        dbContext.Desks.AddRange(
            CreateDesk("A-01", DeskStatus.Active, now),
            CreateDesk("A-02", DeskStatus.Active, now),
            CreateDesk("B-01", DeskStatus.Inactive, now));

        await dbContext.SaveChangesAsync();
    }

    public static Desk CreateDesk(string deskNumber, DeskStatus status, DateTimeOffset now) =>
        new()
        {
            Id = Guid.NewGuid(),
            DeskNumber = deskNumber,
            DeskNumberNormalized = deskNumber.Trim().ToUpperInvariant(),
            Status = status,
            CreatedAt = now,
            UpdatedAt = now,
        };

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
            EmailNormalized = NormalizeEmail(email),
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
