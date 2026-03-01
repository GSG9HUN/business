using DC_bot.Commands;
using DC_bot.Constants;
using DC_bot.Interface;
using DC_bot.Service;
using Microsoft.Extensions.Logging;
using Moq;

namespace DC_bot_tests.UnitTests.CommandTests;

public class PingCommandTest
{
    private const string PingCommandName = "ping";
    private const string PingCommandDescriptionValue = "Answer with pong!";

    private readonly Mock<IDiscordMessage> _messageMock;
    private readonly Mock<IDiscordUser> _discordUserMock;
    private readonly Mock<IResponseBuilder> _responseBuilderMock;
    private readonly PingCommand _pingCommand;

    public PingCommandTest()
    {
        Mock<ILogger<PingCommand>> mockLogger = new();
        Mock<ILogger<ValidationService>> validationLoggerMock = new();
        Mock<ILocalizationService> localizationServiceMock = new();

        localizationServiceMock.Setup(g => g.Get(LocalizationKeys.PingCommandDescription))
            .Returns(PingCommandDescriptionValue);

        var userValidationService = new ValidationService(validationLoggerMock.Object);

        _messageMock = new Mock<IDiscordMessage>();
        _discordUserMock = new Mock<IDiscordUser>();
        _responseBuilderMock = new Mock<IResponseBuilder>();

        _pingCommand = new PingCommand(userValidationService, mockLogger.Object, _responseBuilderMock.Object,
            localizationServiceMock.Object);
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
        _responseBuilderMock.Verify(r => r.SendSuccessAsync(_messageMock.Object, LocalizationKeys.PingCommandResponse), Times.Once);
    }

    [Fact]
    public void Command_Name_And_Description_ShouldReturnCorrectValue_WhenCalled()
    {
        Assert.Equal(PingCommandName, _pingCommand.Name);
        Assert.Equal(PingCommandDescriptionValue, _pingCommand.Description);
    }
}