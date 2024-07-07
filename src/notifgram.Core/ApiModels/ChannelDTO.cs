using System.Text.Json;
using notifgram.Core.ChannelAggregate;

namespace notifgram.Core.ApiModels;

public class ChannelDTO : CreateChannelDTO
{
  public ChannelDTO(int id, string name,string description, string lastUpdate, bool isEnable)
    : base(  name,  description,  lastUpdate)
  {
    Id = id;
    LastUpdate = lastUpdate;
    IsEnable = isEnable;
  }

  public int Id { get; set; }
  public bool IsEnable { get; set; }
  public string LastUpdate { get; set; }

  public override Channel toChannel()
  {
    var channel = base.toChannel();
    channel.LastUpdate = DateTime.ParseExact( LastUpdate, Constants.LastUpdateDateTimeFormat, null);  // is equal to 0 in CreateChannelDTO
    return channel;
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

public class CreateChannelDTO
{
  public CreateChannelDTO(string name, string description, string lastUpdate)
  {
    Name = name;
    Description = description;
  }

  public string Name { get; set; }
  public string Description { get; set; } = string.Empty;

  public virtual Channel toChannel() =>
     new Channel
     {
       Name = this.Name,
       Description = this.Description,
       IsEnable = true
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


public static class ChannelMapper
{
  public static ChannelDTO toChannelDTO(this Channel channel) => new ChannelDTO(
    id: channel.Id,
    name: channel.Name,
    description: channel.Description,
    isEnable: channel.IsEnable,
    lastUpdate: channel.LastUpdate.ToString(Constants.LastUpdateDateTimeFormat)
  );
}

//TODO: is not used
//public static class DateTimeMapper
//{
//  public static string toLastUpdateString(this DateTime dateTime) =>
//    dateTime.ToString(Constants.LastUpdateDateTimeFormat);
//}
