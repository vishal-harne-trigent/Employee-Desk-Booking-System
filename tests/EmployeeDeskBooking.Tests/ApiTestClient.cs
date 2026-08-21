using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using EmployeeDeskBooking.Api.Contracts.Auth;
using EmployeeDeskBooking.Api.Contracts.Bookings;
using EmployeeDeskBooking.Domain.Bookings;
using EmployeeDeskBooking.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace EmployeeDeskBooking.Tests;

public sealed class ApiTestClient(CustomApiApplicationFactory factory)
{
    public static readonly DateOnly FixedToday = BookDeskTestClient.FixedToday;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
    };

    public HttpClient Client { get; } = factory.CreateApiClient();

    public Task<HttpResponseMessage> LoginAsync(string email, string password) =>
        Client.PostAsJsonAsync("/api/auth/login", new LoginRequest
        {
            Email = email,
            Password = password,
        });

    public async Task<(HttpResponseMessage Response, LoginResponse? Body)> LoginWithBodyAsync(
        string email,
        string password)
    {
        var response = await LoginAsync(email, password);
        if (!response.IsSuccessStatusCode)
        {
            return (response, null);
        }

        var body = await response.Content.ReadFromJsonAsync<LoginResponse>(JsonOptions);
        return (response, body);
    }

    public Task<HttpResponseMessage> GetCurrentUserAsync() =>
        Client.GetAsync("/api/auth/me");

    public async Task AuthorizeAsEmployeeAsync()
    {
        var (response, body) = await LoginWithBodyAsync("employee@test.com", CustomApiApplicationFactory.TestPassword);
        response.EnsureSuccessStatusCode();
        SetBearerToken(body!.AccessToken);
    }

    public async Task AuthorizeAsAdminAsync()
    {
        var (response, body) = await LoginWithBodyAsync("admin@test.com", CustomApiApplicationFactory.TestPassword);
        response.EnsureSuccessStatusCode();
        SetBearerToken(body!.AccessToken);
    }

    public Task<HttpResponseMessage> GetAdminBookingsAsync(DateOnly? date = null, BookingStatus? status = null)
    {
        var query = new List<string>();
        if (date.HasValue)
        {
            query.Add($"date={date.Value:yyyy-MM-dd}");
        }

        if (status.HasValue)
        {
            query.Add($"status={status.Value}");
        }

        var qs = query.Count > 0 ? "?" + string.Join("&", query) : string.Empty;
        return Client.GetAsync($"/api/admin/bookings{qs}");
    }

    public Task<HttpResponseMessage> AdminCancelBookingAsync(Guid bookingId) =>
        Client.PostAsync($"/api/admin/bookings/{bookingId}/cancel", null);

    public Task<HttpResponseMessage> GetAdminDesksAsync() =>
        Client.GetAsync("/api/admin/desks");

    public Task<HttpResponseMessage> CreateAdminDeskAsync(string deskNumber) =>
        Client.PostAsJsonAsync("/api/admin/desks", new { deskNumber });

    public Task<HttpResponseMessage> UpdateAdminDeskAsync(Guid deskId, string deskNumber) =>
        Client.PutAsJsonAsync($"/api/admin/desks/{deskId}", new { deskNumber });

    public Task<HttpResponseMessage> DeactivateAdminDeskAsync(Guid deskId) =>
        Client.PostAsync($"/api/admin/desks/{deskId}/deactivate", null);

    public Task<HttpResponseMessage> ActivateAdminDeskAsync(Guid deskId) =>
        Client.PostAsync($"/api/admin/desks/{deskId}/activate", null);

    public Task<HttpResponseMessage> GetAdminUsersAsync() =>
        Client.GetAsync("/api/admin/users");

    public Task<HttpResponseMessage> CreateAdminUserAsync(
        string email,
        string name,
        string role,
        string password) =>
        Client.PostAsJsonAsync("/api/admin/users", new { email, name, role, password });

    public Task<HttpResponseMessage> UpdateAdminUserAsync(
        Guid userId,
        string email,
        string name,
        string role) =>
        Client.PutAsJsonAsync($"/api/admin/users/{userId}", new { email, name, role });

    public Task<HttpResponseMessage> DeactivateAdminUserAsync(Guid userId) =>
        Client.PostAsync($"/api/admin/users/{userId}/deactivate", null);

    public Task<HttpResponseMessage> ResetAdminUserPasswordAsync(Guid userId) =>
        Client.PostAsync($"/api/admin/users/{userId}/reset-password", null);

    public Task<HttpResponseMessage> GetAvailabilityAsync(DateOnly date) =>
        Client.GetAsync($"/api/bookings/availability?date={date:yyyy-MM-dd}");

    public Task<HttpResponseMessage> CreateBookingAsync(Guid deskId, DateOnly date) =>
        Client.PostAsJsonAsync("/api/bookings", new CreateBookingRequest
        {
            DeskId = deskId,
            Date = date,
        });

    public Task<HttpResponseMessage> GetMyBookingsAsync() =>
        Client.GetAsync("/api/bookings/mine");

    public Task<HttpResponseMessage> CancelBookingAsync(Guid bookingId) =>
        Client.PostAsync($"/api/bookings/{bookingId}/cancel", null);

    public Guid GetDeskIdByNumber(string deskNumber)
    {
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        return db.Desks.Single(d => d.DeskNumber == deskNumber).Id;
    }

    public void SetBearerToken(string token) =>
        Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

    public static async Task<string> ReadProblemDetailAsync(HttpResponseMessage response)
    {
        var json = await response.Content.ReadAsStringAsync();
        using var document = JsonDocument.Parse(json);
        return document.RootElement.TryGetProperty("detail", out var detail)
            ? detail.GetString() ?? string.Empty
            : json;
    }
}
