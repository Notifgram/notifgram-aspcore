using notifgram.Core.Interfaces;
using Microsoft.Extensions.Logging;

namespace notifgram.Infrastructure;

public class FakeEmailSender(ILogger<FakeEmailSender> logger) : IEmailSender
{
  private readonly ILogger<FakeEmailSender> _logger = logger;

  public Task SendEmailAsync(string to,  string subject, string body)
  {
    _logger.LogInformation("Not actually sending an email to {to} with subject {subject}", to, subject);
    return Task.CompletedTask;
  }
}
