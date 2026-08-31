using EmployeeDeskBooking.Application.Desks;
using EmployeeDeskBooking.Application.Security;
using EmployeeDeskBooking.Domain.Desks;
using EmployeeDeskBooking.Domain.Users;
using EmployeeDeskBooking.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using EmployeeDeskBooking.Infrastructure.Seed;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace EmployeeDeskBooking.Infrastructure;

public static class DbInitializer
{
    public const string DefaultAdminEmail = "admin@trigent.com";
    public const string DefaultEmployeeEmail = "vishal_h@trigent.com";
    public const string DefaultDeactivatedEmail = "deactivated@trigent.com";
    public const string DefaultEmployeeName = "Vishal Harne";
    public const string DefaultAdminName = "Super Admin";
    public const string DefaultDeactivatedName = "Deactivated Employee";
    public const string DefaultPassword = "Password1!";

    public const int DefaultDeskCount = 5;

    public static IReadOnlyList<string> DefaultDeskNumbers { get; } =
        Enumerable.Range(1, DefaultDeskCount).Select(i => $"A-{i:D2}").ToArray();

    private static readonly string[] LegacySampleUserEmails =
    [
        "admin@company.com",
        "employee@company.com",
    ];

    public static Task SeedAsync(IServiceProvider services) =>
        SeedAsync(services, resetDefaultPasswordsInDevelopment: false);

    public static async Task SeedAsync(IServiceProvider services, bool resetDefaultPasswordsInDevelopment)
    {
        var configuration = services.GetRequiredService<IConfiguration>();
        var mode = SeedOptions.ParseMode(configuration[$"{SeedOptions.SectionName}:Mode"]);

        if (mode == SeedMode.None)
        {
            return;
        }

        if (mode == SeedMode.Json)
        {
            var replace = configuration.GetValue($"{SeedOptions.SectionName}:JsonReplaceExisting", false);
            var onlyIfEmpty = configuration.GetValue($"{SeedOptions.SectionName}:JsonOnlyIfEmpty", false);

            if (onlyIfEmpty && !replace)
            {
                var dbContext = services.GetRequiredService<AppDbContext>();
                if (await dbContext.Users.AnyAsync())
                {
                    return;
                }
            }

            await JsonDatasetSeeder.SeedAsync(
                services,
                resetDefaultPasswordsInDevelopment,
                replaceExistingData: replace);
            return;
        }

        await SeedMinimalAsync(services, resetDefaultPasswordsInDevelopment);
    }

    public static async Task SeedMinimalAsync(IServiceProvider services, bool resetDefaultPasswordsInDevelopment)
    {
        var dbContext = services.GetRequiredService<AppDbContext>();
        var passwordVerifier = services.GetRequiredService<IPasswordVerifier>();
        var now = DateTimeOffset.UtcNow;

        await RemoveLegacySampleUsersAsync(dbContext);

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

        await EnsureDeactivatedUserAsync(dbContext, passwordVerifier, now);

        await SeedDesksAsync(dbContext);
    }

