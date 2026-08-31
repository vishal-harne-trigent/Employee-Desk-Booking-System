using EmployeeDeskBooking.Application.Desks;
using EmployeeDeskBooking.Application.Security;
using EmployeeDeskBooking.Application.Time;
using EmployeeDeskBooking.Domain.Bookings;
using EmployeeDeskBooking.Domain.Desks;
using EmployeeDeskBooking.Domain.Notifications;
using EmployeeDeskBooking.Domain.Users;
using EmployeeDeskBooking.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace EmployeeDeskBooking.Infrastructure;

public static class DbInitializer
{
    public const string DefaultAdminEmail = "admin@trigent.com";
    public const string DefaultEmployeeEmail = "vishal_h@trigent.com";
    public const string DefaultEmployeeName = "Vishal Harne";
    public const string DefaultAdminName = "Super Admin";
    public const string DefaultPassword = "Password1!";

    public const string SecondEmployeeEmail = "employee2@trigent.com";
    public const string SecondEmployeeName = "Priya Sharma";

    public const string DeactivatedEmployeeEmail = "deactivated@trigent.com";
    public const string DeactivatedEmployeeName = "Deactivated User";

    public const string DefaultInactiveDeskNumber = "B-99";

    public const int DefaultDeskCount = 5;

    public static IReadOnlyList<string> DefaultDeskNumbers { get; } =
        Enumerable.Range(1, DefaultDeskCount).Select(i => $"A-{i:D2}").ToArray();

    public static IReadOnlyList<string> AllowedDeskNumbers { get; } =
        DefaultDeskNumbers.Append(DefaultInactiveDeskNumber).ToArray();

    private static readonly string[] LegacySampleUserEmails =
    [
        "admin@company.com",
        "employee@company.com",
    ];

    public static Task SeedAsync(IServiceProvider services) =>
        SeedAsync(services, resetDefaultPasswordsInDevelopment: false);

    public static async Task SeedAsync(IServiceProvider services, bool resetDefaultPasswordsInDevelopment)
    {
        var dbContext = services.GetRequiredService<AppDbContext>();
        var passwordVerifier = services.GetRequiredService<IPasswordVerifier>();
        var configuration = services.GetRequiredService<IConfiguration>();
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

        await SeedDesksAsync(dbContext);

        if (resetDefaultPasswordsInDevelopment && IsRichDatasetEnabled(configuration))
        {
            await SeedDevelopmentUsersAsync(
                dbContext,
                passwordVerifier,
                resetDefaultPasswordsInDevelopment,
                now);

            var officeClock = services.GetRequiredService<IOfficeClock>();
            await SeedDevelopmentBookingsAsync(dbContext, officeClock, now);
            await SeedDevelopmentNotificationPreferencesAsync(dbContext, now);
        }
    }

    private static bool IsRichDatasetEnabled(IConfiguration configuration) =>
        configuration.GetValue("Seed:RichDataset", true);

    private static async Task SeedDevelopmentUsersAsync(
        AppDbContext dbContext,
        IPasswordVerifier passwordVerifier,
        bool resetPasswordInDevelopment,
        DateTimeOffset now)
    {
        await EnsureDefaultUserAsync(
            dbContext,
            passwordVerifier,
            SecondEmployeeEmail,
            SecondEmployeeName,
            UserRole.Employee,
            resetPasswordInDevelopment,
            now);

        await EnsureDefaultUserAsync(
            dbContext,
            passwordVerifier,
            DeactivatedEmployeeEmail,
            DeactivatedEmployeeName,
            UserRole.Employee,
            resetPasswordInDevelopment: false,
            now,
            isActive: false);
    }

    private static async Task SeedDevelopmentBookingsAsync(
        AppDbContext dbContext,
        IOfficeClock officeClock,
        DateTimeOffset now)
    {
        if (await dbContext.Bookings.AnyAsync())
        {
            return;
        }

        var employee = await RequireUserAsync(dbContext, DefaultEmployeeEmail);
        var secondEmployee = await RequireUserAsync(dbContext, SecondEmployeeEmail);

        var today = officeClock.Today;
        var nextWorkingDay = NextWorkingDay(today, officeClock);
        var dayAfterNext = NextWorkingDay(nextWorkingDay, officeClock);
        var previousWorkingDay = PreviousWorkingDay(today, officeClock);
        var twoWorkingDaysAgo = PreviousWorkingDay(previousWorkingDay, officeClock);

        var desksByNumber = await dbContext.Desks.ToDictionaryAsync(d => d.DeskNumberNormalized);

        var bookings = new List<Booking>();

        if (officeClock.IsWorkingDay(today))
        {
            bookings.Add(CreateBooking(employee.Id, desksByNumber, "A-01", today, BookingStatus.Confirmed, now));
        }

        bookings.Add(CreateBooking(employee.Id, desksByNumber, "A-02", nextWorkingDay, BookingStatus.Confirmed, now));
        bookings.Add(CreateBooking(secondEmployee.Id, desksByNumber, "A-05", dayAfterNext, BookingStatus.Confirmed, now));
        bookings.Add(CreateBooking(employee.Id, desksByNumber, "A-03", previousWorkingDay, BookingStatus.Completed, now));
        bookings.Add(CreateBooking(employee.Id, desksByNumber, "A-04", twoWorkingDaysAgo, BookingStatus.Cancelled, now));

        dbContext.Bookings.AddRange(bookings);
        await dbContext.SaveChangesAsync();
    }

