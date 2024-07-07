using Ardalis.Specification;

namespace notifgram.Core.PostAggregate.Specifications;

public class PostByIdSpec : Specification<Post>
{
  public PostByIdSpec(int postId)
  {
    Query
        .Where(post => post.Id == postId);
  }


}
