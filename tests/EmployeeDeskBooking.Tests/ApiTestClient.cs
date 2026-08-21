using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using EmployeeDeskBooking.Api.Contracts.Auth;

namespace EmployeeDeskBooking.Tests;

public sealed class ApiTestClient(CustomApiApplicationFactory factory)
{
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
