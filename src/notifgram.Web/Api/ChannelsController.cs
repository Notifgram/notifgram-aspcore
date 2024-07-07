using notifgram.Core;
using notifgram.Core.ChannelAggregate;
using notifgram.Core.ChannelAggregate.Specifications;
using notifgram.Core.helpers;
using notifgram.Core.LastChangeAggregate;
using notifgram.SharedKernel.Interfaces;
using notifgram.Core.ApiModels;
using Microsoft.AspNetCore.Mvc;
using notifgram.Core.Interfaces;
using notifgram.Core.PostAggregate.Specifications;
using FirebaseAdmin.Messaging;
using notifgram.Infrastructure.Api;

namespace notifgram.Web.Api;

public class ChannelsController : BaseApiController
{
  private const string TAG = "ChannelsController";
  private readonly IRepository<Channel> _repository;
  private readonly ILogger<ChannelsController> _logger;
  private readonly INotificationSender _notificationSender;

  public ChannelsController(IRepository<Channel> repository,
    ILogger<ChannelsController> logger,
     INotificationSender notificationSender)
  {
    _repository = repository;
    _logger = logger;
    _notificationSender = notificationSender;
  }


  [HttpGet("changelist")]
  public async Task<IActionResult> getChangedItems([FromQuery] string? after)
  {
    _logger.LogInformation($"{TAG} getChangedItems() parameter: after={after}");

    var recentChanges = new List<Channel>();

    // if it is the first fetch
    if (after == null || after.Equals(String.Empty))
      recentChanges = await _repository.ListAsync();
    else
    {
      var dateTime = DateTime.ParseExact(after, Constants.LastUpdateDateTimeFormat, null);
      _logger.LogInformation($"{TAG} getChangedItems() dateTime after parsing={dateTime}");

      var spec = new ChangedChannelsSpec(dateTime);
      // returns list of ids of changed items since "after"
      recentChanges = await _repository.ListAsync(spec);
    }

    List<LastChange> result = recentChanges.Select(
      channel => new LastChange(
        id: channel.Id,
        changeListVersion: channel.LastUpdate.ToString(Constants.LastUpdateDateTimeFormat),
        isDelete: !channel.IsEnable)
      ).ToList();

    _logger.LogInformation($"{TAG} getChangedItems() end result={result.Dump()}");
    return Ok(result);
  }

  // GET: api/Channels
  [HttpGet]
  public async Task<IActionResult> List()
  {
    _logger.LogInformation($"{TAG} List()");
    var channelDTOs = (await _repository.ListAsync())
                      .Select(channel => channel.toChannelDTO())
                      .ToList();

    _logger.LogInformation($"{TAG} List() result={channelDTOs.Dump()}");
    return Ok(channelDTOs);
  }

  /// <summary>
  /// </summary>
  /// <param name="id"></param>
  /// <returns></returns>
  // GET: api/Channels
  [HttpGet("{id:int}")]
  public async Task<IActionResult> GetById(int id)
  {
    _logger.LogInformation($"{TAG} GetById() id={id}");
    var channel = await _repository.GetByIdAsync(id);
    _logger.LogInformation($"{TAG} GetById() channel={channel}");

    if (channel == null)
      return NotFound();

    return Ok(channel.toChannelDTO());
  }

  /// <summary>
  /// </summary>
  /// <param name="ids">List of ids of the requested channels</param>
  /// <returns>List of channels associated to the requested ids</returns>
  // GET: api/Channels
  [HttpGet("GetByIds")]
  public async Task<IActionResult> GetByIds([FromQuery] List<int> ids)
  {
    _logger.LogInformation($"{TAG} GetByIds() ids={ids.Dump()}");
    var spec = new ChannelByIdsSpec(ids);
    var channels = await _repository.ListAsync(spec);
    _logger.LogInformation($"{TAG} GetByIds() channels={channels.Dump()}");

    if (channels == null)
      return NotFound();

    var channelDTOs = channels
                     .Select(channel => channel.toChannelDTO())
                     .ToList();

    _logger.LogInformation($"{TAG} GetByIds() channelDTOs={channelDTOs.Dump()}");

    return Ok(channelDTOs);
  }

