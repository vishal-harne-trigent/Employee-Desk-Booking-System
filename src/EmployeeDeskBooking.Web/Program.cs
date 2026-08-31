using EmployeeDeskBooking.Application;
using EmployeeDeskBooking.Infrastructure;
using EmployeeDeskBooking.Infrastructure.Notifications;
using EmployeeDeskBooking.Web.Services;
using Microsoft.AspNetCore.Authentication.Cookies;
using Infra = EmployeeDeskBooking.Infrastructure.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);

if (builder.Environment.IsDevelopment() || builder.Environment.IsEnvironment("Demo"))
{
    builder.Configuration.AddJsonFile(
        $"appsettings.{builder.Environment.EnvironmentName}.local.json",
        optional: true,
        reloadOnChange: true);
}

builder.Services.AddApplication();
var isTesting = builder.Environment.IsEnvironment("Testing");
var reminderJobEnabled = builder.Configuration.GetValue("ReminderJob:Enabled", true);
builder.Services.AddInfrastructure(
    builder.Configuration,
    enableReminderJob: reminderJobEnabled && !isTesting,
    enableCompletionJob: !isTesting);

builder.Services
    .AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login";
        options.LogoutPath = "/Account/Logout";
        options.AccessDeniedPath = "/Account/Login";
        options.Cookie.HttpOnly = true;
        options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
        options.Cookie.SameSite = SameSiteMode.Strict;
    });

builder.Services.AddAuthorization();
builder.Services.AddControllersWithViews();
builder.Services.AddScoped<BookPageModelFactory>();

var app = builder.Build();

if (app.Environment.IsDevelopment() && !isTesting)
{
    var emailEnabled = builder.Configuration.GetValue("Email:Enabled", false)
        || builder.Configuration.GetValue("Smtp:Enabled", false);
    var deliveryMode = builder.Configuration["Smtp:Mode"]
        ?? builder.Configuration["Email:DeliveryMode"]
        ?? EmailOptions.DeliveryModeSmtp;
    if (emailEnabled
        && string.Equals(deliveryMode, EmailOptions.DeliveryModeSmtp, StringComparison.OrdinalIgnoreCase))
    {
        var logger = app.Services.GetRequiredService<ILoggerFactory>().CreateLogger("Email");
        var username = builder.Configuration["Smtp:Username"]
            ?? builder.Configuration["Email:Username"];
        if (string.IsNullOrWhiteSpace(username))
        {
            logger.LogWarning(
                "SMTP mode is active but no mailbox credentials are configured. " +
                "Set Smtp:Username and Smtp:Password, or switch Smtp:Mode to FileDrop for local development.");
        }
    }
}

if (!app.Environment.IsEnvironment("Testing"))
{
    await Infra.InitializeDatabaseAsync(app.Services, app.Environment.IsDevelopment());
}

if (!app.Environment.IsDevelopment() && !app.Environment.IsEnvironment("Testing"))
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

if (!app.Environment.IsDevelopment() && !app.Environment.IsEnvironment("Testing"))
{
    app.UseHttpsRedirection();
}

app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "areas",
    pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}");

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Account}/{action=Login}/{id?}");

app.Run();

public partial class Program;
