using FirebaseAdmin.Messaging;
using notifgram.Core.Interfaces;
using Microsoft.Extensions.Logging;

namespace notifgram.Infrastructure;
public class NotificationSender : INotificationSender
{
  private readonly ILogger<NotificationSender> _logger;

  public NotificationSender(ILogger<NotificationSender> logger)
  {
    _logger = logger;
  }

  public async Task<string> SendNotificationAsync(Message message)
  {
    _logger.LogInformation($"SendNotificationAsync() message={message.ToString()}");
    var messaging = FirebaseMessaging.DefaultInstance;
    var result = await messaging.SendAsync(message);

    _logger.LogInformation(result);
    return result;
  }

  // TODO: merge this method with SendAllNotificationAsync. this one is calling SendAllNotificationAsync.
  public async Task<BatchResponse?> SendAllNotificationsAsync(List<Message> messages)
  {
    BatchResponse? response = null;

    try
    {
      response = await SendAllNotificationAsync(messages);
      //return Ok(response);
    }
    catch (FirebaseMessagingException ex)
    {
      _logger.LogInformation($"Error sending FCM message: {ex.Message}");
      //return StatusCode(500, $"Error sending FCM message: {ex.Message}");
    }
    return response;
  }

  public async Task<BatchResponse> SendAllNotificationAsync(List<Message> messages)
  {
    var messaging = FirebaseMessaging.DefaultInstance;
    // Send the messages
    BatchResponse response;

    response = await messaging.SendEachAsync(messages);
    _logger.LogInformation(response.Responses.ToString());
    return response;
  }

}
