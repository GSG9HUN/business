using DC_bot.Exceptions.Messaging;

namespace DC_bot_tests.UnitTests.Exceptions.Messaging;

public class MessageSendExceptionTests
{
    [Fact]
    public void MessageSendException_OperationIsSet()
    {
        // Arrange
        const string operation = "SendMessage";
        const string message = "Failed to send";

        // Act
        var exception = new MessageSendException(operation, message);

        // Assert
        Assert.Equal(operation, exception.Operation);
        Assert.Contains(message, exception.Message);
    }

    [Fact]
    public void MessageSendException_WithInnerException_PreservesIt()
    {
        // Arrange
        const string operation = "SendEmbed";
        const string message = "Embed send failed";
        var innerException = new TimeoutException("Network timeout");

        // Act
        var exception = new MessageSendException(operation, message, innerException);

        // Assert
        Assert.Equal(operation, exception.Operation);
        Assert.Equal(innerException, exception.InnerException);
    }

    [Fact]
    public void MessageSendException_MultipleOperations_AreIndependent()
    {
        // Arrange & Act
        var exc1 = new MessageSendException("SendMessage", "Failed");
        var exc2 = new MessageSendException("SendEmbed", "Failed");

        // Assert
        Assert.Equal("SendMessage", exc1.Operation);
        Assert.Equal("SendEmbed", exc2.Operation);
    }
}