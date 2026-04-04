using DC_bot_tests.TestHelperFiles;
using DC_bot.Commands.Queue;
using DC_bot.Constants;
using DC_bot.Helper.Validation;
using DC_bot.Interface;
using DC_bot.Interface.Core;
using DC_bot.Interface.Discord;
using DC_bot.Interface.Service.Localization;
using DC_bot.Interface.Service.Music.MusicServiceInterface;
using DC_bot.Interface.Service.Presentation;
using DC_bot.Service.Core;
using Microsoft.Extensions.Logging;
using Moq;

namespace DC_bot_tests.UnitTests.Commands.Queue;

public class RepeatListCommandTests
{
    private const string RepeatListCommandName = "repeatList";
    private const string RepeatListCommandDescriptionValue = "Repeats the current track list.";
    private const string RepeatListRepeatingOnValue = "Repeat is on for current list:";
    private const string RepeatListRepeatingOffValue = "Repeating is off for the list:";
    private const string RepeatListTrackAlreadyRepeatingValue = "This track is already repeating.";
    private const string VoiceChannelRequiredValue = "You must be in a voice channel!";
    private const string TestTrackList = "test track list";
    private readonly Mock<IDiscordChannel> _channelMock;
    private readonly Mock<ICommandHelper> _commandHelperMock;
    private readonly Mock<ICurrentTrackService> _currentTrackServiceMock;
    private readonly Mock<IDiscordMember> _discordMemberMock;
    private readonly Mock<IDiscordUser> _discordUserMock;
    private readonly Mock<IDiscordVoiceState> _discordVoiceStateMock;
    private readonly Mock<IDiscordGuild> _guildMock;
    private readonly Mock<ILocalizationService> _localizationServiceMock = new();
    private readonly Mock<IDiscordMessage> _messageMock;
    private readonly Mock<IMusicQueueService> _queueServiceMock;
    private readonly RepeatListCommand _repeatListCommand;

    private readonly Mock<IRepeatService> _repeatServiceMock;
    private readonly Mock<IResponseBuilder> _responseBuilderMock;
    private readonly Mock<ITrackFormatterService> _trackServiceFormatter;

    public RepeatListCommandTests()
    {
        Mock<ILogger<RepeatListCommand>> loggerMock = new();
        Mock<ILogger<ValidationService>> validationLoggerMock = new();

        _localizationServiceMock.Setup(g => g.Get(LocalizationKeys.RepeatListCommandDescription))
            .Returns(RepeatListCommandDescriptionValue);

        _localizationServiceMock.Setup(g => g.Get(LocalizationKeys.RepeatListCommandRepeatingOn))
            .Returns(RepeatListRepeatingOnValue);

        _localizationServiceMock.Setup(g => g.Get(LocalizationKeys.RepeatListCommandRepeatingOff))
            .Returns(RepeatListRepeatingOffValue);

        _localizationServiceMock.Setup(g => g.Get(LocalizationKeys.RepeatListCommandTrackAlreadyRepeating))
            .Returns(RepeatListTrackAlreadyRepeatingValue);

        _localizationServiceMock.Setup(g => g.Get(ValidationErrorKeys.UserNotInVoiceChannel))
            .Returns(VoiceChannelRequiredValue);

        _discordUserMock = new Mock<IDiscordUser>();
        _discordMemberMock = new Mock<IDiscordMember>();
        _discordVoiceStateMock = new Mock<IDiscordVoiceState>();
        _repeatServiceMock = new Mock<IRepeatService>();
        _currentTrackServiceMock = new Mock<ICurrentTrackService>();
        _queueServiceMock = new Mock<IMusicQueueService>();
        _trackServiceFormatter = new Mock<ITrackFormatterService>();
        _messageMock = new Mock<IDiscordMessage>();
        _channelMock = new Mock<IDiscordChannel>();
        _guildMock = new Mock<IDiscordGuild>();
        _responseBuilderMock = new Mock<IResponseBuilder>();
        _commandHelperMock = new Mock<ICommandHelper>();

        var userValidationService = new ValidationService(validationLoggerMock.Object);
        _repeatListCommand =
            new RepeatListCommand(
                _repeatServiceMock.Object,
                _currentTrackServiceMock.Object,
                _queueServiceMock.Object,
                userValidationService,
                loggerMock.Object,
                _responseBuilderMock.Object,
                _trackServiceFormatter.Object,
                _localizationServiceMock.Object, _commandHelperMock.Object);
    }

