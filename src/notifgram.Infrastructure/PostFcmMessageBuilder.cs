using FirebaseAdmin.Messaging;
using notifgram.Core;
using notifgram.Core.ApiModels;
using Newtonsoft.Json;

namespace notifgram.Infrastructure.Api;

public class PostFcmMessageBuilder : MessageBuilderBase
{

  /// <summary>
  /// Creates an FCM message containing Post object.
  /// </summary>
  /// <param name="postDTO"></param>
  /// <returns></returns>
  public static Message makePostFcmMessage(PostDTO postDTO)
  {
    //var messages = new List<Message>();

    var postJson = JsonConvert.SerializeObject(postDTO);
    var data = new Dictionary<string, string>
            {
                { FcmKeys.JSON_POST.ToString(), postJson },
            };
    var postFcmMessage = new Message()
    {
      Data = data,
      Topic = FcmTopics.FCM_SYNC_TOPIC.ToString()
    };

    return postFcmMessage;
  }

  /// <summary>
  /// Creates an FCM message indicating a post is disabled
  /// </summary>
  /// <param name="id">id of the deleted post object</param>
  /// <returns></returns>
  public static Message makePostDisabledFcmMessage(int id)
  {
    var data = new Dictionary<string, string>
            {
                { FcmKeys.DELETED_POST.ToString(), id.ToString() },
            };
    var message = new Message()
    {
      Data = data,
      Topic = FcmTopics.FCM_SYNC_TOPIC.ToString()
    };
    return message;

  }

}
