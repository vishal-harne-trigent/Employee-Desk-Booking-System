using System.Text;
using EmployeeDeskBooking.Api;
using EmployeeDeskBooking.Application.Security;
using EmployeeDeskBooking.Domain.Users;
using EmployeeDeskBooking.Infrastructure;
using EmployeeDeskBooking.Infrastructure.Data;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;

namespace EmployeeDeskBooking.Tests;

public sealed class CustomApiApplicationFactory : WebApplicationFactory<ApiAssemblyMarker>
{
    public const string TestPassword = DbInitializer.DefaultPassword;
    private const string TestSigningKey = "test-signing-key-at-least-32-characters-long";
    private const string TestIssuer = "EmployeeDeskBooking.Tests";
    private const string TestAudience = "EmployeeDeskBooking.Tests";

    private readonly string _databaseName = Guid.NewGuid().ToString();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        builder.ConfigureAppConfiguration((_, config) =>
        {
            config.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Jwt:Issuer"] = TestIssuer,
                ["Jwt:Audience"] = TestAudience,
                ["Jwt:SigningKey"] = TestSigningKey,
                ["Jwt:ExpiryMinutes"] = "60",
            });
        });

        builder.ConfigureServices(services =>
        {
            services.RemoveAll(typeof(DbContextOptions<AppDbContext>));
            services.RemoveAll(typeof(AppDbContext));

            services.AddDbContext<AppDbContext>(options =>
                options.UseInMemoryDatabase(_databaseName));

            services.PostConfigure<JwtBearerOptions>(JwtBearerDefaults.AuthenticationScheme, options =>
            {
                options.MapInboundClaims = true;
                options.TokenValidationParameters.ValidIssuer = TestIssuer;
                options.TokenValidationParameters.ValidAudience = TestAudience;
                options.TokenValidationParameters.IssuerSigningKey =
                    new SymmetricSecurityKey(Encoding.UTF8.GetBytes(TestSigningKey));
            });
        });
    }

    protected override IHost CreateHost(IHostBuilder builder)
    {
        var host = base.CreateHost(builder);

        using var scope = host.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        db.Database.EnsureCreated();
        SeedTestUsers(scope.ServiceProvider).GetAwaiter().GetResult();

        return host;
    }

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

    public HttpClient CreateApiClient() => CreateClient();
}