    [Fact]
    public async Task ExecuteAsync_TackIsAlreadyRepeating_ShouldSendMessage()
    {
        // Arrange
        const ulong guildId = 123456789UL;

        _discordUserMock.SetupGet(du => du.IsBot).Returns(false);
        _discordMemberMock.SetupGet(dm => dm.VoiceState).Returns(_discordVoiceStateMock.Object);

        _discordVoiceStateMock.SetupGet(vs => vs.Channel).Returns(_channelMock.Object);
        _guildMock.SetupGet(g => g.Id).Returns(guildId);
        _guildMock.Setup(g => g.GetMemberAsync(_discordMemberMock.Object.Id)).ReturnsAsync(_discordMemberMock.Object);

        _channelMock.SetupGet(c => c.Guild).Returns(_guildMock.Object);
        _messageMock.SetupGet(m => m.Channel).Returns(_channelMock.Object);
        _messageMock.SetupGet(m => m.Author).Returns(_discordUserMock.Object);

        _commandHelperMock
            .Setup(h => h.TryValidateUserAsync(
                It.IsAny<IUserValidationService>(),
                It.IsAny<IResponseBuilder>(),
                It.IsAny<IDiscordMessage>()))
            .ReturnsAsync(new UserValidationResult(true, string.Empty, _discordMemberMock.Object));

        _repeatServiceMock.Setup(l => l.IsRepeatingAsync(guildId)).ReturnsAsync(true);

        // Act
        await _repeatListCommand.ExecuteAsync(_messageMock.Object);

        // Assert
        _responseBuilderMock.Verify(r => r.SendSuccessAsync(_messageMock.Object,
            _localizationServiceMock.Object.Get(LocalizationKeys.RepeatListCommandTrackAlreadyRepeating)));
    }

