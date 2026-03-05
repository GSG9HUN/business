using DC_bot.Commands.Queue;
using DC_bot.Constants;
using DC_bot.Helper.Validation;
using DC_bot.Interface.Core;
using DC_bot.Interface.Discord;
using DC_bot.Interface.Service.Localization;
using DC_bot.Interface.Service.Music.MusicServiceInterface;
using DC_bot.Interface.Service.Presentation;
using DC_bot.Service.Core;
using Microsoft.Extensions.Logging;
using Moq;

namespace DC_bot_tests.UnitTests.Commands.Queue;

public class RepeatCommandTest
{
    private const string RepeatCommandName = "repeat";
    private const string RepeatCommandDescriptionValue = "Repeats a specified track infinitely.";
    private const string RepeatCommandRepeatingOffValue = "Repeating is off.";
    private const string RepeatCommandRepeatingOnValue = "Repeat is on for :";
    private const string RepeatCommandListAlreadyRepeatingValue = "This track is already repeating.";
    private const string TestTrackTitle = "Test Track";

    private readonly Mock<IRepeatService> _repeatServiceMock;
    private readonly Mock<ICurrentTrackService> _currentTrackServiceMock;
    private readonly Mock<IDiscordMessage> _messageMock;
    private readonly Mock<IDiscordChannel> _channelMock;
    private readonly Mock<IDiscordGuild> _guildMock;
    private readonly Mock<IDiscordUser> _discordUserMock;
    private readonly Mock<IDiscordMember> _discordMemberMock;
    private readonly Mock<IResponseBuilder> _responseBuilderMock;
    private readonly Mock<IDiscordVoiceState> _discordVoiceStateMock;
    private readonly Mock<ILocalizationService> _localizationServiceMock = new();
    private readonly RepeatCommand _repeatCommand;
    private readonly Mock<ICommandHelper> _commandHelperMock;

    public RepeatCommandTest()
    {
        Mock<ILogger<RepeatCommand>> loggerMock = new();
        Mock<ILogger<ValidationService>> validationLoggerMock = new();

        _localizationServiceMock.Setup(g => g.Get(LocalizationKeys.RepeatCommandDescription))
            .Returns(RepeatCommandDescriptionValue);

        _localizationServiceMock.Setup(g => g.Get(LocalizationKeys.RepeatCommandListAlreadyRepeating))
            .Returns(RepeatCommandListAlreadyRepeatingValue);

        _localizationServiceMock.Setup(g => g.Get(LocalizationKeys.RepeatCommandRepeatingOn))
            .Returns(RepeatCommandRepeatingOnValue);

        _localizationServiceMock.Setup(g => g.Get(LocalizationKeys.RepeatCommandRepeatingOff))
            .Returns(RepeatCommandRepeatingOffValue);

        _repeatServiceMock = new Mock<IRepeatService>();
        _currentTrackServiceMock = new Mock<ICurrentTrackService>();
        _messageMock = new Mock<IDiscordMessage>();
        _channelMock = new Mock<IDiscordChannel>();
        _guildMock = new Mock<IDiscordGuild>();
        _discordUserMock = new Mock<IDiscordUser>();
        _discordMemberMock = new Mock<IDiscordMember>();
        _discordVoiceStateMock = new Mock<IDiscordVoiceState>();
        _responseBuilderMock = new Mock<IResponseBuilder>();
        _commandHelperMock = new Mock<ICommandHelper>();

        var userValidationService = new ValidationService(validationLoggerMock.Object);
        _repeatCommand = new RepeatCommand(_repeatServiceMock.Object, _currentTrackServiceMock.Object, userValidationService, loggerMock.Object,
            _responseBuilderMock.Object, _localizationServiceMock.Object, _commandHelperMock.Object);
    }

    [Fact]
    public async Task ExecuteAsync_ListIsAlreadyRepeating_ShouldSendMessage()
    {
        // Arrange
        const ulong guildId = 123456789UL;

        _channelMock.SetupGet(c => c.Guild).Returns(_guildMock.Object);
        _guildMock.SetupGet(g => g.Id).Returns(guildId);
        _messageMock.SetupGet(m => m.Channel).Returns(_channelMock.Object);
        _messageMock.SetupGet(m => m.Author).Returns(_discordUserMock.Object);

        _commandHelperMock
            .Setup(h => h.TryValidateUserAsync(
                It.IsAny<IUserValidationService>(),
                It.IsAny<IResponseBuilder>(),
                It.IsAny<IDiscordMessage>()))
            .ReturnsAsync(new UserValidationResult(true, string.Empty, _discordMemberMock.Object));

        _repeatServiceMock.Setup(l => l.IsRepeatingList(guildId)).Returns(true);

        // Act
        await _repeatCommand.ExecuteAsync(_messageMock.Object);

        // Assert
        _responseBuilderMock.Verify(r => r.SendSuccessAsync(_messageMock.Object,
            _localizationServiceMock.Object.Get(LocalizationKeys.RepeatCommandListAlreadyRepeating)));
    }

