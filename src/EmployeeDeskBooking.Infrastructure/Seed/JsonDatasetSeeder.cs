using System.Text.Json;
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

namespace EmployeeDeskBooking.Infrastructure.Seed;

public static class JsonDatasetSeeder
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        ReadCommentHandling = JsonCommentHandling.Skip,
        AllowTrailingCommas = true,
    };

    public static async Task SeedAsync(
        IServiceProvider services,
        bool resetPasswordsInDevelopment,
        bool replaceExistingData = false,
        string? datasetPath = null)
    {
        var configuration = services.GetRequiredService<IConfiguration>();
        var path = datasetPath ?? ResolveDatasetPath(configuration);

        if (!File.Exists(path))
        {
            throw new FileNotFoundException($"Dataset file not found: {path}");
        }

        await using var stream = File.OpenRead(path);
        var document = await JsonSerializer.DeserializeAsync<JsonDatasetDocument>(stream, JsonOptions)
            ?? throw new InvalidOperationException($"Dataset file is empty or invalid: {path}");

        var dbContext = services.GetRequiredService<AppDbContext>();
        var passwordVerifier = services.GetRequiredService<IPasswordVerifier>();
        var officeClock = services.GetRequiredService<IOfficeClock>();
        var now = DateTimeOffset.UtcNow;

        if (replaceExistingData)
        {
            await ClearSeedDataAsync(dbContext);
        }

        await DbInitializer.RemoveLegacySampleUsersAsync(dbContext);

        foreach (var entry in document.Users)
        {
            await UpsertUserAsync(
                dbContext,
                passwordVerifier,
                entry,
                document.DefaultPassword,
                resetPasswordsInDevelopment,
                now);
        }

        foreach (var entry in document.Desks)
        {
            await UpsertDeskAsync(dbContext, entry, now);
        }

        if (replaceExistingData || !await dbContext.Bookings.AnyAsync())
        {
            await SeedBookingsAsync(dbContext, document, officeClock, now);
        }

        foreach (var entry in document.NotificationPreferences)
        {
            await UpsertNotificationPreferenceAsync(dbContext, entry, now);
        }
    }

    public static string ResolveDatasetPath(IConfiguration configuration)
    {
        var configured = configuration[$"{SeedOptions.SectionName}:DatasetPath"];
        if (!string.IsNullOrWhiteSpace(configured))
        {
            return Path.IsPathRooted(configured)
                ? configured
                : Path.GetFullPath(Path.Combine(ResolveRepoRoot(), configured));
        }

        return Path.GetFullPath(Path.Combine(ResolveRepoRoot(), SeedOptions.DefaultDatasetRelativePath));
    }

    public static string ResolveRepoRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null)
        {
            if (File.Exists(Path.Combine(directory.FullName, "EmployeeDeskBooking.sln")))
            {
                return directory.FullName;
            }

            directory = directory.Parent;
        }

        return Directory.GetCurrentDirectory();
    }

    private static async Task ClearSeedDataAsync(AppDbContext dbContext)
    {
        dbContext.BookingReminders.RemoveRange(await dbContext.BookingReminders.ToListAsync());
        dbContext.EmailDeliveryLogs.RemoveRange(await dbContext.EmailDeliveryLogs.ToListAsync());
        dbContext.Bookings.RemoveRange(await dbContext.Bookings.ToListAsync());
        dbContext.NotificationPreferences.RemoveRange(await dbContext.NotificationPreferences.ToListAsync());
        dbContext.Users.RemoveRange(await dbContext.Users.ToListAsync());
        dbContext.Desks.RemoveRange(await dbContext.Desks.ToListAsync());
        await dbContext.SaveChangesAsync();
    }

    private static async Task UpsertUserAsync(
        AppDbContext dbContext,
        IPasswordVerifier passwordVerifier,
        JsonDatasetUser entry,
        string defaultPassword,
        bool resetPasswordInDevelopment,
        DateTimeOffset now)
    {
        var normalized = NormalizeEmail(entry.Email);
        var role = ParseRole(entry.Role);
        var user = await dbContext.Users.FirstOrDefaultAsync(u => u.EmailNormalized == normalized);

        if (user is null)
        {
            user = DbInitializer.CreateUser(
                entry.Email,
                entry.Name,
                role,
                entry.IsActive,
                passwordVerifier,
                now);
            user.PasswordHash = passwordVerifier.HashPassword(user, entry.Password ?? defaultPassword);
            dbContext.Users.Add(user);
            await dbContext.SaveChangesAsync();
            return;
        }

        var changed = false;

        if (user.Name != entry.Name)
        {
            user.Name = entry.Name;
            changed = true;
        }

        if (user.Role != role)
        {
            user.Role = role;
            changed = true;
        }

        if (user.IsActive != entry.IsActive)
        {
            user.IsActive = entry.IsActive;
            changed = true;
        }

        if (resetPasswordInDevelopment && entry.IsActive)
        {
            user.PasswordHash = passwordVerifier.HashPassword(user, entry.Password ?? defaultPassword);
            changed = true;
        }

        if (changed)
        {
            user.UpdatedAt = now;
            await dbContext.SaveChangesAsync();
        }
    }

    private static async Task UpsertDeskAsync(
        AppDbContext dbContext,
        JsonDatasetDesk entry,
        DateTimeOffset now)
    {
        var normalized = entry.DeskNumber.Trim().ToUpperInvariant();
        var status = ParseDeskStatus(entry.Status);
        var desk = await dbContext.Desks.FirstOrDefaultAsync(d => d.DeskNumberNormalized == normalized);

        if (desk is null)
        {
            desk = DbInitializer.CreateDesk(entry.DeskNumber, status, now);
            if (!string.IsNullOrWhiteSpace(entry.Location))
            {
                desk.Location = entry.Location.Trim();
            }

            dbContext.Desks.Add(desk);
            await dbContext.SaveChangesAsync();
            return;
        }

        var changed = false;

        if (desk.Status != status)
        {
            desk.Status = status;
            changed = true;
        }

        var location = string.IsNullOrWhiteSpace(entry.Location)
            ? DeskLocationFormatter.FormatLocation(desk.DeskNumber)
            : entry.Location.Trim();

        if (desk.Location != location)
        {
            desk.Location = location;
            changed = true;
        }

        if (changed)
        {
            desk.UpdatedAt = now;
            await dbContext.SaveChangesAsync();
        }
    }

    private static async Task SeedBookingsAsync(
        AppDbContext dbContext,
        JsonDatasetDocument document,
        IOfficeClock officeClock,
        DateTimeOffset now)
    {
        var usersByEmail = await dbContext.Users.ToDictionaryAsync(u => u.EmailNormalized);
        var desksByNumber = await dbContext.Desks.ToDictionaryAsync(d => d.DeskNumberNormalized);

        foreach (var entry in document.Bookings)
        {
            var user = usersByEmail[NormalizeEmail(entry.UserEmail)];
            var desk = desksByNumber[entry.DeskNumber.Trim().ToUpperInvariant()];
            var bookingDate = ResolveWorkingDay(officeClock.Today, entry.WorkingDayOffset, officeClock);

            if (entry.WorkingDayOffset == 0 && !officeClock.IsWorkingDay(officeClock.Today))
            {
                continue;
            }

            var status = ParseBookingStatus(entry.Status);

            if (status == BookingStatus.Confirmed
                && await HasConfirmedConflictAsync(dbContext, user.Id, desk.Id, bookingDate))
            {
                continue;
            }

            dbContext.Bookings.Add(new Booking
            {
                Id = Guid.NewGuid(),
                UserId = user.Id,
                DeskId = desk.Id,
                BookingDate = bookingDate,
                Status = status,
                CancelledAt = status == BookingStatus.Cancelled ? now : null,
                CompletedAt = status == BookingStatus.Completed ? now : null,
                CreatedAt = now,
                UpdatedAt = now,
            });
        }

        await dbContext.SaveChangesAsync();
    }

    private static async Task<bool> HasConfirmedConflictAsync(
        AppDbContext dbContext,
        Guid userId,
        Guid deskId,
        DateOnly bookingDate) =>
        await dbContext.Bookings.AnyAsync(booking =>
            booking.BookingDate == bookingDate
            && booking.Status == BookingStatus.Confirmed
            && (booking.UserId == userId || booking.DeskId == deskId));

    private static async Task UpsertNotificationPreferenceAsync(
        AppDbContext dbContext,
        JsonDatasetNotificationPreference entry,
        DateTimeOffset now)
    {
        var user = await dbContext.Users.SingleAsync(u => u.EmailNormalized == NormalizeEmail(entry.UserEmail));
        var preference = await dbContext.NotificationPreferences.FirstOrDefaultAsync(p => p.UserId == user.Id);

        if (preference is null)
        {
            dbContext.NotificationPreferences.Add(new NotificationPreference
            {
                UserId = user.Id,
                PushOptIn = entry.PushOptIn,
                PushSubscription = null,
                UpdatedAt = now,
            });
        }
        else
        {
            preference.PushOptIn = entry.PushOptIn;
            preference.UpdatedAt = now;
        }

        await dbContext.SaveChangesAsync();
    }

    private static DateOnly ResolveWorkingDay(DateOnly anchor, int workingDayOffset, IOfficeClock officeClock)
    {
        if (workingDayOffset == 0)
        {
            return anchor;
        }

        return ShiftWorkingDays(anchor, workingDayOffset, officeClock);
    }

    private static DateOnly ShiftWorkingDays(DateOnly anchor, int workingDayOffset, IOfficeClock officeClock)
    {
        var step = workingDayOffset >= 0 ? 1 : -1;
        var remaining = Math.Abs(workingDayOffset);
        var date = anchor;

        while (remaining > 0)
        {
            date = date.AddDays(step);
            if (officeClock.IsWorkingDay(date))
            {
                remaining--;
            }
        }

        return date;
    }

    private static UserRole ParseRole(string role) =>
        role.Trim().ToLowerInvariant() switch
        {
            "admin" => UserRole.Admin,
            "employee" => UserRole.Employee,
            _ => throw new InvalidOperationException($"Unknown user role '{role}' in dataset.json."),
        };

    private static DeskStatus ParseDeskStatus(string status) =>
        status.Trim().ToLowerInvariant() switch
        {
            "active" => DeskStatus.Active,
            "inactive" => DeskStatus.Inactive,
            _ => throw new InvalidOperationException($"Unknown desk status '{status}' in dataset.json."),
        };

    private static BookingStatus ParseBookingStatus(string status) =>
        status.Trim().ToLowerInvariant() switch
        {
            "confirmed" => BookingStatus.Confirmed,
            "cancelled" => BookingStatus.Cancelled,
            "completed" => BookingStatus.Completed,
            _ => throw new InvalidOperationException($"Unknown booking status '{status}' in dataset.json."),
        };

    private static string NormalizeEmail(string email) =>
        email.Trim().ToLowerInvariant();
}
