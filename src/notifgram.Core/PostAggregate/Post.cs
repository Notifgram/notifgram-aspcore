using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using notifgram.SharedKernel.Interfaces;

namespace notifgram.Core.PostAggregate;
public class Post : IAggregateRoot
{

  public int Id { get; set; }
  public int ChannelId { get; set; }
  public bool IsEnable { get; set; }
  public string Text { get; set; } = string.Empty;
  public string FileName { get; set; } = string.Empty;
  public MediaTypes MediaType { get; set; } = MediaTypes.NO_MEDIA;

  [Timestamp]
  public byte[]? RowVersion { get; set; }
  public DateTime LastUpdate { get; set; }


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

public enum MediaTypes
{
  AUDIO = 1, VIDEO = 2, NO_MEDIA = 0, IMAGE = -1
}
