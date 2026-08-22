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

    public const int DefaultDeskCount = 10;

    public static IReadOnlyList<string> DefaultDeskNumbers { get; } =
        Enumerable.Range(1, DefaultDeskCount).Select(i => $"A-{i:D2}").ToArray();

    public static Task SeedAsync(IServiceProvider services) =>
        SeedAsync(services, resetDefaultPasswordsInDevelopment: false);

    public static async Task SeedAsync(IServiceProvider services, bool resetDefaultPasswordsInDevelopment)
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
        var now = DateTimeOffset.UtcNow;

        if (!await dbContext.Desks.AnyAsync())
        {
            dbContext.Desks.AddRange(
                DefaultDeskNumbers.Select(deskNumber => CreateDesk(deskNumber, DeskStatus.Active, now)));
            await dbContext.SaveChangesAsync();
            return;
        }

        var existingNumbers = await dbContext.Desks
            .Select(d => d.DeskNumberNormalized)
            .ToListAsync();

        var missingDesks = DefaultDeskNumbers
            .Where(number => !existingNumbers.Contains(number.Trim().ToUpperInvariant()))
            .Select(number => CreateDesk(number, DeskStatus.Active, now))
            .ToList();

        if (missingDesks.Count == 0)
        {
            return;
        }

        dbContext.Desks.AddRange(missingDesks);
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
