using System.Net.Http.Json;
using EmployeeDeskBooking.Api.Contracts.Notifications;
using EmployeeDeskBooking.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace EmployeeDeskBooking.Tests;

public class ApiNotificationTests(CustomApiApplicationFactory factory) : IClassFixture<CustomApiApplicationFactory>
{
    [Fact(DisplayName = "API returns default opt-out preferences (US-008/AC-01)")]
    public async Task Api_default_opt_out_US_008_AC_01()
    {
        var client = new ApiTestClient(factory);
        await client.AuthorizeAsEmployeeAsync();

        var response = await client.GetNotificationPreferencesAsync();
        response.EnsureSuccessStatusCode();

        var body = await response.Content.ReadFromJsonAsync<NotificationPreferencesResponse>();
        Assert.NotNull(body);
        Assert.False(body.PushOptIn);
        Assert.False(body.HasSubscription);
    }

    [Fact(DisplayName = "API saves push subscription on opt in (US-008/AC-02)")]
    public async Task Api_save_subscription_US_008_AC_02()
    {
        var client = new ApiTestClient(factory);
        await client.AuthorizeAsEmployeeAsync();

        var response = await client.SavePushSubscriptionAsync(new SavePushSubscriptionRequest
        {
            Subscription = new PushSubscriptionPayload
            {
                Endpoint = "https://push.test/send/abc",
                Keys = new PushSubscriptionKeys
                {
                    P256dh = "test-key",
                    Auth = "test-auth",
                },
            },
        });
        response.EnsureSuccessStatusCode();

        var body = await response.Content.ReadFromJsonAsync<NotificationPreferencesResponse>();
        Assert.NotNull(body);
        Assert.True(body.PushOptIn);
        Assert.True(body.HasSubscription);
    }

    [Fact(DisplayName = "API opt out clears subscription (US-008/AC-04)")]
    public async Task Api_opt_out_US_008_AC_04()
    {
        var client = new ApiTestClient(factory);
        await client.AuthorizeAsEmployeeAsync();
        await client.SavePushSubscriptionAsync(new SavePushSubscriptionRequest
        {
            Subscription = new PushSubscriptionPayload
            {
                Endpoint = "https://push.test/send/abc",
                Keys = new PushSubscriptionKeys
                {
                    P256dh = "test-key",
                    Auth = "test-auth",
                },
            },
        });

        var response = await client.UpdateNotificationPreferencesAsync(new UpdateNotificationPreferencesRequest
        {
            PushOptIn = false,
        });
        response.EnsureSuccessStatusCode();

        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var employee = db.Users.Single(u => u.Email == "employee@test.com");
        var preference = db.NotificationPreferences.Single(p => p.UserId == employee.Id);
        Assert.False(preference.PushOptIn);
        Assert.Null(preference.PushSubscription);
    }
}
