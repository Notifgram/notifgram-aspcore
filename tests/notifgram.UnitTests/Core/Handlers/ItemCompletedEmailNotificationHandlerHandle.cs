using notifgram.Core.Interfaces;
using notifgram.Core.ChannelAggregate.Handlers;
using Moq;

namespace notifgram.UnitTests.Core.Handlers;

public class ItemCompletedEmailNotificationHandlerHandle
{
  private ItemCompletedEmailNotificationHandler _handler;
  private Mock<IEmailSender> _emailSenderMock;

  public ItemCompletedEmailNotificationHandlerHandle()
  {
    _emailSenderMock = new Mock<IEmailSender>();
    _handler = new ItemCompletedEmailNotificationHandler(_emailSenderMock.Object);
  }


}
