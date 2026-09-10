using EmployeeDeskBooking.Application.Notifications;
using EmployeeDeskBooking.Infrastructure.Notifications;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

const string defaultRecipient = "vishal_h@trigent.com";
var recipient = args.FirstOrDefault(defaultRecipient);
var webProjectPath = ResolveWebProjectPath();

var configuration = new ConfigurationBuilder()
    .SetBasePath(webProjectPath)
    .AddJsonFile("appsettings.json", optional: true)
    .AddJsonFile("appsettings.Development.json", optional: true)
    .AddJsonFile("appsettings.Development.local.json", optional: true)
    .AddUserSecrets("EmployeeDeskBooking.Web-dev")
    .AddEnvironmentVariables()
    .Build();

var emailOptions = new EmailOptions();
configuration.GetSection("Email").Bind(emailOptions);
BindSmtpOverrides(configuration.GetSection("Smtp"), emailOptions);

var useSmtp = emailOptions.HasConfiguredSmtpCredentials;
emailOptions.DeliveryMode = useSmtp ? EmailOptions.DeliveryModeSmtp : EmailOptions.DeliveryModeFileDrop;
emailOptions.Enabled = true;

var services = new ServiceCollection();
services.AddLogging(builder => builder.AddConsole().SetMinimumLevel(LogLevel.Information));
services.AddSingleton<IConfiguration>(configuration);
services.AddSingleton<IHostEnvironment>(new TestHostEnvironment(webProjectPath));
services.Configure<EmailOptions>(_ =>
{
    _.Enabled = emailOptions.Enabled;
    _.DeliveryMode = emailOptions.DeliveryMode;
    _.FileDropPath = emailOptions.FileDropPath;
    _.FromAddress = emailOptions.FromAddress;
    _.FromName = emailOptions.FromName;
    _.SmtpHost = emailOptions.SmtpHost;
    _.SmtpPort = emailOptions.SmtpPort;
    _.UseStartTls = emailOptions.UseStartTls;
    _.Username = emailOptions.Username;
    _.Password = emailOptions.Password;
});

if (useSmtp)
{
    services.AddSingleton<IEmailSender, MailKitEmailSender>();
    Console.WriteLine($"Sending live SMTP test email to {recipient} via {emailOptions.SmtpHost} as {emailOptions.Username}...");
}
else
{
    services.AddSingleton<IEmailSender, FileDropEmailSender>();
    Console.WriteLine("SMTP credentials not configured — saving test email to App_Data/sent-emails instead.");
    Console.WriteLine("Set Smtp:Username and Smtp:Password in user secrets or appsettings.Development.local.json for live delivery.");
}

var sender = services.BuildServiceProvider().GetRequiredService<IEmailSender>();
var message = new EmailMessage
{
    To = recipient,
    Subject = "EDBS test email — Desk Booking System",
    HtmlBody = "<p>This is a test email from the Employee Desk Booking System.</p>",
};

try
{
    await sender.SendAsync(message);
    Console.WriteLine(useSmtp
        ? $"Test email sent to {recipient}."
        : $"Test email saved for {recipient} under {Path.Combine(webProjectPath, emailOptions.FileDropPath)}");
    return 0;
}
catch (Exception ex)
{
    Console.Error.WriteLine($"Failed to send test email: {ex.Message}");
    return 1;
}

static string ResolveWebProjectPath()
{
    var candidates = new[]
    {
        Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), "..", "..", "src", "EmployeeDeskBooking.Web")),
        Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), "src", "EmployeeDeskBooking.Web")),
        Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "..", "src", "EmployeeDeskBooking.Web")),
    };

    foreach (var candidate in candidates)
    {
        if (Directory.Exists(candidate))
        {
            return candidate;
        }
    }

    throw new DirectoryNotFoundException("Could not locate src/EmployeeDeskBooking.Web.");
}

static void BindSmtpOverrides(IConfigurationSection smtp, EmailOptions options)
{
    if (!smtp.Exists())
    {
        return;
    }

    options.Enabled = smtp.GetValue("Enabled", options.Enabled);
    options.DeliveryMode = smtp["Mode"] ?? options.DeliveryMode;
    options.FileDropPath = smtp["FileDropPath"] ?? options.FileDropPath;
    options.SmtpHost = smtp["Host"] ?? options.SmtpHost;
    options.SmtpPort = smtp.GetValue("Port", options.SmtpPort);
    options.UseStartTls = smtp.GetValue("UseSsl", options.UseStartTls);
    options.FromName = smtp["FromName"] ?? options.FromName;
    options.FromAddress = smtp["FromAddress"] ?? options.FromAddress;
    options.Username = smtp["Username"] ?? options.Username;
    options.Password = smtp["Password"] ?? options.Password;
}

internal sealed class TestHostEnvironment(string contentRoot) : IHostEnvironment
{
    public string EnvironmentName { get; set; } = Environments.Development;

    public string ApplicationName { get; set; } = "SendTestEmail";

    public string ContentRootPath { get; set; } = contentRoot;

    public IFileProvider ContentRootFileProvider { get; set; } = new NullFileProvider();
}
