using EmployeeDeskBooking.Application.Notifications;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Text;

namespace EmployeeDeskBooking.Infrastructure.Notifications;

public sealed class FileDropEmailSender(
    IOptions<EmailOptions> options,
    IHostEnvironment environment,
    ILogger<FileDropEmailSender> logger) : IEmailSender
{
    public async Task SendAsync(EmailMessage message, CancellationToken cancellationToken = default)
    {
        var settings = options.Value;
        var directory = Path.IsPathRooted(settings.FileDropPath)
            ? settings.FileDropPath
            : Path.Combine(environment.ContentRootPath, settings.FileDropPath);

        Directory.CreateDirectory(directory);

        var timestamp = DateTimeOffset.UtcNow.ToString("yyyyMMdd-HHmmss-fff");
        var safeRecipient = SanitizeFileNameSegment(message.To);
        var safeSubject = SanitizeFileNameSegment(message.Subject);
        var fileName = $"{timestamp}_{safeRecipient}_{safeSubject}.html";
        var filePath = Path.Combine(directory, fileName);

        var content = new StringBuilder()
            .AppendLine("<!DOCTYPE html>")
            .AppendLine("<html><head><meta charset=\"utf-8\" />")
            .Append("<title>").Append(EscapeHtml(message.Subject)).AppendLine("</title></head><body>")
            .Append("<p><strong>To:</strong> ").Append(EscapeHtml(message.To)).AppendLine("</p>")
            .Append("<p><strong>Subject:</strong> ").Append(EscapeHtml(message.Subject)).AppendLine("</p>")
            .Append("<hr />")
            .AppendLine(message.HtmlBody)
            .AppendLine("</body></html>")
            .ToString();

        await File.WriteAllTextAsync(filePath, content, cancellationToken);
        logger.LogInformation("Email saved to {FilePath}", filePath);
    }

    private static string SanitizeFileNameSegment(string value)
    {
        var invalid = Path.GetInvalidFileNameChars();
        var builder = new StringBuilder(value.Length);
        foreach (var character in value)
        {
            builder.Append(invalid.Contains(character) ? '_' : character);
        }

        var sanitized = builder.ToString().Trim('_');
        return sanitized.Length <= 60 ? sanitized : sanitized[..60];
    }

    private static string EscapeHtml(string value) =>
        value
            .Replace("&", "&amp;", StringComparison.Ordinal)
            .Replace("<", "&lt;", StringComparison.Ordinal)
            .Replace(">", "&gt;", StringComparison.Ordinal);
}
