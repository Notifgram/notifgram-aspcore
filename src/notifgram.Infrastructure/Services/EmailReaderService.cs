using MailKit.Net.Imap;
using MailKit.Search;
using MailKit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MailKit.Security;

namespace notifgram.Infrastructure.Services;

public class EmailReaderService : BackgroundService
{
  private const string TAG = "EmailReaderService";
  private readonly ILogger<EmailReaderService> _logger;
  private readonly IConfiguration _configuration;
  private readonly IEmailRequestsHandler _emailRequestsHandler;
  private readonly string _imapServer;
  private readonly int _imapPort;
  private readonly string _imapUsername;
  private readonly string _imapPassword;

  public EmailReaderService(IConfiguration configuration,
    ILogger<EmailReaderService> logger,
    IEmailRequestsHandler emailRequestsHandler)
  {
    _logger = logger;
    _configuration = configuration;
    var emailConfig = _configuration.GetSection("EmailConfiguration");
    _imapServer = emailConfig["ImapServer"] ?? string.Empty;
    _imapPort = int.Parse(emailConfig["ImapPort"] ?? "0");
    _imapUsername = emailConfig["ImapUsername"] ?? string.Empty;
    _imapPassword = emailConfig["ImapPassword"] ?? string.Empty;
    _emailRequestsHandler = emailRequestsHandler;
  }

  protected override async Task ExecuteAsync(CancellationToken stoppingToken)
  {
    while (!stoppingToken.IsCancellationRequested)
    {
      using var client = new ImapClient();
      _logger.LogInformation($"{TAG} ExecuteAsync() _imapServer={_imapServer} _imapPort={_imapPort}");

      try
      {
        await client.ConnectAsync(_imapServer, _imapPort, SecureSocketOptions.Auto);
        _logger.LogInformation($"{TAG} ExecuteAsync() after ConnectAsync() ");

        await client.AuthenticateAsync(_imapUsername, _imapPassword);
        _logger.LogInformation($"{TAG} ExecuteAsync() after AuthenticateAsync()");

        var inbox = client.Inbox;
        await inbox.OpenAsync(FolderAccess.ReadWrite);
        _logger.LogInformation($"{TAG} ExecuteAsync() after OpenAsync()");

        while (!stoppingToken.IsCancellationRequested)
        {
          var unreadQuery = SearchQuery.NotSeen;
          var flaggedQuery = SearchQuery.Flagged;
          var combinedQuery = SearchQuery.And(unreadQuery, flaggedQuery);
          var uids = await inbox.SearchAsync(combinedQuery);
          _logger.LogInformation($"{TAG} ExecuteAsync() uids={uids}");

          foreach (var uid in uids)
          {
            var message = await inbox.GetMessageAsync(uid);
            // Set the 'Seen' flag
            client.Inbox.AddFlags(uid, MessageFlags.Seen, true);

            //// deletes the email for security reasons.
            //client.Inbox.AddFlags(uid, MessageFlags.Deleted,true);
            ////permanently remove all messages that have been marked for deletion.
            //await inbox.ExpungeAsync();

            _emailRequestsHandler.ProcessEmailSubject(message.Subject, message.From.Mailboxes.First().Address);
          }

          // Wait for a while before checking for new messages again
          await Task.Delay(TimeSpan.FromSeconds(10), stoppingToken);
        }
      }
      catch(Exception ex)
      {
        _logger.LogError($"{TAG} ExecuteAsync() ex={ex}");

      }
      await Task.Delay(TimeSpan.FromSeconds(10), stoppingToken);

      //await client.DisconnectAsync(true);
    }
  }


}
