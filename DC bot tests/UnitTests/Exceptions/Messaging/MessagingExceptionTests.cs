using DC_bot.Exceptions.Messaging;

namespace DC_bot_tests.UnitTests.Exceptions.Messaging;

[Trait("Category", "Unit")]
public class MessageSendExceptionTests
{
    [Fact]
    public void MessageSendException_OperationIsSet()
    {
        const string operation = "SendMessage";
        const string message = "Failed to send";

        var exception = new MessageSendException(operation, message);

        Assert.Equal(operation, exception.Operation);
        Assert.Contains(message, exception.Message);
    }

    [Fact]
    public void MessageSendException_WithInnerException_PreservesIt()
    {
        const string operation = "SendEmbed";
        const string message = "Embed send failed";
        var innerException = new TimeoutException("Network timeout");

        var exception = new MessageSendException(operation, message, innerException);

        Assert.Equal(operation, exception.Operation);
        Assert.Equal(innerException, exception.InnerException);
    }

    [Fact]
    public void MessageSendException_MultipleOperations_AreIndependent()
    {
        var exc1 = new MessageSendException("SendMessage", "Failed");
        var exc2 = new MessageSendException("SendEmbed", "Failed");

        Assert.Equal("SendMessage", exc1.Operation);
        Assert.Equal("SendEmbed", exc2.Operation);
    }
}
