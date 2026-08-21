using EmployeeDeskBooking.Api.Auth;
using Microsoft.AspNetCore.Http;

namespace EmployeeDeskBooking.Tests;

public class ApiAuthTests(CustomApiApplicationFactory factory) : IClassFixture<CustomApiApplicationFactory>
{
    [Fact(DisplayName = "API login returns Employee role token (US-001/AC-01)")]
    public async Task Api_login_returns_employee_role_US_001_AC_01()
    {
        var client = new ApiTestClient(factory);
        var (response, body) = await client.LoginWithBodyAsync(
            "employee@test.com",
            CustomApiApplicationFactory.TestPassword);

        Assert.Equal(StatusCodes.Status200OK, (int)response.StatusCode);
        Assert.NotNull(body);
        Assert.Equal("Employee", body.Role);
        Assert.False(string.IsNullOrWhiteSpace(body.AccessToken));
    }

    [Fact(DisplayName = "API login returns Admin role token (US-001/AC-02)")]
    public async Task Api_login_returns_admin_role_US_001_AC_02()
    {
        var client = new ApiTestClient(factory);
        var (response, body) = await client.LoginWithBodyAsync(
            "admin@test.com",
            CustomApiApplicationFactory.TestPassword);

        Assert.Equal(StatusCodes.Status200OK, (int)response.StatusCode);
        Assert.NotNull(body);
        Assert.Equal("Admin", body.Role);
    }

    [Fact(DisplayName = "API invalid credentials return 401 (US-001/AC-03)")]
    public async Task Api_invalid_credentials_return_401_US_001_AC_03()
    {
        var client = new ApiTestClient(factory);
        var response = await client.LoginAsync("employee@test.com", "WrongPassword!");

        Assert.Equal(StatusCodes.Status401Unauthorized, (int)response.StatusCode);
        var detail = await ApiTestClient.ReadProblemDetailAsync(response);
        Assert.Contains(ApiAuthMessages.InvalidCredentials, detail, StringComparison.Ordinal);
    }

    [Fact(DisplayName = "API deactivated account returns 403 (US-001/AC-04)")]
    public async Task Api_deactivated_account_returns_403_US_001_AC_04()
    {
        var client = new ApiTestClient(factory);
        var response = await client.LoginAsync("deactivated@test.com", CustomApiApplicationFactory.TestPassword);

        Assert.Equal(StatusCodes.Status403Forbidden, (int)response.StatusCode);
        var detail = await ApiTestClient.ReadProblemDetailAsync(response);
        Assert.Contains(ApiAuthMessages.DeactivatedAccount, detail, StringComparison.Ordinal);
    }

    [Fact(DisplayName = "API protected booking routes require JWT (US-001/AC-05)")]
    public async Task Api_protected_routes_require_jwt_US_001_AC_05()
    {
        var client = new ApiTestClient(factory);
        var response = await client.GetAvailabilityAsync(ApiTestClient.FixedToday.AddDays(1));

        Assert.Equal(StatusCodes.Status401Unauthorized, (int)response.StatusCode);
    }
}