    private static async Task SeedDevelopmentNotificationPreferencesAsync(
        AppDbContext dbContext,
        DateTimeOffset now)
    {
        var employee = await RequireUserAsync(dbContext, DefaultEmployeeEmail);

        if (await dbContext.NotificationPreferences.AnyAsync(p => p.UserId == employee.Id))
        {
            return;
        }

        dbContext.NotificationPreferences.Add(new NotificationPreference
        {
            UserId = employee.Id,
            PushOptIn = true,
            PushSubscription = null,
            UpdatedAt = now,
        });

        await dbContext.SaveChangesAsync();
    }

    private static async Task<User> RequireUserAsync(AppDbContext dbContext, string email)
    {
        var normalized = NormalizeEmail(email);
        return await dbContext.Users.SingleAsync(u => u.EmailNormalized == normalized);
    }

    private static Booking CreateBooking(
        Guid userId,
        IReadOnlyDictionary<string, Desk> desksByNumber,
        string deskNumber,
        DateOnly date,
        BookingStatus status,
        DateTimeOffset now)
    {
        var desk = desksByNumber[deskNumber.Trim().ToUpperInvariant()];

        return new Booking
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            DeskId = desk.Id,
            BookingDate = date,
            Status = status,
            CancelledAt = status == BookingStatus.Cancelled ? now : null,
            CompletedAt = status == BookingStatus.Completed ? now : null,
            CreatedAt = now,
            UpdatedAt = now,
        };
    }

    private static DateOnly NextWorkingDay(DateOnly from, IOfficeClock officeClock)
    {
        var date = from.AddDays(1);
        while (!officeClock.IsWorkingDay(date))
        {
            date = date.AddDays(1);
        }

        return date;
    }

    private static DateOnly PreviousWorkingDay(DateOnly from, IOfficeClock officeClock)
    {
        var date = from.AddDays(-1);
        while (!officeClock.IsWorkingDay(date))
        {
            date = date.AddDays(-1);
        }

        return date;
    }

    private static async Task RemoveLegacySampleUsersAsync(AppDbContext dbContext)
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
        DateTimeOffset now,
        bool isActive = true)
    {
        var normalized = NormalizeEmail(email);
        var user = await dbContext.Users.FirstOrDefaultAsync(u => u.EmailNormalized == normalized);

        if (user is null)
        {
            dbContext.Users.Add(CreateUser(email, name, role, isActive, passwordVerifier, now));
            await dbContext.SaveChangesAsync();
            return;
        }

        var changed = false;

        if (user.Name != name)
        {
            user.Name = name;
            changed = true;
        }

        if (user.IsActive != isActive)
        {
            user.IsActive = isActive;
            changed = true;
        }

        if (resetPasswordInDevelopment && isActive)
        {
            user.PasswordHash = passwordVerifier.HashPassword(user, DefaultPassword);
            changed = true;
        }

        if (changed)
        {
            user.UpdatedAt = now;
            await dbContext.SaveChangesAsync();
        }
    }

    private static string NormalizeEmail(string email) =>
        email.Trim().ToLowerInvariant();

    private static async Task SeedDesksAsync(AppDbContext dbContext)
    {
        var now = DateTimeOffset.UtcNow;
        var allowedNumbers = AllowedDeskNumbers
            .Select(number => number.Trim().ToUpperInvariant())
            .ToHashSet(StringComparer.Ordinal);

        if (!await dbContext.Desks.AnyAsync())
        {
            dbContext.Desks.AddRange(
                DefaultDeskNumbers.Select(deskNumber => CreateDesk(deskNumber, DeskStatus.Active, now)));
            dbContext.Desks.Add(CreateDesk(DefaultInactiveDeskNumber, DeskStatus.Inactive, now));
            await dbContext.SaveChangesAsync();
            return;
        }

        var existingNumbers = await dbContext.Desks
            .Select(d => d.DeskNumberNormalized)
            .ToListAsync();

        var missingDesks = AllowedDeskNumbers
            .Where(number => !existingNumbers.Contains(number.Trim().ToUpperInvariant()))
            .Select(number => CreateDesk(
                number,
                string.Equals(number, DefaultInactiveDeskNumber, StringComparison.OrdinalIgnoreCase)
                    ? DeskStatus.Inactive
                    : DeskStatus.Active,
                now))
            .ToList();

        if (missingDesks.Count > 0)
        {
            var currentNumbers = await dbContext.Desks
                .Select(d => d.DeskNumberNormalized)
                .ToListAsync();
            var desksToAdd = missingDesks
                .Where(desk => !currentNumbers.Contains(desk.DeskNumberNormalized))
                .ToList();

            if (desksToAdd.Count > 0)
            {
                dbContext.Desks.AddRange(desksToAdd);
                await dbContext.SaveChangesAsync();
            }
        }

        var inactiveDesk = await dbContext.Desks
            .FirstOrDefaultAsync(d => d.DeskNumberNormalized == DefaultInactiveDeskNumber.ToUpperInvariant());

        if (inactiveDesk is not null && inactiveDesk.Status != DeskStatus.Inactive)
        {
            inactiveDesk.Status = DeskStatus.Inactive;
            inactiveDesk.UpdatedAt = now;
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