  // POST: api/channels
  /// <summary>
  /// Creates a single Channel.
  /// </summary>
  /// <param name="request"></param>
  /// <returns>A newly created Channel</returns>
  /// <remarks>
  /// Sample request:
  ///
  /// </remarks>
  /// <response code="201">Returns the newly created item</response>
  /// <response code="400">If the item is null</response>
  [HttpPost]
  public async Task<IActionResult> Post([FromBody] CreateChannelDTO request)
  {
    _logger.LogInformation($"{TAG} post() request={request}");

    Channel channel = request.toChannel();

    _logger.LogInformation($"{TAG} post() channel={channel.Dump()}");

    if (request.Name == null || request.Description == null)
      return BadRequest();

    var dateTimeString = DateTime.UtcNow.ToString(Constants.LastUpdateDateTimeFormat);
    channel.LastUpdate = DateTime.ParseExact(dateTimeString, Constants.LastUpdateDateTimeFormat, null);

    Channel createdChannel = await _repository.AddAsync(channel);
    ChannelDTO result = createdChannel.toChannelDTO();

    _logger.LogInformation($"{TAG} post() result={result.Dump()}");


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
  /// Deletes all Channels.
  /// </summary>
  /// <returns>Count of deleted Channels</returns>
  // NOT TESTED
  // DELETE: api/Channels
  [HttpDelete]
  public async Task<IActionResult> Delete()
  {
    _logger.LogInformation($"{TAG}  Delete()");
    var allRows = await _repository.ListAsync();
    await _repository.DeleteRangeAsync(allRows);

    //return NoContent();
    return Ok(allRows.Count);
  }

  // DELETE: api/Channels
  [HttpDelete("{id:int}")]
  public async Task<IActionResult> DeleteById(int id)
  {
    _logger.LogInformation($"{TAG}  DeleteById() id={id}");

    var channel = await _repository.GetByIdAsync(id);
    if (channel == null)
      return NotFound();

    await _repository.DeleteAsync(channel);

    return NoContent();
  }

  // PATCH: api/Channels
  [HttpPatch("{id:int}")]
  public async Task<IActionResult> DisableChannel(int id)
  {
    _logger.LogInformation($"{TAG} DisableChannel() id={id}");

    Channel? channel = await _repository.GetByIdAsync(id);
    _logger.LogInformation($"{TAG} DisableChannel() channel={channel}");

    if (channel == null)
      return NotFound();

    channel.IsEnable = false;
    await _repository.UpdateAsync(channel);

    Channel? modifiedChannel = await _repository.GetByIdAsync(id);
    ChannelDTO result = modifiedChannel!.toChannelDTO();
    var fcmResponse = await Operations.SendFcmMessages(result, _notificationSender);

    return Ok(
      new
      {
        result,
        fcmResponse
      }
    );

    //return Ok(channel); 
  }

  // NOT TESTED

  // PUT: api/Channels/5
  // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
  [HttpPut("{id}")]
  public async Task<IActionResult> PutChannel(int id, CreateChannelDTO request)
  {
    _logger.LogInformation($"{TAG} PutChannel() id={id} request={request}");

    Channel channel = request.toChannel();
    if (channel is null)
    {
      return BadRequest();
    }

    await _repository.UpdateAsync(channel);

    var resultChannel = await _repository.GetByIdAsync(id);
    if (resultChannel == null)
      return NotFound();

    ChannelDTO createdChannel = resultChannel.toChannelDTO();
    var fcmResponse = await Operations.SendFcmMessages(createdChannel, _notificationSender);

    return Ok(
        new
        {
          createdChannel,
          fcmResponse
        }
    );

  }

  private async Task<bool> ChannelExists(int id)
  {
    var spec = new ChannelByIdSpec(id);
    return await _repository.AnyAsync(spec);
  }

}
