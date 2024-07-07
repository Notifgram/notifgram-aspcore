using MailKit.Net.Smtp;
using MimeKit;
using notifgram.Core.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;

namespace notifgram.Infrastructure;

public class SmtpEmailSender : IEmailSender
{
  private const string TAG = "SmtpEmailSender";

  private readonly ILogger<SmtpEmailSender> _logger;
  private readonly IConfiguration _configuration;
  private readonly string _server;
  private readonly int _port;
  private readonly string _username;
  private readonly string _password;

  public SmtpEmailSender(IConfiguration configuration, ILogger<SmtpEmailSender> logger)
  {
    _logger = logger;
    _configuration = configuration;
    var emailConfig = _configuration.GetSection("SmtpEmailConfiguration");
    _server = emailConfig["Server"] ?? string.Empty;
    _port = int.Parse(emailConfig["Port"] ?? "0");
    _username = emailConfig["Username"] ?? string.Empty;
    _password = emailConfig["Password"] ?? string.Empty;
  }

  public async Task SendEmailAsync(string email, string subject, string message)
  {
    _logger.LogInformation($"{TAG} SendEmailAsync() email={email} subject={subject} message={message}");

    var emailMessage = new MimeMessage();

    emailMessage.From.Add(new MailboxAddress("Your Name", _username));
    emailMessage.To.Add(new MailboxAddress("", email));
    emailMessage.Subject = subject;
    emailMessage.Body = new TextPart(MimeKit.Text.TextFormat.Html)
    {
      Text = message
    };

    using var client = new SmtpClient();
    await client.ConnectAsync(_server, _port, false);
    await client.AuthenticateAsync(_username, _password);
    await client.SendAsync(emailMessage);

    await client.DisconnectAsync(true);
  }

}
