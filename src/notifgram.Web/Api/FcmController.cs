using FirebaseAdmin.Messaging;
using Microsoft.AspNetCore.Mvc;
using System.Text;
using System.Text.Json;
using System.Security.Cryptography;
using notifgram.Core;
using notifgram.Core.Interfaces;

namespace fcmAspCore.Controllers;

[ApiController]
[Route("[controller]")]
public class FcmController : ControllerBase
{
  private readonly ILogger<FcmController> _logger;
  private readonly INotificationSender _notificationSender;
  //private FirebaseApp defaultApp;

  public FcmController(ILogger<FcmController> logger, INotificationSender notificationSender)
  {
    _logger = logger;
    _notificationSender = notificationSender;

    //defaultApp = FirebaseApp.Create(new AppOptions()
    //{
    //    Credential = GoogleCredential.FromFile(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "key.json")),
    //});
    //Console.WriteLine(defaultApp.Name); // "[DEFAULT]"
  }

  //[HttpGet("[action]")]
  //public async Task<string> SendNotification()
  //{
  //  var message = new Message()
  //  {
  //    Notification = new Notification
  //    {
  //      Title = "Notif title",
  //      Body = "Notif body"
  //    },

  //    //Token = "",
  //    Topic = "a"
  //  };

  //  return await _notificationSender.SendNotificationAsync(message);

  //}


  [HttpGet("SendText")]
  public async Task<string> SendText()
  {
    var message = new Message()
    {
      Data = new Dictionary<string, string>()
      {
        [FcmKeys.TEXT_CONTENT.ToString()] = "This is a text message from backend" +
            "\n sent through FCM message"
      },
      //Token = "d3aLewjvTNw:APA91bE94LuGCqCSInwVaPuL1RoqWokeSLtwauyK-r0EmkPNeZmGavSG6ZgYQ4GRjp0NgOI1p-OAKORiNPHZe2IQWz5v1c3mwRE5s5WTv6_Pbhh58rY0yGEMQdDNEtPPZ_kJmqN5CaIc",
      Topic = FcmTopics.FCM_SYNC_TOPIC.ToString()
    };
    return await _notificationSender.SendNotificationAsync(message);
  }

  [HttpGet("[action]")]
  public async Task<IActionResult> SendFile(string fileName)
  {
    // Split the file into chunks
    // Maximum payload is 4000 bytes
    const int chunkSize = ((int)(2.85 * 1024)); // 4 KB
    var chunks = new List<byte[]>();
    var buffer = new byte[chunkSize];
    int bytesRead;
    // var fileName =   "c.jpg";// "a.txt"; 
    var filePath = fileName; // Replace with the path to your file
    if (!System.IO.File.Exists(filePath))
      return BadRequest("File not found");

    _logger.LogInformation(">>>>>>> FILE HASH=" + A.GetFileHash(filePath));
    var fileSize = new FileInfo(filePath).Length;
    _logger.LogInformation($"file size of {filePath}={fileSize} bytes");

    using (var stream = System.IO.File.OpenRead(filePath))
    {
      _logger.LogInformation($"file size of {filePath}={stream.Length} Bytes");
      while ((bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length)) > 0)
      {
        var chunk = new byte[bytesRead];
        Array.Copy(buffer, chunk, bytesRead);
        chunks.Add(chunk);
      }
    }
    //System.IO.File.WriteAllBytes    
    // Send each chunk as a separate FCM message
    var messages = new List<Message>();
    for (var i = 0; i < chunks.Count; i++)
    {
      var encodedData = Convert.ToBase64String(chunks[i]);
      _logger.LogInformation($"\n Data={chunks[i]}");
      _logger.LogInformation($"\n encodedData={encodedData}");

      var data = new Dictionary<string, string>
            {
                { FcmKeys.FILE_NAME.ToString(), fileName },
                { FcmKeys.FILE_CHUNK_INDEX.ToString(), i.ToString() },
                { FcmKeys.FILE_TOTAL_CHUNKS.ToString(), chunks.Count.ToString() },
                { FcmKeys.FILE_CHUNK_DATA.ToString(), encodedData },
            };
      int payloadSizeInBytes = CalculateSizeOfPayload(data);
      _logger.LogInformation($"Payload size: {payloadSizeInBytes}");

      var message = new Message
      {
        Data = data,
        Topic = FcmTopics.FCM_SYNC_TOPIC.ToString(),
        //Android = new AndroidConfig
        //{
        //    Priority = Priority.High
        //},
        //Apns = new ApnsConfig
        //{
        //    Aps = new Aps
        //    {
        //        ContentAvailable = true
        //    }
        //}
      };
      //message.Data["chunk"] = Convert.ToBase64String(chunks[i]);
      messages.Add(message);

    }

    BatchResponse response;
    try
    {
      response = await _notificationSender.SendAllNotificationAsync(messages);
      return Ok(response);
    }
    catch (FirebaseMessagingException ex)
    {
      _logger.LogInformation($"Error sending FCM message: {ex.Message}");
      return StatusCode(500, $"Error sending FCM message: {ex.Message}");
    }

  }

  private static int CalculateSizeOfPayload(Dictionary<string, string> payload)
  {
    string payloadJson = JsonSerializer.Serialize(payload);
    byte[] payloadBytes = Encoding.UTF8.GetBytes(payloadJson);
    return payloadBytes.Length;
  }



}

public static class A
{
  public static string GetFileHash(string filePath)
  {
    using (var stream = new BufferedStream(File.OpenRead(filePath), 1200000))
    {
      var sha256 = SHA256.Create();
      byte[] hashBytes = sha256.ComputeHash(stream);
      return BitConverter.ToString(hashBytes).Replace("-", "").ToLower();
    }
  }
}
