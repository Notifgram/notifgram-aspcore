using System.Linq.Expressions;
using System.Reflection;
using System.Reflection.Metadata;
using Ardalis.Specification;

namespace notifgram.Core.PostAggregate.Specifications;

public class ChangedPostsSpec : Specification<Post>
{
  public ChangedPostsSpec(DateTime after)
  {
    Query.Where(post => DateTime.Compare(post.LastUpdate, after) > 0);
  }

}
