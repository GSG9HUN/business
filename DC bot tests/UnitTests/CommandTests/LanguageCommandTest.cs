using DC_bot.Commands;
using DC_bot.Interface;
using DC_bot.Service;
using Microsoft.Extensions.Logging;
using Moq;

namespace DC_bot_tests.UnitTests.CommandTests;

public class LanguageCommandTest
{
    private readonly Mock<IDiscordMessage> _messageMock;
    private readonly Mock<IDiscordGuild> _guildMock;
    private readonly LanguageCommand _languageCommand;
    private readonly Mock<IResponseBuilder> _responseBuilderMock;
    private readonly Mock<ILocalizationService> _localizationServiceMock;

    public LanguageCommandTest()
    {
        Mock<ILogger<LanguageCommand>> loggerMock = new();
        Mock<IDiscordChannel> channelMock = new();
        Mock<ILogger<ValidationService>> validationLoggerMock = new();

        _localizationServiceMock = new Mock<ILocalizationService>();
        
        _localizationServiceMock.Setup(l => l.Get("language_command_description"))
            .Returns("Change the bot language.");

        _messageMock = new Mock<IDiscordMessage>();
        _guildMock = new Mock<IDiscordGuild>();
        _responseBuilderMock = new Mock<IResponseBuilder>();

        _messageMock.Setup(m => m.Channel).Returns(channelMock.Object);
        channelMock.Setup(c => c.Guild).Returns(_guildMock.Object);
        var userValidationService = new ValidationService(validationLoggerMock.Object);

        _languageCommand = new LanguageCommand(loggerMock.Object, userValidationService, _responseBuilderMock.Object,
            _localizationServiceMock.Object);
    }

    [Fact]
    public async Task ExecuteAsync_Should_Return_UsageMessage_When_No_Language_Provided()
    {
        // Arrange
        var userMock = new Mock<IDiscordUser>();

        userMock.Setup(u => u.IsBot).Returns(false);
        userMock.Setup(u => u.Id).Returns(111112);

        _messageMock.Setup(m => m.Content).Returns("!language");
        _messageMock.Setup(m => m.Author).Returns(userMock.Object);
        // Act
        await _languageCommand.ExecuteAsync(_messageMock.Object);

        // Assert
        _responseBuilderMock.Verify(r => r.SendUsageAsync(_messageMock.Object, "language"), Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_Should_SaveLanguage_And_Send_Response_When_Language_Is_Provided()
    {
        // Arrange
        ulong guildId = 123456;
        var userMock = new Mock<IDiscordUser>();

        userMock.Setup(u => u.IsBot).Returns(false);
        userMock.Setup(u => u.Id).Returns(111112);

        _messageMock.Setup(m => m.Content).Returns("!language");
        _messageMock.Setup(m => m.Author).Returns(userMock.Object);
        _messageMock.Setup(m => m.Content).Returns("!language hu");
        _guildMock.Setup(g => g.Id).Returns(guildId);

        // Act
        await _languageCommand.ExecuteAsync(_messageMock.Object);

        // Assert
        _localizationServiceMock.Verify(l => l.SaveLanguage(guildId, "hu"), Times.Once);
        _responseBuilderMock.Verify(r => r.SendCommandResponseAsync(_messageMock.Object, "language"), Times.Once);
    }

    [Fact]
    public void Command_Name_And_Description_ShouldReturnCorrectValue_WhenCalled()
    {
        Assert.Equal("language", _languageCommand.Name);
        Assert.Equal("Change the bot language.", _languageCommand.Description);
    }
}