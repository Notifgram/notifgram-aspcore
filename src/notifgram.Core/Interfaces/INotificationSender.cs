using FirebaseAdmin.Messaging;

namespace notifgram.Core.Interfaces;
public interface INotificationSender
{
  Task<string> SendNotificationAsync(Message message);
  Task<BatchResponse?> SendAllNotificationsAsync(List<Message> messages);
  Task<BatchResponse> SendAllNotificationAsync(List<Message> messages);
}
