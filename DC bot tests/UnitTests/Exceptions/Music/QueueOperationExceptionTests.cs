using DC_bot.Exceptions.Music;

namespace DC_bot_tests.UnitTests.Exceptions.Music;

[Trait("Category", "Unit")]
public class QueueOperationExceptionTests
{
    [Fact]
    public void QueueOperationException_OperationAndGuildIdAreSet()
    {
        const string operation = "SaveQueue";
        const ulong guildId = 123456789;
        const string message = "Save failed";

        var exception = new QueueOperationException(operation, guildId, message);

        Assert.Equal(operation, exception.Operation);
        Assert.Equal(guildId, exception.GuildId);
        Assert.Contains(message, exception.Message);
    }

    [Fact]
    public void QueueOperationException_WithInnerException_PreservesIt()
    {
        var innerException = new IOException("Disk error");

        var exception = new QueueOperationException("LoadQueue", 987654321, "Failed", innerException);

        Assert.Equal(innerException, exception.InnerException);
    }

    [Fact]
    public void QueueOperationException_MessageIncludesGuildId()
    {
        const ulong guildId = 555555555;

        var exception = new QueueOperationException("Save", guildId, "Test");

        Assert.Contains(guildId.ToString(), exception.Message);
    }
}
