using EmployeeDeskBooking.Application.Notifications;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MimeKit;

namespace EmployeeDeskBooking.Infrastructure.Notifications;

public sealed class MailKitEmailSender(IOptions<EmailOptions> options, ILogger<MailKitEmailSender> logger) : IEmailSender
{
    public async Task SendAsync(EmailMessage message, CancellationToken cancellationToken = default)
    {
        var settings = options.Value;
        EnsureSmtpCredentials(settings);

        var mime = new MimeMessage();
        mime.From.Add(new MailboxAddress(settings.FromName, settings.FromAddress));
        mime.To.Add(MailboxAddress.Parse(message.To));
        mime.Subject = message.Subject;
        mime.Body = new TextPart("html") { Text = message.HtmlBody };

        using var client = new SmtpClient();
        try
        {
            await client.ConnectAsync(
                settings.SmtpHost,
                settings.SmtpPort,
                settings.UseStartTls ? SecureSocketOptions.StartTls : SecureSocketOptions.Auto,
                cancellationToken);
            await client.AuthenticateAsync(settings.Username!, settings.Password ?? string.Empty, cancellationToken);
            await client.SendAsync(mime, cancellationToken);
            await client.DisconnectAsync(true, cancellationToken);
            logger.LogInformation("Email sent to {Recipient} subject {Subject}", message.To, message.Subject);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to send email to {Recipient} subject {Subject}", message.To, message.Subject);
            throw;
        }
    }

    private static void EnsureSmtpCredentials(EmailOptions settings)
    {
        if (IsLocalSmtp(settings))
        {
            return;
        }

        if (string.IsNullOrWhiteSpace(settings.Username))
        {
            throw new InvalidOperationException(
                "SMTP username is not configured. Set Smtp:Username and Smtp:Password in user secrets, environment variables, or appsettings.Development.local.json.");
        }

        if (string.IsNullOrWhiteSpace(settings.Password)
            || settings.Password.Contains("REPLACE_WITH", StringComparison.OrdinalIgnoreCase)
            || settings.Password.Contains("PASTE_", StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException(
                "SMTP password is not configured. Set Smtp:Password in user secrets, environment variables, or appsettings.Development.local.json, or use Smtp:Mode FileDrop for local development.");
        }
    }

    private static bool IsLocalSmtp(EmailOptions settings) =>
        string.Equals(settings.SmtpHost, "localhost", StringComparison.OrdinalIgnoreCase)
        || string.Equals(settings.SmtpHost, "127.0.0.1", StringComparison.OrdinalIgnoreCase);
}

public sealed class NoOpEmailSender(ILogger<NoOpEmailSender> logger) : IEmailSender
{
    public Task SendAsync(EmailMessage message, CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Email disabled — skipped send to {Recipient} subject {Subject}", message.To, message.Subject);
        return Task.CompletedTask;
    }
}