    [Fact]
    public async Task ExecuteAsync_ListIsRepeating_ShouldTurnOffRepeat()
    {
        // Arrange
        const ulong guildId = 123456789UL;

        _discordUserMock.SetupGet(du => du.IsBot).Returns(false);
        _discordMemberMock.SetupGet(dm => dm.VoiceState).Returns(_discordVoiceStateMock.Object);

        _discordVoiceStateMock.SetupGet(vs => vs.Channel).Returns(_channelMock.Object);
        _guildMock.SetupGet(g => g.Id).Returns(guildId);
        _guildMock.Setup(g => g.GetMemberAsync(_discordMemberMock.Object.Id)).ReturnsAsync(_discordMemberMock.Object);

        _channelMock.SetupGet(c => c.Guild).Returns(_guildMock.Object);
        _messageMock.SetupGet(m => m.Channel).Returns(_channelMock.Object);
        _messageMock.SetupGet(m => m.Author).Returns(_discordUserMock.Object);

        _commandHelperMock
            .Setup(h => h.TryValidateUserAsync(
                It.IsAny<IUserValidationService>(),
                It.IsAny<IResponseBuilder>(),
                It.IsAny<IDiscordMessage>()))
            .ReturnsAsync(new UserValidationResult(true, string.Empty, _discordMemberMock.Object));

        _repeatServiceMock.Setup(l => l.IsRepeatingAsync(guildId)).ReturnsAsync(false);
        _repeatServiceMock.Setup(l => l.IsRepeatingListAsync(guildId)).ReturnsAsync(true);
        _trackServiceFormatter.Setup(c => c.FormatCurrentTrackListAsync(guildId)).ReturnsAsync(TestTrackList);

        // Act
        await _repeatListCommand.ExecuteAsync(_messageMock.Object);

        // Assert
        _repeatServiceMock.Verify(l => l.SetRepeatingListAsync(guildId, false), Times.Once);
        _responseBuilderMock.Verify(
            r => r.SendSuccessAsync(_messageMock.Object,
                It.IsAny<string>()), Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_NoRepeat_ShouldEnableRepeat()
    {
        // Arrange
        const ulong guildId = 123456789UL;

        _discordUserMock.SetupGet(du => du.IsBot).Returns(false);
        _discordMemberMock.SetupGet(dm => dm.VoiceState).Returns(_discordVoiceStateMock.Object);

        _discordVoiceStateMock.SetupGet(vs => vs.Channel).Returns(_channelMock.Object);
        _guildMock.SetupGet(g => g.Id).Returns(guildId);
        _guildMock.Setup(g => g.GetMemberAsync(_discordMemberMock.Object.Id)).ReturnsAsync(_discordMemberMock.Object);

        _channelMock.SetupGet(c => c.Guild).Returns(_guildMock.Object);
        _messageMock.SetupGet(m => m.Channel).Returns(_channelMock.Object);
        _messageMock.SetupGet(m => m.Author).Returns(_discordUserMock.Object);

        _commandHelperMock
            .Setup(h => h.TryValidateUserAsync(
                It.IsAny<IUserValidationService>(),
                It.IsAny<IResponseBuilder>(),
                It.IsAny<IDiscordMessage>()))
            .ReturnsAsync(new UserValidationResult(true, string.Empty, _discordMemberMock.Object));

        _repeatServiceMock.Setup(l => l.IsRepeatingAsync(guildId)).ReturnsAsync(false);
        _repeatServiceMock.Setup(l => l.IsRepeatingListAsync(guildId)).ReturnsAsync(false);
        _trackServiceFormatter.Setup(c => c.FormatCurrentTrackListAsync(guildId)).ReturnsAsync(TestTrackList);
        _queueServiceMock.Setup(q => q.ViewQueue(guildId)).ReturnsAsync(new List<ILavaLinkTrack>());
        _currentTrackServiceMock.Setup(c => c.GetCurrentTrack(guildId)).Returns(
            TrackTestHelper.CreateTrackWrapper("Test", "Test Author", "asdasdasdad")
        );

        // Act
        await _repeatListCommand.ExecuteAsync(_messageMock.Object);

        // Assert
        _repeatServiceMock.Verify(l => l.SaveRepeatListSnapshotAsync(guildId, It.IsAny<ILavaLinkTrack>(), It.IsAny<IReadOnlyCollection<ILavaLinkTrack>>()), Times.Once);
        _repeatServiceMock.Verify(l => l.SetRepeatingListAsync(guildId, true), Times.Once);
        _responseBuilderMock.Verify(
            r => r.SendSuccessAsync(_messageMock.Object,
                It.IsAny<string>()), Times.Once);
    }


    [Fact]
    public async Task ExecuteAsync_UserIsABot_ShouldDoNothing()
    {
        //Arrange
        _discordUserMock.SetupGet(du => du.IsBot).Returns(true);
        _messageMock.SetupGet(m => m.Author).Returns(_discordUserMock.Object);

        _commandHelperMock
            .Setup(h => h.TryValidateUserAsync(
                It.IsAny<IUserValidationService>(),
                It.IsAny<IResponseBuilder>(),
                It.IsAny<IDiscordMessage>()))
            .ReturnsAsync((UserValidationResult?)null);

        // Act
        await _repeatListCommand.ExecuteAsync(_messageMock.Object);

        //Assert
        _messageMock.Verify(m => m.RespondAsync(It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task ExecuteAsync_UserIsNotInVoiceChannel()
    {
        //Arrange
        _guildMock.Setup(g => g.GetMemberAsync(_discordUserMock.Object.Id)).ReturnsAsync(_discordMemberMock.Object);
        _discordMemberMock.SetupGet(dm => dm.VoiceState).Returns((IDiscordVoiceState?)null);
        _discordUserMock.SetupGet(du => du.IsBot).Returns(false);
        _channelMock.SetupGet(c => c.Guild).Returns(_guildMock.Object);
        _messageMock.SetupGet(m => m.Author).Returns(_discordUserMock.Object);
        _messageMock.SetupGet(m => m.Channel).Returns(_channelMock.Object);

        _commandHelperMock
            .Setup(h => h.TryValidateUserAsync(
                It.IsAny<IUserValidationService>(),
                It.IsAny<IResponseBuilder>(),
                It.IsAny<IDiscordMessage>()))
            .Returns<IUserValidationService, IResponseBuilder, IDiscordMessage>(async (_, rb, msg) =>
            {
                await rb.SendValidationErrorAsync(msg, ValidationErrorKeys.UserNotInVoiceChannel);
                return null;
            });

        // Act
        await _repeatListCommand.ExecuteAsync(_messageMock.Object);

        //Assert
        _responseBuilderMock.Verify(r =>
            r.SendValidationErrorAsync(_messageMock.Object, ValidationErrorKeys.UserNotInVoiceChannel), Times.Once);
    }

    [Fact]
    public void Command_Name_And_Description_ShouldReturnCorrectValue_WhenCalled()
    {
        Assert.Equal(RepeatListCommandName, _repeatListCommand.Name);
        Assert.Equal(RepeatListCommandDescriptionValue, _repeatListCommand.Description);
    }
}