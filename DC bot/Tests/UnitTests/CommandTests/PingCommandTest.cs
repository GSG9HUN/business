using DC_bot.Commands;
using DC_bot.Interface;
using DC_bot.Service;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace DC_bot.Tests.UnitTests.CommandTests;

public class PingCommandTest
{
    private readonly Mock<IDiscordMessage> _messageMock;
    private readonly Mock<IDiscordUser> _discordUserMock;
    private readonly PingCommand _pingCommand;

    public PingCommandTest()
    {
        Mock<ILogger<PingCommand>> mockLogger = new();
        Mock<ILogger<UserValidationService>> userLogger = new();
        
        var userValidationService = new UserValidationService(userLogger.Object);
        
        _messageMock = new Mock<IDiscordMessage>();
        _discordUserMock = new Mock<IDiscordUser>();
        _pingCommand = new PingCommand(userValidationService, mockLogger.Object);
    }
    
    [Fact]
    public async Task ExecuteAsync_UserIsBot_ShouldSendNothing()
    {
        //Arrange
        _discordUserMock.SetupGet(du => du.IsBot).Returns(true);
        _messageMock.SetupGet(m => m.Author).Returns(_discordUserMock.Object);
        
        // Act
        await _pingCommand.ExecuteAsync(_messageMock.Object);

        // Assert
        _messageMock.Verify(m => m.RespondAsync(It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task ExecuteAsync_UserIsNotBot_ShouldSendPongMessage()
    {
        //Arrange
        _discordUserMock.SetupGet(du => du.IsBot).Returns(false);
        _messageMock.SetupGet(m => m.Author).Returns(_discordUserMock.Object);
        
        // Act
        await _pingCommand.ExecuteAsync(_messageMock.Object);

        // Assert
        _messageMock.Verify(m => m.RespondAsync("Pong!"), Times.Once);
    }

    [Fact]
    public void Command_Name_And_Description_ShouldReturnCorrectValue_WhenCalled()
    {
        Assert.Equal("ping", _pingCommand.Name);
        Assert.Equal("Answer with pong!", _pingCommand.Description);
    }
}