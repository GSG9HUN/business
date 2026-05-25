using DC_bot.Commands.Utility;
using DC_bot.Constants;
using DC_bot.Exceptions.Localization;
using DC_bot.Interface.Core;
using DC_bot.Interface.Discord;
using DC_bot.Interface.Service.Localization;
using DC_bot.Interface.Service.Presentation;
using DC_bot.Service.Core;
using Microsoft.Extensions.Logging;
using Moq;

namespace DC_bot_tests.UnitTests.Commands.Utility;

[Trait("Category", "Unit")]
public class LanguageCommandTests
{
    private const string LanguageCommandName = "language";
    private const string LanguageCommandDescriptionValue = "Change the bot language.";
    private const string LanguageCommandContentNoArgs = "!language";
    private const string LanguageCommandContentHu = "!language hu";
    private const string LanguageCommandContentUpperHu = "!language HU";
    private const string LanguageCodeHu = "hu";
    private const ulong TestGuildId = 123456UL;
    private const ulong TestUserId = 111112UL;
    private readonly Mock<IDiscordGuild> _guildMock;
    private readonly LanguageCommand _languageCommand;
    private readonly Mock<ILocalizationService> _localizationServiceMock;

    private readonly Mock<IDiscordMessage> _messageMock;
    private readonly Mock<IResponseBuilder> _responseBuilderMock;

    public LanguageCommandTests()
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
        var commandHelperMock = new Mock<ICommandHelper>();

        commandHelperMock
            .Setup(h => h.TryGetArgumentAsync(
                It.IsAny<IDiscordMessage>(),
                It.IsAny<IResponseBuilder>(),
                It.IsAny<ILogger>(),
                It.IsAny<string>()))
            .Returns<IDiscordMessage, IResponseBuilder, ILogger, string>(async (msg, rb, _, commandName) =>
            {
                var parts = msg.Content.Split(" ", 2);
                if (parts.Length >= 2) return parts[1].Trim();
                await rb.SendUsageAsync(msg, commandName);
                return null;
            });

        _messageMock.Setup(m => m.Channel).Returns(channelMock.Object);
        channelMock.Setup(c => c.Guild).Returns(_guildMock.Object);
        var userValidationService = new ValidationService(validationLoggerMock.Object);