    internal static async Task RemoveLegacySampleUsersAsync(AppDbContext dbContext)
    {
        var normalizedEmails = LegacySampleUserEmails.Select(NormalizeEmail).ToList();
        var users = await dbContext.Users
            .Where(user => normalizedEmails.Contains(user.EmailNormalized))
            .ToListAsync();

        if (users.Count == 0)
        {
            return;
        }

        var userIds = users.Select(user => user.Id).ToList();
        var bookingIds = await dbContext.Bookings
            .Where(booking => userIds.Contains(booking.UserId))
            .Select(booking => booking.Id)
            .ToListAsync();

        if (bookingIds.Count > 0)
        {
            dbContext.BookingReminders.RemoveRange(
                await dbContext.BookingReminders
                    .Where(reminder => bookingIds.Contains(reminder.BookingId))
                    .ToListAsync());
            dbContext.EmailDeliveryLogs.RemoveRange(
                await dbContext.EmailDeliveryLogs
                    .Where(log => log.BookingId.HasValue && bookingIds.Contains(log.BookingId.Value))
                    .ToListAsync());
            dbContext.Bookings.RemoveRange(
                await dbContext.Bookings
                    .Where(booking => bookingIds.Contains(booking.Id))
                    .ToListAsync());
        }

        dbContext.EmailDeliveryLogs.RemoveRange(
            await dbContext.EmailDeliveryLogs
                .Where(log => log.UserId.HasValue && userIds.Contains(log.UserId.Value))
                .ToListAsync());
        dbContext.NotificationPreferences.RemoveRange(
            await dbContext.NotificationPreferences
                .Where(preference => userIds.Contains(preference.UserId))
                .ToListAsync());
        dbContext.Users.RemoveRange(users);
        await dbContext.SaveChangesAsync();
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

    private static async Task EnsureDeactivatedUserAsync(
        AppDbContext dbContext,
        IPasswordVerifier passwordVerifier,
        DateTimeOffset now)
    {
        var normalized = NormalizeEmail(DefaultDeactivatedEmail);
        var user = await dbContext.Users.FirstOrDefaultAsync(u => u.EmailNormalized == normalized);

        if (user is null)
        {
            dbContext.Users.Add(
                CreateUser(
                    DefaultDeactivatedEmail,
                    DefaultDeactivatedName,
                    UserRole.Employee,
                    isActive: false,
                    passwordVerifier,
                    now));
            await dbContext.SaveChangesAsync();
        }
    }

    private static string NormalizeEmail(string email) =>
        email.Trim().ToLowerInvariant();

    private static async Task SeedDesksAsync(AppDbContext dbContext)
    {
        var now = DateTimeOffset.UtcNow;
        var allowedNumbers = DefaultDeskNumbers
            .Select(number => number.Trim().ToUpperInvariant())
            .ToHashSet(StringComparer.Ordinal);

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

        if (missingDesks.Count > 0)
        {
            dbContext.Desks.AddRange(missingDesks);
            await dbContext.SaveChangesAsync();
        }

        var extraDesks = await dbContext.Desks
            .Where(d => !allowedNumbers.Contains(d.DeskNumberNormalized))
            .ToListAsync();

        if (extraDesks.Count > 0)
        {
            var extraDeskIds = extraDesks.Select(d => d.Id).ToList();
            var deskIdsWithBookings = await dbContext.Bookings
                .Where(b => extraDeskIds.Contains(b.DeskId))
                .Select(b => b.DeskId)
                .Distinct()
                .ToListAsync();
            var deskIdsWithBookingsSet = deskIdsWithBookings.ToHashSet();

            var removableDesks = extraDesks
                .Where(d => !deskIdsWithBookingsSet.Contains(d.Id))
                .ToList();
            if (removableDesks.Count > 0)
            {
                dbContext.Desks.RemoveRange(removableDesks);
                await dbContext.SaveChangesAsync();
            }

            foreach (var desk in extraDesks.Where(d => deskIdsWithBookingsSet.Contains(d.Id)))
            {
                if (desk.Status == DeskStatus.Active)
                {
                    desk.Status = DeskStatus.Inactive;
                    desk.UpdatedAt = now;
                }
            }

            await dbContext.SaveChangesAsync();
        }

        var desksMissingLocation = await dbContext.Desks
            .Where(d => d.Location == string.Empty)
            .ToListAsync();

        foreach (var desk in desksMissingLocation)
        {
            desk.Location = DeskLocationFormatter.FormatLocation(desk.DeskNumber);
            desk.UpdatedAt = now;
        }

        if (desksMissingLocation.Count > 0)
        {
            await dbContext.SaveChangesAsync();
        }
    }

    public static Desk CreateDesk(string deskNumber, DeskStatus status, DateTimeOffset now) =>
        new()
        {
            Id = Guid.NewGuid(),
            DeskNumber = deskNumber,
            DeskNumberNormalized = deskNumber.Trim().ToUpperInvariant(),
            Location = DeskLocationFormatter.FormatLocation(deskNumber),
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
