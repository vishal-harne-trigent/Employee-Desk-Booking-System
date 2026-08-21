using AngleSharp;
using AngleSharp.Html.Dom;
using EmployeeDeskBooking.Application.Notifications;
using EmployeeDeskBooking.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace EmployeeDeskBooking.Tests;

public class NotificationSettingsTests(CustomWebApplicationFactory factory) : IClassFixture<CustomWebApplicationFactory>
{
    [Fact(DisplayName = "Opt in via settings saves preference (US-008/AC-02)")]
    public async Task Opt_in_via_settings_US_008_AC_02()
    {
        await factory.ResetBookingsAsync();
        var client = new NotificationSettingsTestClient(factory);
        await client.LoginAsEmployeeAsync();

        var response = await client.SaveSubscriptionAsync(PushTestData.SampleSubscriptionJson);
        response.EnsureSuccessStatusCode();

        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var employee = db.Users.Single(u => u.Email == "employee@test.com");
        var preference = db.NotificationPreferences.Single(p => p.UserId == employee.Id);
        Assert.True(preference.PushOptIn);
        Assert.NotNull(preference.PushSubscription);
    }

    [Fact(DisplayName = "Settings page shows unsupported browser guidance (US-008/NFR-006)")]
    public async Task Settings_page_shows_unsupported_guidance_US_008_NFR_006()
    {
        await factory.ResetBookingsAsync();
        var client = new NotificationSettingsTestClient(factory);
        await client.LoginAsEmployeeAsync();

        var response = await client.GetSettingsPageAsync();
        var body = await response.Content.ReadAsStringAsync();

        response.EnsureSuccessStatusCode();
        Assert.Contains("Notification settings", body, StringComparison.Ordinal);
        Assert.Contains("does not support push", body, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("email", body, StringComparison.OrdinalIgnoreCase);
    }

    [Fact(DisplayName = "Disable push from settings (US-008/AC-04)")]
    public async Task Disable_push_from_settings_US_008_AC_04()
    {
        await factory.ResetBookingsAsync();
        Guid employeeId;
        using (var scope = factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var employee = db.Users.Single(u => u.Email == "employee@test.com");
            employeeId = employee.Id;
            await BookDeskTestFactoryExtensions.SeedPushOptInAsync(db, employee.Id);
        }

        var client = new NotificationSettingsTestClient(factory);
        await client.LoginAsEmployeeAsync();
        var response = await client.DisablePushAsync();
        response.EnsureSuccessStatusCode();

        using var verifyScope = factory.Services.CreateScope();
        var preferenceService = verifyScope.ServiceProvider.GetRequiredService<INotificationPreferenceService>();
        var state = await preferenceService.GetPreferencesAsync(employeeId);
        Assert.False(state.PushOptIn);
    }
}

public sealed class NotificationSettingsTestClient(CustomWebApplicationFactory factory)
{
    private static readonly IBrowsingContext HtmlParser =
        BrowsingContext.New(Configuration.Default.WithDefaultLoader());

    public LoginTestClient Login { get; } = factory.CreateLoginTestClient();

    public async Task LoginAsEmployeeAsync()
    {
        var response = await Login.LoginAsync("employee@test.com", CustomWebApplicationFactory.TestPassword);
        Assert.Equal(302, (int)response.StatusCode);
    }

    public Task<HttpResponseMessage> GetSettingsPageAsync() =>
        Login.Client.GetAsync("/Settings/Notifications");

    public async Task<HttpResponseMessage> SaveSubscriptionAsync(string subscriptionJson)
    {
        var page = await GetSettingsPageAsync();
        page.EnsureSuccessStatusCode();
        var token = await GetAntiforgeryTokenAsync(await page.Content.ReadAsStringAsync());

        var form = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["subscriptionJson"] = subscriptionJson,
            ["__RequestVerificationToken"] = token,
        });

        return await Login.Client.PostAsync("/Settings/Notifications/SaveSubscription", form);
    }

    public async Task<HttpResponseMessage> DisablePushAsync()
    {
        var page = await GetSettingsPageAsync();
        page.EnsureSuccessStatusCode();
        var token = await GetAntiforgeryTokenAsync(await page.Content.ReadAsStringAsync());

        var form = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["__RequestVerificationToken"] = token,
        });

        return await Login.Client.PostAsync("/Settings/Notifications/Disable", form);
    }

    private static async Task<string> GetAntiforgeryTokenAsync(string html)
    {
        var document = await HtmlParser.OpenAsync(req => req.Content(html));
        var input = document.QuerySelector("input[name='__RequestVerificationToken']") as IHtmlInputElement
            ?? throw new InvalidOperationException("Anti-forgery token input was not found.");
        return input.Value ?? throw new InvalidOperationException("Anti-forgery token value was empty.");
    }
}
