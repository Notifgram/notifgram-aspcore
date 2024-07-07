using Ardalis.Specification;
using notifgram.Core.ChannelAggregate;

namespace notifgram.Core.PostAggregate.Specifications;
public class ChannelByIdsSpec : Specification<Channel>
{
  public ChannelByIdsSpec(List<int> channelIds)
  {
    Query
        .Where(channel => channelIds.Contains(channel.Id));
  }
}
