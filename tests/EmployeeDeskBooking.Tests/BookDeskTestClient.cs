using AngleSharp;
using AngleSharp.Html.Dom;
using EmployeeDeskBooking.Application.Time;
using EmployeeDeskBooking.Domain.Bookings;
using EmployeeDeskBooking.Domain.Desks;
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
        Assert.StartsWith("/Book", response.Headers.Location?.OriginalString, StringComparison.Ordinal);
    }

    public async Task<HttpResponseMessage> CheckAvailabilityAsync(DateOnly date)
    {
        var bookPage = await Login.Client.GetAsync("/Book/Index");
        bookPage.EnsureSuccessStatusCode();
        var token = await GetAntiforgeryTokenAsync(await bookPage.Content.ReadAsStringAsync());

        var form = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["selectedDate"] = date.ToString("yyyy-MM-dd"),
            ["__RequestVerificationToken"] = token,
        });

        return await Login.Client.PostAsync("/Book/CheckAvailability", form);
    }

    public async Task<HttpResponseMessage> BookDeskAsync(Guid deskId, DateOnly date)
    {
        var bookPage = await Login.Client.GetAsync("/Book/Index");
        bookPage.EnsureSuccessStatusCode();
        var token = await GetAntiforgeryTokenAsync(await bookPage.Content.ReadAsStringAsync());

        var form = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["deskId"] = deskId.ToString(),
            ["selectedDate"] = date.ToString("yyyy-MM-dd"),
            ["__RequestVerificationToken"] = token,
        });

        return await Login.Client.PostAsync("/Book/BookDesk", form);
    }

    public Guid GetDeskIdByNumber(string deskNumber)
    {
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var desk = db.Desks.Single(d => d.DeskNumber == deskNumber);
        return desk.Id;
    }

    private static async Task<string> GetAntiforgeryTokenAsync(string html)
    {
        var document = await HtmlParser.OpenAsync(req => req.Content(html));
        var input = document.QuerySelector("input[name='__RequestVerificationToken']") as IHtmlInputElement
            ?? throw new InvalidOperationException("Anti-forgery token input was not found.");
        return input.Value ?? throw new InvalidOperationException("Anti-forgery token value was empty.");
    }
}

public static class BookDeskTestFactoryExtensions
{
    public static void ConfigureBookingTests(this IServiceCollection services)
    {
        services.RemoveAll<IOfficeClock>();
        services.AddSingleton<IOfficeClock>(new TestOfficeClock(BookDeskTestClient.FixedToday));
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
