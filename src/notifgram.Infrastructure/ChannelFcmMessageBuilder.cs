using FirebaseAdmin.Messaging;
using notifgram.Core;
using notifgram.Core.ApiModels;
using Newtonsoft.Json;

namespace notifgram.Infrastructure.Api;

public class ChannelFcmMessageBuilder : MessageBuilderBase
{

  /// <summary>
  /// Creates an FCM message containing a json object of all channels.
  /// </summary>
  /// <param name="channelDTOs"></param>
  /// <param name="receipientFcmToken"></param>
  /// <returns></returns>
  public static Message AllChannelsFcmMessage(List<ChannelDTO> channelDTOs,string receipientFcmToken)
  {
    var channelsJson = JsonConvert.SerializeObject(channelDTOs);
    var data = new Dictionary<string, string>
    {
        { FcmKeys.JSON_ALL_CHANNEL.ToString(), channelsJson },
    };
    var channelsFcmMessage = new Message()
    {
      Data = data,
      Token= receipientFcmToken
    };
    return channelsFcmMessage;
  }

  /// <summary>
  /// Creates an FCM message containing Channel object.
  /// </summary>
  /// <param name="channelDTO"></param>
  /// <returns></returns>
  public static Message makeChannelFcmMessage(ChannelDTO channelDTO)
  {
    //var messages = new List<Message>();

    var channelJson = JsonConvert.SerializeObject(channelDTO);
    var data = new Dictionary<string, string>
    {
        { FcmKeys.JSON_CHANNEL.ToString(), channelJson },
    };
    var channelFcmMessage = new Message()
    {
      Data = data,
      Topic = FcmTopics.FCM_SYNC_TOPIC.ToString()
    };

    return channelFcmMessage;
  }

  /// <summary>
  /// Creates an FCM message indicating a channel is disabled
  /// </summary>
  /// <param name="id">id of the deleted channel object</param>
  /// <returns></returns>
  public static Message makeChannelDisabledFcmMessage(int id)
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
