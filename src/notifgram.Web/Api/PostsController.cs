using notifgram.Core.LastChangeAggregate;
using notifgram.SharedKernel.Interfaces;
using Microsoft.AspNetCore.Mvc;
using notifgram.Core.Interfaces;
using FirebaseAdmin.Messaging;
using notifgram.Core.ApiModels;
using notifgram.Core.PostAggregate.Specifications;
using notifgram.Core.PostAggregate;
using notifgram.Core;
using notifgram.Core.helpers;
using notifgram.Infrastructure.Api;

namespace notifgram.Web.Api;

public class PostsController : BaseApiController
{
  private const string TAG = "PostsController";
  private readonly IRepository<Post> _repository;
  private readonly ILogger<PostsController> _logger;
  private readonly INotificationSender _notificationSender;

  public PostsController(IRepository<Post> repository,
    ILogger<PostsController> logger,
  INotificationSender notificationSender)
  {
    _repository = repository;
    _logger = logger;
    _notificationSender = notificationSender;
  }


  //[HttpGet(Name ="changelist/{after}")]
  [HttpGet("changelist")]
  public async Task<IActionResult> getChangedItems([FromQuery] string? after)
  {
    _logger.LogInformation($"{TAG} getChangedItems() parameter: after={after}");

    var recentChanges = new List<Post>();

    // if it is the first fetch
    if (after == null || after.Equals(System.String.Empty))
      recentChanges = await _repository.ListAsync();
    else
    {
      var dateTime = DateTime.ParseExact(after, Constants.LastUpdateDateTimeFormat, null);
      _logger.LogInformation($"{TAG} getChangedItems() dateTime after parsing={dateTime}");

      var spec = new ChangedPostsSpec(dateTime);
      // returns list of ids of changed items since "after"
      recentChanges = await _repository.ListAsync(spec);
    }

    List<LastChange> result = recentChanges.Select(
      post => new LastChange(
        id: post.Id,
        changeListVersion: post.LastUpdate.ToString(Constants.LastUpdateDateTimeFormat),
        isDelete: !post.IsEnable)
      ).ToList();

    _logger.LogInformation($"{TAG} getChangedItems() end result={result.Dump()}");
    return Ok(result);
  }

  //[HttpGet("{after:int}")]
  //public async Task<IActionResult> getChangedItems(byte[] after)
  //{
  //  _logger.LogInformation($"{TAG} changes() after={after}");
  //  var changedItems = (await _repository.ListAsync())
  //     .Select(Post => Post.RowVersion > after).toList();

  //  _logger.LogInformation($"{TAG} List() result={PostDTOs}");
  //  return Ok(changedItems);
  //  //await _repository.SaveChangesAsync();


  //}



  // GET: api/Posts
  [HttpGet]
  public async Task<IActionResult> List()
  {
    _logger.LogInformation($"{TAG} List()");
    var postDTOs = (await _repository.ListAsync())
                      .Select(post => post.toPostDTO())
                      .ToList();

    _logger.LogInformation($"{TAG} List() result={postDTOs.Dump()}");
    return Ok(postDTOs);
  }

  /// <summary>
  /// </summary>
  /// <param name="id"></param>
  /// <returns></returns>
  // GET: api/Posts
  [HttpGet("{id:int}")]
  public async Task<IActionResult> GetById(int id)
  {
    _logger.LogInformation($"{TAG} GetById() id={id}");
    var post = await _repository.GetByIdAsync(id);
    _logger.LogInformation($"{TAG} GetById() post={post}");

    if (post == null)
      return NotFound();

    return Ok(post.toPostDTO()); 
  }

  /// <summary>
  /// </summary>
  /// <param name="ids">List of ids of the requested posts</param>
  /// <returns>List of posts associated to the requested ids</returns>
  // GET: api/Posts
  //[HttpGet("{ids:List<int>}")]
  [HttpGet("GetByIds")]
  public async Task<IActionResult> GetByIds([FromQuery] List<int> ids)
  {
    _logger.LogInformation($"{TAG} GetByIds() ids={ids.Dump()}");
    var spec = new PostByIdsSpec(ids);
    var posts = await _repository.ListAsync(spec);
    _logger.LogInformation($"{TAG} GetByIds() posts={posts.Dump()}");

    if (posts == null)
      return NotFound();

    var postDTOs = posts
                     .Select(post => post.toPostDTO())
                     .ToList();

    _logger.LogInformation($"{TAG} GetByIds() postDTOs={postDTOs.Dump()}");

    return Ok(postDTOs); 
  }

