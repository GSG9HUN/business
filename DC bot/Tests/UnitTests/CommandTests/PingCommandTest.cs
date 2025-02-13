using DC_bot.Commands;
using DC_bot.Interface;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace DC_bot.Tests.UnitTests.CommandTests;

public class PingCommandTest
{
    private readonly Mock<ILogger<PingCommand>> _mockLogger;
    private readonly Mock<IDiscordMessageWrapper> _mockMessage;
    private readonly PingCommand _pingCommand;

    public PingCommandTest()
    {
        _mockLogger = new Mock<ILogger<PingCommand>>();
        _mockMessage = new Mock<IDiscordMessageWrapper>();
        _pingCommand = new PingCommand(_mockLogger.Object);
    }
    
    [Fact]   
    public async Task ExecuteAsync_ShouldSendPongMessage_WhenCalled()
    {
        // Act
        await _pingCommand.ExecuteAsync(_mockMessage.Object);

        // Assert
        _mockMessage.Verify(m => m.RespondAsync("Pong!"), Times.Once);
    }
    
}