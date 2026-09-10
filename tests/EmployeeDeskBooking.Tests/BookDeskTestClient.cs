using AngleSharp;
using AngleSharp.Html.Dom;
using EmployeeDeskBooking.Application.Notifications;
using EmployeeDeskBooking.Application.Time;
using EmployeeDeskBooking.Domain.Bookings;
using EmployeeDeskBooking.Domain.Desks;
using EmployeeDeskBooking.Domain.Notifications;
using EmployeeDeskBooking.Infrastructure;
using EmployeeDeskBooking.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace EmployeeDeskBooking.Tests;

public sealed class BookDeskTestClient(CustomWebApplicationFactory factory)
{
    public static readonly DateOnly FixedToday = new(2026, 8, 18);

    private static readonly IBrowsingContext HtmlParser =
        BrowsingContext.New(Configuration.Default.WithDefaultLoader());

    public LoginTestClient Login { get; } = factory.CreateLoginTestClient();

    public async Task LoginAsEmployeeAsync()
    {
        var response = await Login.LoginAsync("employee@test.com", CustomWebApplicationFactory.TestPassword);
        Assert.Equal(302, (int)response.StatusCode);
        Assert.StartsWith("/Desks/Availability", response.Headers.Location?.OriginalString, StringComparison.Ordinal);
    }

    public Task<HttpResponseMessage> GetAvailabilityPageAsync(DateOnly? date = null)
    {
        var path = date.HasValue
            ? $"/Desks/Availability?date={date.Value:yyyy-MM-dd}"
            : "/Desks/Availability";
        return Login.Client.GetAsync(path);
    }

    public Task<HttpResponseMessage> CheckAvailabilityAsync(DateOnly date) =>
        GetAvailabilityPageAsync(date);

    public Task<HttpResponseMessage> BookDeskAsync(Guid deskId, DateOnly date) =>
        Login.Client.GetAsync($"/Desks/Book?deskId={deskId}&date={date:yyyy-MM-dd}");

    public Guid GetDeskIdByNumber(string deskNumber)
    {
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var desk = db.Desks.Single(d => d.DeskNumber == deskNumber);
        return desk.Id;
    }
}

public static class BookDeskTestFactoryExtensions
{
    public static void ConfigureBookingTests(this IServiceCollection services)
    {
        services.RemoveAll<IOfficeClock>();
        services.AddSingleton<IOfficeClock>(new TestOfficeClock(BookDeskTestClient.FixedToday));

        services.RemoveAll<IEmailSender>();
        services.RemoveAll<InMemoryEmailSender>();
        var emailSender = new InMemoryEmailSender();
        services.AddSingleton(emailSender);
        services.AddSingleton<IEmailSender>(emailSender);

        services.RemoveAll<IPushNotificationSender>();
        services.RemoveAll<InMemoryPushNotificationSender>();
        var pushSender = new InMemoryPushNotificationSender();
        services.AddSingleton(pushSender);
        services.AddSingleton<IPushNotificationSender>(pushSender);
    }

    public static async Task SeedPushOptInAsync(
        AppDbContext db,
        Guid userId,
        string subscriptionJson = PushTestData.SampleSubscriptionJson)
    {
        var now = DateTimeOffset.UtcNow;
        db.NotificationPreferences.Add(new Domain.Notifications.NotificationPreference
        {
            UserId = userId,
            PushOptIn = true,
            PushSubscription = subscriptionJson,
            UpdatedAt = now,
        });
        await db.SaveChangesAsync();
    }

    public static async Task SeedBookingTestDataAsync(AppDbContext db)
    {
        if (!await db.Desks.AnyAsync())
        {
            var now = DateTimeOffset.UtcNow;
            db.Desks.AddRange(
                DbInitializer.CreateDesk("A-01", DeskStatus.Active, now),
                DbInitializer.CreateDesk("A-02", DeskStatus.Active, now),
                DbInitializer.CreateDesk("B-01", DeskStatus.Inactive, now));
            await db.SaveChangesAsync();
        }
    }

    public static async Task SeedConfirmedBookingAsync(
        AppDbContext db,
        Guid userId,
        string deskNumber,
        DateOnly date)
    {
        await SeedBookingAsync(db, userId, deskNumber, date, BookingStatus.Confirmed);
    }

    public static async Task SeedBookingAsync(
        AppDbContext db,
        Guid userId,
        string deskNumber,
        DateOnly date,
        BookingStatus status)
    {
        var desk = db.Desks.Single(d => d.DeskNumber == deskNumber);
        var now = DateTimeOffset.UtcNow;
        db.Bookings.Add(new Booking
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
        });
        await db.SaveChangesAsync();
    }
}
