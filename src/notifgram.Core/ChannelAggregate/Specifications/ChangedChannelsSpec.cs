using System.Linq.Expressions;
using System.Reflection;
using System.Reflection.Metadata;
using Ardalis.Specification;
using notifgram.Core.ChannelAggregate;

namespace notifgram.Core.ChannelAggregate.Specifications;

public class ChangedChannelsSpec : Specification<Channel>
{
  public ChangedChannelsSpec(DateTime after)
  {
    //Query.Where(channel => channel.LastUpdate > after);
    Query.Where(channel => DateTime.Compare(channel.LastUpdate, after) > 0);
  }

}
