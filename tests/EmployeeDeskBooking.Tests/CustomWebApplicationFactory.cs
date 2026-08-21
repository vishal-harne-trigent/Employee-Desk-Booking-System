using AngleSharp;
using AngleSharp.Html.Dom;
using EmployeeDeskBooking.Application.Notifications;
using EmployeeDeskBooking.Application.Security;
using EmployeeDeskBooking.Domain.Users;
using EmployeeDeskBooking.Infrastructure;
using EmployeeDeskBooking.Infrastructure.Data;
using EmployeeDeskBooking.Web;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;

namespace EmployeeDeskBooking.Tests;

public sealed class CustomWebApplicationFactory : WebApplicationFactory<WebAssemblyMarker>
{
    public const string TestPassword = DbInitializer.DefaultPassword;

    private readonly string _databaseName = Guid.NewGuid().ToString();
    private static readonly SemaphoreSlim SeedLock = new(1, 1);

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        builder.ConfigureServices(services =>
        {
            services.RemoveAll(typeof(DbContextOptions<AppDbContext>));
            services.RemoveAll(typeof(AppDbContext));

            services.AddDbContext<AppDbContext>(options =>
                options.UseInMemoryDatabase(_databaseName));

            services.ConfigureBookingTests();
        });
    }

    protected override IHost CreateHost(IHostBuilder builder)
    {
        var host = base.CreateHost(builder);

        using var scope = host.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        db.Database.EnsureCreated();
        SeedLock.Wait();
        try
        {
            SeedTestUsers(scope.ServiceProvider).GetAwaiter().GetResult();
            BookDeskTestFactoryExtensions.SeedBookingTestDataAsync(db).GetAwaiter().GetResult();
        }
        finally
        {
            SeedLock.Release();
        }

        return host;
    }

    public async Task ResetBookingsAsync()
    {
        using var scope = Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        db.Bookings.RemoveRange(await db.Bookings.ToListAsync());
        db.EmailDeliveryLogs.RemoveRange(await db.EmailDeliveryLogs.ToListAsync());
        db.BookingReminders.RemoveRange(await db.BookingReminders.ToListAsync());
        await db.SaveChangesAsync();
        scope.ServiceProvider.GetRequiredService<InMemoryEmailSender>().Reset();
    }

    public InMemoryEmailSender GetEmailSender() =>
        Services.GetRequiredService<InMemoryEmailSender>();

    private static async Task SeedTestUsers(IServiceProvider services)
    {
        var db = services.GetRequiredService<AppDbContext>();
        if (await db.Users.AnyAsync())
        {
            return;
        }

        var passwordVerifier = services.GetRequiredService<IPasswordVerifier>();
        var now = DateTimeOffset.UtcNow;

        var seeded = new[]
        {
            DbInitializer.CreateUser(
                "employee@test.com",
                "Test Employee",
                UserRole.Employee,
                isActive: true,
                passwordVerifier,
                now),
            DbInitializer.CreateUser(
                "admin@test.com",
                "Test Admin",
                UserRole.Admin,
                isActive: true,
                passwordVerifier,
                now),
            DbInitializer.CreateUser(
                "deactivated@test.com",
                "Deactivated User",
                UserRole.Employee,
                isActive: false,
                passwordVerifier,
                now),
        };
        foreach (var user in seeded)
        {
            user.EmailNormalized = user.Email.Trim().ToLowerInvariant();
        }

        db.Users.AddRange(seeded);

        await db.SaveChangesAsync();
    }

    public HttpClient CreateLoginClient() =>
        CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false,
            HandleCookies = true,
        });

    public LoginTestClient CreateLoginTestClient() =>
        new(CreateLoginClient());
}

public sealed class LoginTestClient(HttpClient client)
{
    private static readonly IBrowsingContext HtmlParser =
        BrowsingContext.New(Configuration.Default.WithDefaultLoader());

    public HttpClient Client { get; } = client;

    public async Task<HttpResponseMessage> GetLoginPageAsync(CancellationToken cancellationToken = default) =>
        await Client.GetAsync("/Account/Login", cancellationToken);

    public async Task<HttpResponseMessage> LoginAsync(
        string email,
        string password,
        CancellationToken cancellationToken = default)
    {
        var loginPage = await GetLoginPageAsync(cancellationToken);
        loginPage.EnsureSuccessStatusCode();

        var html = await loginPage.Content.ReadAsStringAsync(cancellationToken);
        var token = await GetAntiforgeryTokenAsync(html);

        var form = new Dictionary<string, string>
        {
            ["Email"] = email,
            ["Password"] = password,
            ["__RequestVerificationToken"] = token,
        };

        using var request = new HttpRequestMessage(HttpMethod.Post, "/Account/Login")
        {
            Content = new FormUrlEncodedContent(form),
        };

        return await Client.SendAsync(request, cancellationToken);
    }

    public async Task<HttpResponseMessage> LogoutFromAsync(
        string authenticatedPagePath,
        CancellationToken cancellationToken = default)
    {
        var page = await Client.GetAsync(authenticatedPagePath, cancellationToken);
        page.EnsureSuccessStatusCode();

        var html = await page.Content.ReadAsStringAsync(cancellationToken);
        var token = await GetAntiforgeryTokenAsync(html);

        using var request = new HttpRequestMessage(HttpMethod.Post, "/Account/Logout")
        {
            Content = new FormUrlEncodedContent(new Dictionary<string, string>
            {
                ["__RequestVerificationToken"] = token,
            }),
        };

        return await Client.SendAsync(request, cancellationToken);
    }

    private static async Task<string> GetAntiforgeryTokenAsync(string html)
    {
        var document = await HtmlParser.OpenAsync(req => req.Content(html));
        var input = document.QuerySelector("input[name='__RequestVerificationToken']") as IHtmlInputElement
            ?? throw new InvalidOperationException("Anti-forgery token input was not found.");

        return input.Value ?? throw new InvalidOperationException("Anti-forgery token value was empty.");
    }
}
