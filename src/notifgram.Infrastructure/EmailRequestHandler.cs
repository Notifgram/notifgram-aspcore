using FirebaseAdmin.Messaging;
using notifgram.Core.ChannelAggregate;
using notifgram.Core.Interfaces;
using notifgram.Infrastructure.Api;
using notifgram.Infrastructure.Services;
using notifgram.SharedKernel.Interfaces;
using Microsoft.Extensions.Logging;
using notifgram.Core.ApiModels;
using notifgram.Core.helpers;

namespace notifgram.Infrastructure;
public class EmailRequestsHandler(IRepository<Channel> repository,
   ILogger<EmailRequestsHandler> logger, INotificationSender notificationSender,
   IEmailSender smtpEmailSender, IFcmTokenVerifier fcmTokenVerifier) : IEmailRequestsHandler
{
  private const string TAG = "EmailRequestsHandler";
  private readonly ILogger<EmailRequestsHandler> _logger = logger;
  private readonly IRepository<Channel> _repository = repository;
  private readonly INotificationSender _notificationSender = notificationSender;
  private readonly IEmailSender _smtpEmailSender = smtpEmailSender;
  private readonly IFcmTokenVerifier _fcmTokenVerifier = fcmTokenVerifier;

  public async void ProcessEmailSubject(string subject, string from)
  {

    _logger.LogInformation($"{TAG} ProcessEmailSubject() New email received. subject: {subject}");
    var requestParts = subject.Split(".");

    if (requestParts.Length != 2)
    {
      _logger.LogInformation($"{TAG} ProcessEmailSubject() subject is not valid. request parts count is not 2");
      informUserOnInvalidEmailSubject(from);
      return;
    }

    string command = requestParts[0].ToLower();
    _logger.LogInformation($"{TAG} ProcessEmailSubject() command={command}");
    string fcmToken = requestParts[1];
    _logger.LogInformation($"{TAG} ProcessEmailSubject() fcmToken={fcmToken}");

    if (command is null ||
      command.Trim().Equals(String.Empty) ||
      fcmToken is null ||
      fcmToken.Trim().Equals(String.Empty) ||
      await _fcmTokenVerifier.IsValidTokenAsync(fcmToken)
      )
    {
      _logger.LogInformation($"{TAG} ProcessEmailSubject() subject is not valid.");
      informUserOnInvalidEmailSubject(from);
      return;
    }

    switch (command)
    {
      case "channels": { handleChannelsRequestEmail(fcmToken); break; };
      default: return;
    }
  }

  private async void handleChannelsRequestEmail(string fcmToken)
  {
    // TODO: send only enabled channels.
    // TODO: send only after lastchange.
    var channelDTOs = (await _repository.ListAsync())
                     .Select(channel => channel.toChannelDTO())
                     .ToList();

    _logger.LogInformation($"{TAG} handleChannelsRequestEmail() channelDTOs={channelDTOs.Dump()}");
    BatchResponse? fcmResponse = await Operations.SendFcmMessages(channelDTOs, fcmToken, _notificationSender);
    _logger.LogInformation($"{TAG} handleChannelsRequestEmail() fcmResponse={fcmResponse?.Responses.Dump()}");
  }

  private async void informUserOnInvalidEmailSubject(string to)
  {
    _logger.LogInformation($"{TAG} informUserOnInvalidEmailSubject() to={to}");
    await _smtpEmailSender.SendEmailAsync(to, "invalid request", "the subject of your email is invalid.");
  }
}