  // POST: api/Posts
  /// <summary>
  /// Creates a single Post and notifies the clients about the new post creation.
  /// </summary>
  /// <param name="request"></param>
  /// <returns>A newly created Post</returns>
  /// <remarks>
  /// Sample request:
  ///
  /// POST /Post
  ///    {
  ///       {
  ///         "channelId": 0,
  ///         "fileName": "string",
  ///         "text": "string",
  ///         "mediaType": 0
  ///       }
  ///    }
  /// </remarks>
  /// <response code="201">Returns the newly created item</response>
  /// <response code="400">If the item is null</response>
  [HttpPost]
  public async Task<IActionResult> Post([FromBody] CreatePostDTO request)
  {
    _logger.LogInformation($"{TAG} post() request={request}");

    Post post = request.toPost();

    if (post.MediaType != MediaTypes.NO_MEDIA && request.FileName == null)
      return BadRequest();

    Post createdPost = await _repository.AddAsync(post);
    PostDTO result = createdPost.toPostDTO();

    BatchResponse? fcmResponse = await Operations.SendFcmMessages(result, _notificationSender);

    return Created(
      string.Empty,
      new
      {
        result,
        fcmResponse
      }
    );

  }



  /// <summary>
  /// Deletes all Posts.
  /// </summary>
  /// <returns>Count of deleted Posts</returns>
  // DELETE: api/Posts
  [HttpDelete]
  public async Task<IActionResult> Delete()
  {
    _logger.LogInformation($"{TAG}  Delete()");
    var allRows = await _repository.ListAsync();
    await _repository.DeleteRangeAsync(allRows);

    //return NoContent();
    return Ok(allRows.Count);
  }

  // DELETE: api/Posts
  [HttpDelete("{id:int}")]
  public async Task<IActionResult> DeleteById(int id)
  {
    _logger.LogInformation($"{TAG}  DeleteById() id={id}");

    var Post = await _repository.GetByIdAsync(id);
    if (Post == null)
      return NotFound();

    await _repository.DeleteAsync(Post);

    return NoContent();
  }

  // PATCH: api/Posts
  [HttpPatch("{id:int}")]
  public async Task<IActionResult> DisablePost(int id)
  {
    _logger.LogInformation($"{TAG} DisablePost() id={id}");

    Post? post = await _repository.GetByIdAsync(id);
    _logger.LogInformation($"{TAG} DisablePost() post={post}");

    if (post == null)
      return NotFound();

    post.IsEnable = false;
    await _repository.UpdateAsync(post);

    Post? modifiedPost = await _repository.GetByIdAsync(id);
    PostDTO result = modifiedPost!.toPostDTO();
    var fcmResponse = await Operations.SendFcmMessages(result, _notificationSender);

    return Ok(
      new
      {
        result,
        fcmResponse
      }
    );
    //return NoContent() ; 
  }


  // PUT: api/Posts/5
  // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
  [HttpPut("{id}")]
  public async Task<IActionResult> PutPost(int id, CreatePostDTO request)
  {
    _logger.LogInformation($"{TAG} PutPost() id={id} request={request}");

    Post post = request.toPost();
    if (post is null )
      return BadRequest();

    await _repository.UpdateAsync(post);

    var resultPost = await _repository.GetByIdAsync(id);
    if (resultPost == null)
      return NotFound();

    PostDTO createdPost = resultPost.toPostDTO();
    var fcmResponse = await Operations.SendFcmMessages(createdPost, _notificationSender);

    return Ok(
        new
        {
          createdPost,
          fcmResponse
        }
    );
    //return NoContent();
  }

  private async Task<bool> PostExists(int id)
  {
    var spec = new PostByIdSpec(id);
    return await _repository.AnyAsync(spec);
  }

}
