namespace notifgram.Infrastructure.Services;

public interface IEmailRequestsHandler
{
  public void ProcessEmailSubject(string subject, string from);
}
