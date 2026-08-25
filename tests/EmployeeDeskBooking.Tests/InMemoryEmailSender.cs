using EmployeeDeskBooking.Application.Notifications;

namespace EmployeeDeskBooking.Tests;

public sealed class InMemoryEmailSender : IEmailSender
{
    public List<EmailMessage> Sent { get; } = [];

    public bool FailNext { get; set; }

    public Exception? NextException { get; set; }

    public Task SendAsync(EmailMessage message, CancellationToken cancellationToken = default)
    {
        if (FailNext || NextException is not null)
        {
            FailNext = false;
            throw NextException ?? new InvalidOperationException("Simulated SMTP failure");
        }

        Sent.Add(message);
        return Task.CompletedTask;
    }

    public void Reset()
    {
        Sent.Clear();
        FailNext = false;
        NextException = null;
    }
}
