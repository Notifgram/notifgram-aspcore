using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using notifgram.SharedKernel.Interfaces;

namespace notifgram.Core.ChannelAggregate;
public class Channel : IAggregateRoot
{
  public int Id { get; set; }
  public string Name { get; set; } = string.Empty;
  public string Description { get; set; } = string.Empty;
  public bool IsEnable { get; set; }

  // For relationships
  //public ICollection<Post>? Posts { get; set; }

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
