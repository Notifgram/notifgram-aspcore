
namespace notifgram.Infrastructure;

public interface IFcmTokenVerifier
{
  Task<bool> IsValidTokenAsync(string fcmToken);
}
