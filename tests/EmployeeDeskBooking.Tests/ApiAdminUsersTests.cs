using EmployeeDeskBooking.Domain.Users;
using EmployeeDeskBooking.Infrastructure.Data;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Net.Http.Json;

namespace EmployeeDeskBooking.Tests;

public class ApiAdminUsersTests(CustomApiApplicationFactory factory) : IClassFixture<CustomApiApplicationFactory>
{
    private const string TestPassword = CustomApiApplicationFactory.TestPassword;

    [Fact(DisplayName = "API admin creates user who can sign in (US-006/AC-01)")]
    public async Task Api_admin_creates_user_US_006_AC_01()
    {
        var email = UniqueEmail("api-create");
        var client = new ApiTestClient(factory);
        await client.AuthorizeAsAdminAsync();

        var response = await client.CreateAdminUserAsync(email, "Api User", "Employee", TestPassword);
        Assert.Equal(StatusCodes.Status201Created, (int)response.StatusCode);

        var loginClient = new ApiTestClient(factory);
        var login = await loginClient.LoginAsync(email, TestPassword);
        login.EnsureSuccessStatusCode();
    }

    [Fact(DisplayName = "API admin rejects duplicate email (US-006/AC-02)")]
    public async Task Api_admin_rejects_duplicate_email_US_006_AC_02()
    {
        var client = new ApiTestClient(factory);
        await client.AuthorizeAsAdminAsync();

        var response = await client.CreateAdminUserAsync("employee@test.com", "Dup", "Employee", TestPassword);
        Assert.Equal(StatusCodes.Status409Conflict, (int)response.StatusCode);
    }

    [Fact(DisplayName = "API admin updates user profile (US-006/AC-03)")]
    public async Task Api_admin_updates_user_US_006_AC_03()
    {
        var email = UniqueEmail("api-edit");
        var client = new ApiTestClient(factory);
        await client.AuthorizeAsAdminAsync();
        await client.CreateAdminUserAsync(email, "Before", "Employee", TestPassword);

        Guid userId;
        using (var scope = factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            userId = db.Users.Single(u => u.Email == email).Id;
        }

        var updatedEmail = UniqueEmail("api-edited");
        var response = await client.UpdateAdminUserAsync(userId, updatedEmail, "After", "Employee");
        Assert.Equal(StatusCodes.Status200OK, (int)response.StatusCode);

        var list = await client.GetAdminUsersAsync();
        var body = await list.Content.ReadFromJsonAsync<AdminUsersApiResponse>();
        Assert.Contains(body!.Users, u => u.Email == updatedEmail && u.Name == "After");
    }

    [Fact(DisplayName = "API admin deactivates user (US-006/AC-04)")]
    public async Task Api_admin_deactivates_user_US_006_AC_04()
    {
        var email = UniqueEmail("api-deactivate");
        var client = new ApiTestClient(factory);
        await client.AuthorizeAsAdminAsync();
        await client.CreateAdminUserAsync(email, "Deactivate", "Employee", TestPassword);

        Guid userId;
        using (var scope = factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            userId = db.Users.Single(u => u.Email == email).Id;
        }

        var response = await client.DeactivateAdminUserAsync(userId);
        Assert.Equal(StatusCodes.Status200OK, (int)response.StatusCode);

        var loginClient = new ApiTestClient(factory);
        var login = await loginClient.LoginAsync(email, TestPassword);
        Assert.Equal(StatusCodes.Status403Forbidden, (int)login.StatusCode);
    }

    [Fact(DisplayName = "API admin reset password returns temporary password (US-006/AC-05)")]
    public async Task Api_admin_resets_password_US_006_AC_05()
    {
        var email = UniqueEmail("api-reset");
        var client = new ApiTestClient(factory);
        await client.AuthorizeAsAdminAsync();
        await client.CreateAdminUserAsync(email, "Reset", "Employee", TestPassword);

        Guid userId;
        using (var scope = factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            userId = db.Users.Single(u => u.Email == email).Id;
        }

        var response = await client.ResetAdminUserPasswordAsync(userId);
        var body = await response.Content.ReadFromJsonAsync<ResetPasswordApiResponse>();
        Assert.NotNull(body);
        Assert.False(string.IsNullOrWhiteSpace(body!.TemporaryPassword));

        var loginClient = new ApiTestClient(factory);
        var login = await loginClient.LoginAsync(email, body.TemporaryPassword);
        login.EnsureSuccessStatusCode();
    }

    [Fact(DisplayName = "API admin changes user role (US-006/AC-06)")]
    public async Task Api_admin_changes_role_US_006_AC_06()
    {
        var email = UniqueEmail("api-promote");
        var client = new ApiTestClient(factory);
        await client.AuthorizeAsAdminAsync();
        await client.CreateAdminUserAsync(email, "Promote", "Employee", TestPassword);

        Guid userId;
        using (var scope = factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            userId = db.Users.Single(u => u.Email == email).Id;
        }

        var response = await client.UpdateAdminUserAsync(userId, email, "Promote", "Admin");
        Assert.Equal(StatusCodes.Status200OK, (int)response.StatusCode);

        using var verifyScope = factory.Services.CreateScope();
        var verifyDb = verifyScope.ServiceProvider.GetRequiredService<AppDbContext>();
        var user = await verifyDb.Users.SingleAsync(u => u.Id == userId);
        Assert.Equal(UserRole.Admin, user.Role);
    }

    [Fact(DisplayName = "API admin blocked from removing last Admin (US-006/AC-07)")]
    public async Task Api_admin_protects_last_admin_US_006_AC_07()
    {
        Guid soleAdminId;
        using (var scope = factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            soleAdminId = db.Users.Single(u => u.Email == "admin@test.com").Id;
        }

        var client = new ApiTestClient(factory);
        await client.AuthorizeAsAdminAsync();

        var deactivate = await client.DeactivateAdminUserAsync(soleAdminId);
        Assert.Equal(StatusCodes.Status409Conflict, (int)deactivate.StatusCode);

        var demote = await client.UpdateAdminUserAsync(soleAdminId, "admin@test.com", "Test Admin", "Employee");
        Assert.Equal(StatusCodes.Status409Conflict, (int)demote.StatusCode);
    }

    [Fact(DisplayName = "API employee cannot access admin users (US-006/V-07)")]
    public async Task Api_employee_denied_admin_users_V_07()
    {
        var client = new ApiTestClient(factory);
        await client.AuthorizeAsEmployeeAsync();

        var response = await client.GetAdminUsersAsync();
        Assert.Equal(StatusCodes.Status403Forbidden, (int)response.StatusCode);
    }

    private static string UniqueEmail(string prefix) => $"{prefix}.{Guid.NewGuid():N}@test.com";

    private sealed class AdminUsersApiResponse
    {
        public List<AdminUserApiItem> Users { get; set; } = [];
    }

    private sealed class AdminUserApiItem
    {
        public string Email { get; set; } = string.Empty;

        public string Name { get; set; } = string.Empty;
    }

    private sealed class ResetPasswordApiResponse
    {
        public string TemporaryPassword { get; set; } = string.Empty;
    }
}
