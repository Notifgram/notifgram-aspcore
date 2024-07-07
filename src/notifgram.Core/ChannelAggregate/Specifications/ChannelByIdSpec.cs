using Ardalis.Specification;

namespace notifgram.Core.ChannelAggregate.Specifications;

public class ChannelByIdSpec : Specification<Channel>
{
  public ChannelByIdSpec(int channelId)
  {
    Query
        .Where(channel => channel.Id == channelId);
  }
}
