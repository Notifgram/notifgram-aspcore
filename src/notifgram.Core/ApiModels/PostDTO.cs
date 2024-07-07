using System.Text.Json;
using notifgram.Core.PostAggregate;

namespace notifgram.Core.ApiModels;
public class PostDTO : CreatePostDTO
{
  public PostDTO(int id, string lastUpdate, int channelId, string fileName, string text, int mediaType, bool isEnable)
    : base(channelId, fileName, text, mediaType)
  {
    Id = id;
    LastUpdate = lastUpdate;
    IsEnable = isEnable;
  }

  public int Id { get; set; }
  public bool IsEnable { get; set; }
  public string LastUpdate { get; set; }

  public override Post toPost()
  {
    var post = base.toPost();
    post.LastUpdate = DateTime.ParseExact(LastUpdate, Constants.LastUpdateDateTimeFormat, null);  // is equal to 0 in CreatePostDTO
    return post;
  }

  public override string ToString()
  {
    try
    {
      return JsonSerializer.Serialize(this);
    }
    catch (Exception)
    {
      return string.Empty;
    }
  }

}

public class CreatePostDTO
{
  public CreatePostDTO(int channelId, string fileName, string text, int mediaType)
  {
    ChannelId = channelId;
    FileName = fileName;
    Text = text;
    MediaType = mediaType;
  }

  public int ChannelId { get; set; }
  public string FileName { get; set; } = string.Empty;
  public string Text { get; set; } = string.Empty;
  public int MediaType { get; set; }

  //public virtual int LastUpdate { get; set; } = 0;

  public virtual Post toPost() =>
     new Post
     {
       FileName = this.FileName,
       Text = this.Text,
       MediaType = this.MediaType.toMediaTypes(),
       ChannelId = this.ChannelId,
       IsEnable=true

       //LastUpdate = DateTime.UtcNow
     };

  public override string ToString()
  {
    try
    {
      return JsonSerializer.Serialize(this);
    }
    catch (Exception)
    {
      return string.Empty;
    }
  }
}


public static class PostMapper
{
  public static PostDTO toPostDTO(this Post post) => new PostDTO(
    id: post.Id,
    channelId: post.ChannelId,
    text: post.Text,
    fileName: post.FileName,
    mediaType: post.MediaType.toInt(),
    isEnable: post.IsEnable,
    lastUpdate: post.LastUpdate.ToString(Constants.LastUpdateDateTimeFormat)
  );
}

public static class MediaTypeMapper
{
  public static int toInt(this MediaTypes mediaType)
  {
    switch (mediaType)
    {
      case MediaTypes.AUDIO: return 1;
      case MediaTypes.VIDEO: return 2;
      case MediaTypes.IMAGE: return -1;
      case MediaTypes.NO_MEDIA: return 0;
      default: return 0;
    };
  }

  public static MediaTypes toMediaTypes(this int mediaType)
  {
    switch (mediaType)
    {
      case 1: return MediaTypes.AUDIO;
      case 2: return MediaTypes.VIDEO;
      case -1: return MediaTypes.IMAGE;
      case 0: return MediaTypes.NO_MEDIA;
      default: return 0;
    };
  }

}
