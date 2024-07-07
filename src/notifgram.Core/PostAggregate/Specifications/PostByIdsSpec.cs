using Ardalis.Specification;

namespace notifgram.Core.PostAggregate.Specifications;
public class PostByIdsSpec : Specification<Post>
{
  public PostByIdsSpec(List<int> postIds)
  {
    Query
        .Where(post => postIds.Contains(post.Id));
  }
}
