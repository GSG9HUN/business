using DC_bot.Exceptions.Music;

namespace DC_bot_tests.UnitTests.Exceptions.Music;

public class QueueOperationExceptionTests
{
    [Fact]
    public void QueueOperationException_OperationAndGuildIdAreSet()
    {
        // Arrange
        const string operation = "SaveQueue";
        const ulong guildId = 123456789;
        const string message = "Save failed";

        // Act
        var exception = new QueueOperationException(operation, guildId, message);

        // Assert
        Assert.Equal(operation, exception.Operation);
        Assert.Equal(guildId, exception.GuildId);
        Assert.Contains(message, exception.Message);
    }

    [Fact]
    public void QueueOperationException_WithInnerException_PreservesIt()
    {
        // Arrange
        var innerException = new IOException("Disk error");

        // Act
        var exception = new QueueOperationException("LoadQueue", 987654321, "Failed", innerException);

        // Assert
        Assert.Equal(innerException, exception.InnerException);
    }

    [Fact]
    public void QueueOperationException_MessageIncludesGuildId()
    {
        // Arrange
        const ulong guildId = 555555555;

        // Act
        var exception = new QueueOperationException("Save", guildId, "Test");

        // Assert
        Assert.Contains(guildId.ToString(), exception.Message);
    }
}