using Microsoft.Extensions.Logging;

namespace notifgram.Infrastructure;
public class FcmTokenVerifier(ILogger<FcmTokenVerifier> logger) : IFcmTokenVerifier
{
  private const string TAG = "FcmTokenVerifier";
  private readonly ILogger<FcmTokenVerifier> _logger = logger;

  public async Task<bool> IsValidTokenAsync(string fcmToken)
  {
    _logger.LogInformation($"{TAG} IsValidTokenAsync() fcmToken={fcmToken}");
    var firebaseAuth = FirebaseAdmin.Auth.FirebaseAuth.DefaultInstance;
    try
    {
      var token = await firebaseAuth.VerifyIdTokenAsync(fcmToken);
      _logger.LogInformation($"{TAG} IsValidTokenAsync() fcm token is valid.");
      return true;
    }
    catch (Exception)
    {
      _logger.LogError($"{TAG} IsValidTokenAsync() fcm token is not valid.");
      return false;
    }
  }

}
