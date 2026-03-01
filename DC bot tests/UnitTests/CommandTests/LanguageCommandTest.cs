using DC_bot.Commands;
using DC_bot.Constants;
using DC_bot.Interface;
using DC_bot.Service;
using Microsoft.Extensions.Logging;
using Moq;

namespace DC_bot_tests.UnitTests.CommandTests;

public class LanguageCommandTest
{
    private const string LanguageCommandName = "language";
    private const string LanguageCommandDescriptionValue = "Change the bot language.";
    private const string LanguageCommandContentNoArgs = "!language";
    private const string LanguageCommandContentHu = "!language hu";
    private const string LanguageCodeHu = "hu";
    private const ulong TestGuildId = 123456UL;
    private const ulong TestUserId = 111112UL;

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

        _localizationServiceMock.Setup(l => l.Get(LocalizationKeys.LanguageCommandDescription))
            .Returns(LanguageCommandDescriptionValue);

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
        userMock.Setup(u => u.Id).Returns(TestUserId);

        _messageMock.Setup(m => m.Content).Returns(LanguageCommandContentNoArgs);
        _messageMock.Setup(m => m.Author).Returns(userMock.Object);
        // Act
        await _languageCommand.ExecuteAsync(_messageMock.Object);

        // Assert
        _responseBuilderMock.Verify(r => r.SendUsageAsync(_messageMock.Object, LanguageCommandName), Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_Should_SaveLanguage_And_Send_Response_When_Language_Is_Provided()
    {
        // Arrange
        var userMock = new Mock<IDiscordUser>();

        userMock.Setup(u => u.IsBot).Returns(false);
        userMock.Setup(u => u.Id).Returns(TestUserId);

        _messageMock.Setup(m => m.Content).Returns(LanguageCommandContentNoArgs);
        _messageMock.Setup(m => m.Author).Returns(userMock.Object);
        _messageMock.Setup(m => m.Content).Returns(LanguageCommandContentHu);
        _guildMock.Setup(g => g.Id).Returns(TestGuildId);

        // Act
        await _languageCommand.ExecuteAsync(_messageMock.Object);

        // Assert
        _localizationServiceMock.Verify(l => l.SaveLanguage(TestGuildId, LanguageCodeHu), Times.Once);
        _responseBuilderMock.Verify(r => r.SendCommandResponseAsync(_messageMock.Object, LanguageCommandName), Times.Once);
    }

    [Fact]
    public void Command_Name_And_Description_ShouldReturnCorrectValue_WhenCalled()
    {
        Assert.Equal(LanguageCommandName, _languageCommand.Name);
        Assert.Equal(LanguageCommandDescriptionValue, _languageCommand.Description);
    }

    [Fact]
    public async Task ExecuteAsync_Should_DoNothing_WhenUserIsBot()
    {
        // Arrange
        var userMock = new Mock<IDiscordUser>();
        userMock.Setup(u => u.IsBot).Returns(true);
        userMock.Setup(u => u.Id).Returns(999999);

        _messageMock.Setup(m => m.Content).Returns(LanguageCommandContentHu);
        _messageMock.Setup(m => m.Author).Returns(userMock.Object);

        // Act
        await _languageCommand.ExecuteAsync(_messageMock.Object);

        // Assert
        _localizationServiceMock.Verify(l => l.SaveLanguage(It.IsAny<ulong>(), It.IsAny<string>()), Times.Never);
        _responseBuilderMock.Verify(r => r.SendCommandResponseAsync(It.IsAny<IDiscordMessage>(), It.IsAny<string>()), Times.Never);
        _responseBuilderMock.Verify(r => r.SendUsageAsync(It.IsAny<IDiscordMessage>(), It.IsAny<string>()), Times.Never);
    }

    // TODO: ExecuteAsync_Should_Error_WhenInvalidLanguageProvided: érvénytelen nyelvkód (pl. "huen", "asder") ->
    //     hibaüzenet küldése (jelenleg ez az eset nincs lekezelve a kódban sem, implementálni kell először)
}