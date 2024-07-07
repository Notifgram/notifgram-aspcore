namespace notifgram.Core.Interfaces;

public interface IEmailSender
{
  Task SendEmailAsync(string to,  string subject, string body);
}
