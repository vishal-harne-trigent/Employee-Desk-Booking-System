using EmployeeDeskBooking.Domain.Users;
using EmployeeDeskBooking.Infrastructure.Data;
using EmployeeDeskBooking.Web.Auth;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Text.RegularExpressions;

namespace EmployeeDeskBooking.Tests;

public class AdminUsersTests(CustomWebApplicationFactory factory) : IClassFixture<CustomWebApplicationFactory>
{
    private const string TestPassword = CustomWebApplicationFactory.TestPassword;

    [Fact(DisplayName = "Admin creates user who can sign in (US-006/AC-01)")]
    public async Task Admin_creates_user_US_006_AC_01()
    {
        var email = UniqueEmail("create");
        var client = new AdminUsersTestClient(factory);
        await client.LoginAsAdminAsync();

        var response = await client.CreateUserAsync(email, "New User", "0", TestPassword);
        response.EnsureSuccessStatusCode();

        var login = factory.CreateLoginTestClient();
        var loginResponse = await login.LoginAsync(email, TestPassword);
        Assert.Equal(302, (int)loginResponse.StatusCode);
        Assert.StartsWith("/Book", loginResponse.Headers.Location?.OriginalString, StringComparison.Ordinal);
    }

    [Fact(DisplayName = "Admin cannot save duplicate email (US-006/AC-02)")]
    public async Task Admin_rejects_duplicate_email_US_006_AC_02()
    {
        var client = new AdminUsersTestClient(factory);
        await client.LoginAsAdminAsync();

        var response = await client.CreateUserAsync("employee@test.com", "Duplicate", "0", TestPassword);
        var body = await response.Content.ReadAsStringAsync();

        response.EnsureSuccessStatusCode();
        Assert.Contains("already in use", body, StringComparison.OrdinalIgnoreCase);
    }

    [Fact(DisplayName = "Admin edits user name and email (US-006/AC-03)")]
    public async Task Admin_edits_user_US_006_AC_03()
    {
        var email = UniqueEmail("edit");
        var client = new AdminUsersTestClient(factory);
        await client.LoginAsAdminAsync();
        await client.CreateUserAsync(email, "Edit Me", "0", TestPassword);

        Guid userId;
        using (var scope = factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            userId = db.Users.Single(u => u.Email == email).Id;
        }

        var updatedEmail = UniqueEmail("edited");
        var response = await client.EditUserAsync(userId, updatedEmail, "Edited Name", "0");
        var body = await response.Content.ReadAsStringAsync();

        response.EnsureSuccessStatusCode();
        Assert.Contains(updatedEmail, body, StringComparison.Ordinal);
        Assert.Contains("Edited Name", body, StringComparison.Ordinal);
    }

    [Fact(DisplayName = "Admin deactivates user who cannot sign in (US-006/AC-04)")]
    public async Task Admin_deactivates_user_US_006_AC_04()
    {
        var email = UniqueEmail("deactivate");
        var client = new AdminUsersTestClient(factory);
        await client.LoginAsAdminAsync();
        await client.CreateUserAsync(email, "Deactivate Me", "0", TestPassword);

        Guid userId;
        using (var scope = factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            userId = db.Users.Single(u => u.Email == email).Id;
        }

        var deactivate = await client.DeactivateUserAsync(userId);
        deactivate.EnsureSuccessStatusCode();

        var login = factory.CreateLoginTestClient();
        var loginResponse = await login.LoginAsync(email, TestPassword);
        Assert.Equal(StatusCodes.Status200OK, (int)loginResponse.StatusCode);
        var body = await loginResponse.Content.ReadAsStringAsync();
        Assert.Contains(AuthMessages.DeactivatedAccount, body);
    }

    [Fact(DisplayName = "Admin reset password shows one-time password (US-006/AC-05)")]
    public async Task Admin_resets_password_US_006_AC_05()
    {
        var email = UniqueEmail("reset");
        var client = new AdminUsersTestClient(factory);
        await client.LoginAsAdminAsync();
        await client.CreateUserAsync(email, "Reset Me", "0", TestPassword);

        Guid userId;
        using (var scope = factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            userId = db.Users.Single(u => u.Email == email).Id;
        }

        var response = await client.ResetPasswordAsync(userId);
        var body = await response.Content.ReadAsStringAsync();

        response.EnsureSuccessStatusCode();
        var match = Regex.Match(body, @"<code class=""temporary-password"">([^<]+)</code>");
        Assert.True(match.Success, "Temporary password panel was not rendered.");
        var temporaryPassword = match.Groups[1].Value;

        var login = factory.CreateLoginTestClient();
        var loginResponse = await login.LoginAsync(email, temporaryPassword);
        Assert.Equal(302, (int)loginResponse.StatusCode);
    }

    [Fact(DisplayName = "Admin changes user role (US-006/AC-06)")]
    public async Task Admin_changes_role_US_006_AC_06()
    {
        var email = UniqueEmail("promote");
        var client = new AdminUsersTestClient(factory);
        await client.LoginAsAdminAsync();
        await client.CreateUserAsync(email, "Promote Me", "0", TestPassword);

        Guid userId;
        using (var scope = factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            userId = db.Users.Single(u => u.Email == email).Id;
        }

        var response = await client.EditUserAsync(userId, email, "Promote Me", "1");
        response.EnsureSuccessStatusCode();

        using var verifyScope = factory.Services.CreateScope();
        var verifyDb = verifyScope.ServiceProvider.GetRequiredService<AppDbContext>();
        var user = await verifyDb.Users.SingleAsync(u => u.Id == userId);
        Assert.Equal(UserRole.Admin, user.Role);
    }

    [Fact(DisplayName = "Admin cannot remove last active Admin (US-006/AC-07)")]
    public async Task Admin_protects_last_admin_US_006_AC_07()
    {
        var client = new AdminUsersTestClient(factory);
        await client.LoginAsAdminAsync();

        Guid soleAdminId;
        using (var scope = factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            soleAdminId = db.Users.Single(u => u.Email == "admin@test.com").Id;
        }

        var deactivateResponse = await client.DeactivateUserAsync(soleAdminId);
        var deactivateBody = await deactivateResponse.Content.ReadAsStringAsync();
        Assert.Contains("last active Admin", deactivateBody, StringComparison.OrdinalIgnoreCase);

        var demoteResponse = await client.EditUserAsync(soleAdminId, "admin@test.com", "Test Admin", "0");
        var demoteBody = await demoteResponse.Content.ReadAsStringAsync();
        Assert.Contains("last active Admin", demoteBody, StringComparison.OrdinalIgnoreCase);
    }

    [Fact(DisplayName = "Employee cannot access manage users page (US-006/V-07)")]
    public async Task Employee_cannot_access_admin_users_V_07()
    {
        var client = new BookDeskTestClient(factory);
        await client.LoginAsEmployeeAsync();

        var response = await client.Login.Client.GetAsync("/Admin/AdminUsers");
        Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);
        Assert.Contains("/Account/Login", response.Headers.Location?.OriginalString, StringComparison.Ordinal);
    }

    private static string UniqueEmail(string prefix) => $"{prefix}.{Guid.NewGuid():N}@test.com";
}