    [Fact]
    public async Task ExecuteAsync_TrackIsRepeating_ShouldTurnOffRepeat()
    {
        // Arrange
        const ulong guildId = 123456789UL;

        _channelMock.SetupGet(c => c.Guild).Returns(_guildMock.Object);
        _guildMock.SetupGet(g => g.Id).Returns(guildId);
        _messageMock.SetupGet(m => m.Channel).Returns(_channelMock.Object);
        _messageMock.SetupGet(m => m.Author).Returns(_discordUserMock.Object);

        _commandHelperMock
            .Setup(h => h.TryValidateUserAsync(
                It.IsAny<IUserValidationService>(),
                It.IsAny<IResponseBuilder>(),
                It.IsAny<IDiscordMessage>()))
            .ReturnsAsync(new UserValidationResult(true, string.Empty, _discordMemberMock.Object));

        _repeatServiceMock.Setup(l => l.IsRepeatingList(guildId)).Returns(false);
        _repeatServiceMock.Setup(l => l.IsRepeating(guildId)).Returns(true);

        // Act
        await _repeatCommand.ExecuteAsync(_messageMock.Object);

        // Assert
        _repeatServiceMock.Verify(l => l.SetRepeating(guildId, false), Times.Once);
        _responseBuilderMock.Verify(
            r => r.SendSuccessAsync(_messageMock.Object,
                _localizationServiceMock.Object.Get(LocalizationKeys.RepeatCommandRepeatingOff)), Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_NoRepeat_ShouldEnableRepeat()
    {
        // Arrange
        const ulong guildId = 123456789UL;

        _channelMock.SetupGet(c => c.Guild).Returns(_guildMock.Object);
        _guildMock.SetupGet(g => g.Id).Returns(guildId);
        _messageMock.SetupGet(m => m.Channel).Returns(_channelMock.Object);
        _messageMock.SetupGet(m => m.Author).Returns(_discordUserMock.Object);

        _commandHelperMock
            .Setup(h => h.TryValidateUserAsync(
                It.IsAny<IUserValidationService>(),
                It.IsAny<IResponseBuilder>(),
                It.IsAny<IDiscordMessage>()))
            .ReturnsAsync(new UserValidationResult(true, string.Empty, _discordMemberMock.Object));

        _repeatServiceMock.Setup(l => l.IsRepeatingList(guildId)).Returns(false);
        _repeatServiceMock.Setup(l => l.IsRepeating(guildId)).Returns(false);
        _currentTrackServiceMock.Setup(c => c.GetCurrentTrackFormatted(guildId)).Returns($"{TestTrackTitle} Test Author");

        // Act
        await _repeatCommand.ExecuteAsync(_messageMock.Object);

        // Assert
        _repeatServiceMock.Verify(l => l.SetRepeating(guildId, true), Times.Once);
        _responseBuilderMock.Verify(
            r => r.SendSuccessAsync(_messageMock.Object, It.IsAny<string>()), Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_UserIsABot_ShouldDoNothing()
    {
        //Arrange
        _messageMock.SetupGet(m => m.Author).Returns(_discordUserMock.Object);

        _commandHelperMock
            .Setup(h => h.TryValidateUserAsync(
                It.IsAny<IUserValidationService>(),
                It.IsAny<IResponseBuilder>(),
                It.IsAny<IDiscordMessage>()))
            .ReturnsAsync((UserValidationResult?)null);

        // Act
        await _repeatCommand.ExecuteAsync(_messageMock.Object);

        //Assert
        _responseBuilderMock.Verify(r => r.SendSuccessAsync(It.IsAny<IDiscordMessage>(), It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task ExecuteAsync_UserIsNotInVoiceChannel()
    {
        //Arrange
        _messageMock.SetupGet(m => m.Author).Returns(_discordUserMock.Object);
        _messageMock.SetupGet(m => m.Channel).Returns(_channelMock.Object);

        _commandHelperMock
            .Setup(h => h.TryValidateUserAsync(
                It.IsAny<IUserValidationService>(),
                It.IsAny<IResponseBuilder>(),
                It.IsAny<IDiscordMessage>()))
            .Returns<IUserValidationService, IResponseBuilder, IDiscordMessage>(
                async (_, rb, msg) =>
                {
                    await rb.SendValidationErrorAsync(msg, ValidationErrorKeys.UserNotInVoiceChannel);
                    return null;
                });

        // Act
        await _repeatCommand.ExecuteAsync(_messageMock.Object);

        //Assert
        _responseBuilderMock.Verify(r =>
            r.SendValidationErrorAsync(_messageMock.Object, ValidationErrorKeys.UserNotInVoiceChannel), Times.Once);
    }

    [Fact]
    public void Command_Name_And_Description_ShouldReturnCorrectValue_WhenCalled()
    {
        Assert.Equal(RepeatCommandName, _repeatCommand.Name);
        Assert.Equal(RepeatCommandDescriptionValue, _repeatCommand.Description);
    }
}