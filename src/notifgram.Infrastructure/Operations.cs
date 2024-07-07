using FirebaseAdmin.Messaging;
using notifgram.Core.Interfaces;
using notifgram.Core.ApiModels;

namespace notifgram.Infrastructure.Api;

public class Operations
{

  public static async Task<BatchResponse?> SendFcmMessages(ChannelDTO channelDTO, INotificationSender _notificationSender)
  {
    var messages = new List<Message>();

    // Ask clients to start a sync
    Message startSyncMessage = ChannelFcmMessageBuilder.MakeStartSyncMessage();
    messages.Add(startSyncMessage);

    if (channelDTO.IsEnable)
    {
      // Create Channel Message to be sent to clients through FCM
      Message newChannelFcmMessage = ChannelFcmMessageBuilder.makeChannelFcmMessage(channelDTO);
      messages.Add(newChannelFcmMessage);
    }
    else
    {
      Message disabledChannelMessage = ChannelFcmMessageBuilder.makeChannelDisabledFcmMessage(channelDTO.Id);
      messages.Add(disabledChannelMessage);
    }
    var response = await _notificationSender.SendAllNotificationsAsync(messages);
    return response;
  }


  public static async Task<BatchResponse?> SendFcmMessages(PostDTO postDTO, INotificationSender _notificationSender)
  {
    var messages = new List<Message>();

    // Ask clients to start a sync
    Message startSyncMessage = PostFcmMessageBuilder.MakeStartSyncMessage();
    messages.Add(startSyncMessage);

    if (postDTO.IsEnable)
    {
      // Create Post Message to be sent to clients through FCM
      Message newPostFcmMessage = PostFcmMessageBuilder.makePostFcmMessage(postDTO);
      messages.Add(newPostFcmMessage);
    }
    else
    {
      Message disabledPostMessage = PostFcmMessageBuilder.makePostDisabledFcmMessage(postDTO.Id);
      messages.Add(disabledPostMessage);
    }
    var response = await _notificationSender.SendAllNotificationsAsync(messages);
    return response;
  }

  public static async Task<BatchResponse?> SendFcmMessages(List<ChannelDTO> channelDTOs, string receipientFcmToken, INotificationSender _notificationSender)
  {
    var messages = new List<Message>();

    Message startSyncMessage = ChannelFcmMessageBuilder.AllChannelsFcmMessage(channelDTOs, receipientFcmToken);
    messages.Add(startSyncMessage);

    var response = await _notificationSender.SendAllNotificationsAsync(messages);
    return response;
  }

}