        _languageCommand = new LanguageCommand(loggerMock.Object, userValidationService, _responseBuilderMock.Object,
            _localizationServiceMock.Object, commandHelperMock.Object);
    }

    [Fact]
    public async Task ExecuteAsync_Should_Return_UsageMessage_When_No_Language_Provided()
    {
        var userMock = new Mock<IDiscordUser>();

        userMock.Setup(u => u.IsBot).Returns(false);
        userMock.Setup(u => u.Id).Returns(TestUserId);

        _messageMock.Setup(m => m.Content).Returns(LanguageCommandContentNoArgs);
        _messageMock.Setup(m => m.Author).Returns(userMock.Object);
        await _languageCommand.ExecuteAsync(_messageMock.Object);

        _responseBuilderMock.Verify(r => r.SendUsageAsync(_messageMock.Object, LanguageCommandName), Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_Should_SaveLanguage_And_Send_Response_When_Language_Is_Provided()
    {
        var userMock = new Mock<IDiscordUser>();

        userMock.Setup(u => u.IsBot).Returns(false);
        userMock.Setup(u => u.Id).Returns(TestUserId);

        _messageMock.Setup(m => m.Author).Returns(userMock.Object);
        _messageMock.Setup(m => m.Content).Returns(LanguageCommandContentHu);
        _guildMock.Setup(g => g.Id).Returns(TestGuildId);

        await _languageCommand.ExecuteAsync(_messageMock.Object);

        _localizationServiceMock.Verify(l => l.SaveLanguage(TestGuildId, LanguageCodeHu), Times.Once);
        _responseBuilderMock.Verify(r => r.SendCommandResponseAsync(_messageMock.Object, LanguageCommandName),
            Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_Should_NormalizeLanguageCode_When_Language_Is_Provided_WithDifferentCasing()
    {
        // Arrange
        var userMock = new Mock<IDiscordUser>();

        userMock.Setup(u => u.IsBot).Returns(false);
        userMock.Setup(u => u.Id).Returns(TestUserId);

        _messageMock.Setup(m => m.Content).Returns(LanguageCommandContentUpperHu);
        _messageMock.Setup(m => m.Author).Returns(userMock.Object);
        _guildMock.Setup(g => g.Id).Returns(TestGuildId);

        // Act
        await _languageCommand.ExecuteAsync(_messageMock.Object);

        // Assert
        _localizationServiceMock.Verify(l => l.SaveLanguage(TestGuildId, LanguageCodeHu), Times.Once);
        _responseBuilderMock.Verify(r => r.SendCommandResponseAsync(_messageMock.Object, LanguageCommandName),
            Times.Once);
    }

    [Theory]
    [InlineData("!language huen")]
    [InlineData("!language hu eng")]
    [InlineData("!language asder")]
    [InlineData("!language ")]
    public async Task ExecuteAsync_Should_SendValidationError_And_NotSave_When_Language_Is_Invalid(string content)
    {
        // Arrange
        var userMock = new Mock<IDiscordUser>();

        userMock.Setup(u => u.IsBot).Returns(false);
        userMock.Setup(u => u.Id).Returns(TestUserId);

        _messageMock.Setup(m => m.Content).Returns(content);
        _messageMock.Setup(m => m.Author).Returns(userMock.Object);
        _guildMock.Setup(g => g.Id).Returns(TestGuildId);

        // Act
        await _languageCommand.ExecuteAsync(_messageMock.Object);

        // Assert
        _localizationServiceMock.Verify(l => l.SaveLanguage(It.IsAny<ulong>(), It.IsAny<string>()), Times.Never);
        _responseBuilderMock.Verify(
            r => r.SendValidationErrorAsync(_messageMock.Object, LocalizationKeys.LanguageCommandInvalidLanguage),
            Times.Once);
        _responseBuilderMock.Verify(r => r.SendCommandResponseAsync(It.IsAny<IDiscordMessage>(), It.IsAny<string>()),
            Times.Never);
    }

    [Fact]
    public async Task ExecuteAsync_WhenLanguageIsInvalid_ShouldSendCommandError()
    {
        var userMock = new Mock<IDiscordUser>();

        userMock.Setup(u => u.IsBot).Returns(false);
        userMock.Setup(u => u.Id).Returns(TestUserId);

        _messageMock.Setup(m => m.Content).Returns("!language invalid");
        _messageMock.Setup(m => m.Author).Returns(userMock.Object);
        _guildMock.Setup(g => g.Id).Returns(TestGuildId);
        _localizationServiceMock
            .Setup(l => l.SaveLanguage(TestGuildId, "invalid"))
            .Throws(new LocalizationException("invalid", "Translation file not found"));

        await _languageCommand.ExecuteAsync(_messageMock.Object);

        _responseBuilderMock.Verify(r => r.SendCommandErrorResponse(_messageMock.Object, LanguageCommandName),
            Times.Once);
        _responseBuilderMock.Verify(r => r.SendCommandResponseAsync(It.IsAny<IDiscordMessage>(), It.IsAny<string>()),
            Times.Never);
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
        var userMock = new Mock<IDiscordUser>();
        userMock.Setup(u => u.IsBot).Returns(true);
        userMock.Setup(u => u.Id).Returns(999999);

        _messageMock.Setup(m => m.Content).Returns(LanguageCommandContentHu);
        _messageMock.Setup(m => m.Author).Returns(userMock.Object);

        await _languageCommand.ExecuteAsync(_messageMock.Object);

        _localizationServiceMock.Verify(l => l.SaveLanguage(It.IsAny<ulong>(), It.IsAny<string>()), Times.Never);
        _responseBuilderMock.Verify(r => r.SendCommandResponseAsync(It.IsAny<IDiscordMessage>(), It.IsAny<string>()),
            Times.Never);
        _responseBuilderMock.Verify(r => r.SendUsageAsync(It.IsAny<IDiscordMessage>(), It.IsAny<string>()),
            Times.Never);
    }

}
