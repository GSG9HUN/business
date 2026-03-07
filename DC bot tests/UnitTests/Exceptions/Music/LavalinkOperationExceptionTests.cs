using DC_bot.Exceptions.Music;

namespace DC_bot_tests.UnitTests.Exceptions.Music;

public class LavalinkOperationExceptionTests
{
    [Fact]
    public void LavalinkOperationException_OperationIsSet()
    {
        // Arrange
        const string operation = "Connect";
        const string message = "Failed to connect";

        // Act
        var exception = new LavalinkOperationException(operation, message);

        // Assert
        Assert.Equal(operation, exception.Operation);
        Assert.Contains(message, exception.Message);
    }

    [Fact]
    public void LavalinkOperationException_WithInnerException_PreservesIt()
    {
        // Arrange
        var innerException = new InvalidOperationException("Server down");

        // Act
        var exception = new LavalinkOperationException("Connect", "Failed", innerException);

        // Assert
        Assert.Equal(innerException, exception.InnerException);
    }
}