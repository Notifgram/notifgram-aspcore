using FirebaseAdmin.Messaging;
using notifgram.Core;
using notifgram.Core.ApiModels;

namespace notifgram.Infrastructure.Api;

public abstract class MessageBuilderBase
{

  /// <summary>
  /// Creates an FCM message which can be used to notify clients to start syncing usng Restful API.
  /// </summary>
  /// <returns></returns>
  public static Message MakeStartSyncMessage()
  {
    var message = new Message()
    {
      Topic = FcmTopics.RESTFUL_SYNC_REQUEST_TOPIC.ToString()
    };
    return message;
  }

}
