using DC_bot.Exceptions.Music;

namespace DC_bot_tests.UnitTests.Exceptions.Music;

[Trait("Category", "Unit")]
public class LavalinkOperationExceptionTests
{
    [Fact]
    public void LavalinkOperationException_OperationIsSet()
    {
        const string operation = "Connect";
        const string message = "Failed to connect";

        var exception = new LavalinkOperationException(operation, message);

        Assert.Equal(operation, exception.Operation);
        Assert.Contains(message, exception.Message);
    }

    [Fact]
    public void LavalinkOperationException_WithInnerException_PreservesIt()
    {
        var innerException = new InvalidOperationException("Server down");

        var exception = new LavalinkOperationException("Connect", "Failed", innerException);

        Assert.Equal(innerException, exception.InnerException);
    }
}
