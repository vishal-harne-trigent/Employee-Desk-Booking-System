using AngleSharp;
using AngleSharp.Html.Dom;
using EmployeeDeskBooking.Application.Security;
using EmployeeDeskBooking.Domain.Users;
using EmployeeDeskBooking.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace EmployeeDeskBooking.Tests;

public sealed class AdminUsersTestClient(CustomWebApplicationFactory factory)
{
    private static readonly IBrowsingContext HtmlParser =
        BrowsingContext.New(Configuration.Default.WithDefaultLoader());

    public LoginTestClient Login { get; } = factory.CreateLoginTestClient();

    public async Task LoginAsAdminAsync()
    {
        await EnsureAdminLoginReadyAsync();
        var response = await Login.LoginAsync("admin@test.com", CustomWebApplicationFactory.TestPassword);
        Assert.Equal(302, (int)response.StatusCode);
    }

    public Task<HttpResponseMessage> GetAdminUsersPageAsync() =>
        Login.Client.GetAsync("/Admin/AdminUsers");

    public async Task<HttpResponseMessage> CreateUserAsync(
        string email,
        string name,
        string role,
        string password)
    {
        var token = await GetTokenFromPageAsync();
        var form = new Dictionary<string, string>
        {
            ["email"] = email,
            ["name"] = name,
            ["role"] = role,
            ["password"] = password,
            ["__RequestVerificationToken"] = token,
        };
        return await Login.Client.PostAsync("/Admin/AdminUsers/Create", new FormUrlEncodedContent(form));
    }

    public async Task<HttpResponseMessage> EditUserAsync(
        Guid userId,
        string email,
        string name,
        string role)
    {
        var token = await GetTokenFromPageAsync();
        var form = new Dictionary<string, string>
        {
            ["userId"] = userId.ToString(),
            ["email"] = email,
            ["name"] = name,
            ["role"] = role,
            ["__RequestVerificationToken"] = token,
        };
        return await Login.Client.PostAsync("/Admin/AdminUsers/Edit", new FormUrlEncodedContent(form));
    }

    public async Task<HttpResponseMessage> DeactivateUserAsync(Guid userId)
    {
        var token = await GetTokenFromPageAsync();
        var form = new Dictionary<string, string>
        {
            ["userId"] = userId.ToString(),
            ["__RequestVerificationToken"] = token,
        };
        return await Login.Client.PostAsync("/Admin/AdminUsers/Deactivate", new FormUrlEncodedContent(form));
    }

    public async Task<HttpResponseMessage> ActivateUserAsync(Guid userId)
    {
        var token = await GetTokenFromPageAsync();
        var form = new Dictionary<string, string>
        {
            ["userId"] = userId.ToString(),
            ["__RequestVerificationToken"] = token,
        };
        return await Login.Client.PostAsync("/Admin/AdminUsers/Activate", new FormUrlEncodedContent(form));
    }

    public Task<HttpResponseMessage> GetResetPasswordPageAsync(Guid userId) =>
        Login.Client.GetAsync($"/Admin/AdminUsers/ResetPassword?userId={userId}");

    public async Task<HttpResponseMessage> ResetPasswordAsync(
        Guid userId,
        string newPassword,
        string confirmPassword)
    {
        var page = await GetResetPasswordPageAsync(userId);
        page.EnsureSuccessStatusCode();
        var token = await GetAntiforgeryTokenAsync(await page.Content.ReadAsStringAsync());
        var form = new Dictionary<string, string>
        {
            ["userId"] = userId.ToString(),
            ["newPassword"] = newPassword,
            ["confirmPassword"] = confirmPassword,
            ["__RequestVerificationToken"] = token,
        };
        return await Login.Client.PostAsync("/Admin/AdminUsers/ResetPassword", new FormUrlEncodedContent(form));
    }

    private async Task<string> GetTokenFromPageAsync()
    {
        var page = await GetAdminUsersPageAsync();
        page.EnsureSuccessStatusCode();
        return await GetAntiforgeryTokenAsync(await page.Content.ReadAsStringAsync());
    }

    private static async Task<string> GetAntiforgeryTokenAsync(string html)
    {
        var document = await HtmlParser.OpenAsync(req => req.Content(html));
        var input = document.QuerySelector("input[name='__RequestVerificationToken']") as IHtmlInputElement
            ?? throw new InvalidOperationException("Anti-forgery token input was not found.");
        return input.Value ?? throw new InvalidOperationException("Anti-forgery token value was empty.");
    }

    private async Task EnsureAdminLoginReadyAsync()
    {
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var passwordVerifier = scope.ServiceProvider.GetRequiredService<IPasswordVerifier>();
        var admin = await db.Users.SingleAsync(u => u.Email == "admin@test.com");
        admin.IsActive = true;
        admin.Role = UserRole.Admin;
        admin.PasswordHash = passwordVerifier.HashPassword(admin, CustomWebApplicationFactory.TestPassword);

        var extraAdmins = await db.Users
            .Where(u => u.Id != admin.Id && u.Role == UserRole.Admin && u.IsActive)
            .ToListAsync();
        foreach (var user in extraAdmins)
        {
            user.Role = UserRole.Employee;
        }

        await db.SaveChangesAsync();
    }
}
