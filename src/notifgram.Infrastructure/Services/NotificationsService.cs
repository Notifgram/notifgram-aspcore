using FirebaseAdmin.Messaging;
using notifgram.Core.Interfaces;
using Microsoft.Extensions.Logging;

namespace notifgram.Infrastructure;

public interface INotificationsService
{
  Task<string> SendNotification();
}
public class NotificationsService : INotificationsService
{
  private readonly ILogger<NotificationsService> _logger;
  private readonly INotificationSender _notificationSender;

  public NotificationsService(ILogger<NotificationsService> logger, INotificationSender notificationSender)
  {
    _logger = logger;
    _notificationSender = notificationSender;
  }

  public async Task<string> SendNotification()
  {
    var message = new Message()
    {
      Notification = new Notification
      {
        Title = "Notif title",
        Body = "Notif body"
      },

      //Token = "",
      Topic = ""
    };

    return await _notificationSender.SendNotificationAsync(message);
  }

}
