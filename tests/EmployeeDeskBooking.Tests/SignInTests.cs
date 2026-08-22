using EmployeeDeskBooking.Web.Auth;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Testing;

namespace EmployeeDeskBooking.Tests;

public class SignInTests(CustomWebApplicationFactory factory) : IClassFixture<CustomWebApplicationFactory>
{
    [Fact(DisplayName = "Employee lands on Book Desk after sign-in (US-001/AC-01)")]
    public async Task Employee_sign_in_redirects_to_book_desk()
    {
        var login = factory.CreateLoginTestClient();
        var response = await login.LoginAsync("employee@test.com", CustomWebApplicationFactory.TestPassword);

        Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);
        Assert.Equal("/Desks/Availability", response.Headers.Location?.OriginalString);

        var bookPage = await login.Client.GetAsync(response.Headers.Location!);
        var html = await bookPage.Content.ReadAsStringAsync();
        Assert.Contains("Desk Availability", html);
    }

    [Fact(DisplayName = "Admin lands on All Bookings after sign-in (US-001/AC-02)")]
    public async Task Admin_sign_in_redirects_to_admin_bookings()
    {
        var login = factory.CreateLoginTestClient();
        var response = await login.LoginAsync("admin@test.com", CustomWebApplicationFactory.TestPassword);

        Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);
        Assert.Equal("/Admin/AdminBookings", response.Headers.Location?.OriginalString);

        var adminPage = await login.Client.GetAsync(response.Headers.Location!);
        var html = await adminPage.Content.ReadAsStringAsync();
        Assert.Contains("All Bookings", html);
    }

    [Fact(DisplayName = "Invalid credentials rejected with generic error (US-001/AC-03)")]
    public async Task Invalid_credentials_show_generic_error_and_no_session()
    {
        var login = factory.CreateLoginTestClient();
        var response = await login.LoginAsync("employee@test.com", "WrongPassword!");

        Assert.Equal(StatusCodes.Status200OK, (int)response.StatusCode);
        var html = await response.Content.ReadAsStringAsync();
        Assert.Contains(AuthMessages.InvalidCredentials, html);

        var protectedResponse = await login.Client.GetAsync("/Desks/Availability");
        Assert.Equal(StatusCodes.Status302Found, (int)protectedResponse.StatusCode);
        Assert.Contains("/Account/Login", protectedResponse.Headers.Location?.OriginalString, StringComparison.OrdinalIgnoreCase);
    }

    [Fact(DisplayName = "Deactivated account rejected with deactivated message (US-001/AC-04)")]
    public async Task Deactivated_account_shows_deactivated_message()
    {
        var login = factory.CreateLoginTestClient();
        var response = await login.LoginAsync("deactivated@test.com", CustomWebApplicationFactory.TestPassword);

        Assert.Equal(StatusCodes.Status200OK, (int)response.StatusCode);
        var html = await response.Content.ReadAsStringAsync();
        Assert.Contains(AuthMessages.DeactivatedAccount, html);
    }

    [Fact(DisplayName = "Sign out ends session and returns to sign-in (US-001/AC-05)")]
    public async Task Sign_out_returns_to_sign_in_screen()
    {
        var login = factory.CreateLoginTestClient();
        var signIn = await login.LoginAsync("employee@test.com", CustomWebApplicationFactory.TestPassword);
        Assert.Equal(StatusCodes.Status302Found, (int)signIn.StatusCode);

        var logout = await login.LogoutFromAsync("/Desks/Availability");
        Assert.Equal(StatusCodes.Status302Found, (int)logout.StatusCode);
        var logoutLocation = logout.Headers.Location?.OriginalString ?? string.Empty;
        Assert.True(
            logoutLocation is "/" or "/Account/Login",
            $"Expected sign-in redirect, got {logoutLocation}");

        var protectedResponse = await login.Client.GetAsync("/Desks/Availability");
        Assert.Equal(StatusCodes.Status302Found, (int)protectedResponse.StatusCode);
        Assert.Contains("/Account/Login", protectedResponse.Headers.Location?.OriginalString, StringComparison.OrdinalIgnoreCase);
    }
}
