using DC_bot.Commands;
using DC_bot.Constants;
using DC_bot.Interface;
using DC_bot.Service;
using Microsoft.Extensions.Logging;
using Moq;

namespace DC_bot_tests.UnitTests.CommandTests;

public class RepeatListCommandTest
{
    private const string RepeatListCommandName = "repeatList";
    private const string RepeatListCommandDescriptionValue = "Repeats the current track list.";
    private const string RepeatListRepeatingOnValue = "Repeat is on for current list:";
    private const string RepeatListRepeatingOffValue = "Repeating is off for the list:";
    private const string RepeatListTrackAlreadyRepeatingValue = "This track is already repeating.";
    private const string VoiceChannelRequiredValue = "You must be in a voice channel!";
    private const string TestTrackList = "test track list";

    private readonly Mock<ILavaLinkService> _lavaLinkServiceMock;
    private readonly Mock<IDiscordMessage> _messageMock;
    private readonly Mock<IDiscordChannel> _channelMock;
    private readonly Mock<IDiscordGuild> _guildMock;
    private readonly Mock<IDiscordUser> _discordUserMock;
    private readonly Mock<IDiscordMember> _discordMemberMock;
    private readonly Mock<IResponseBuilder> _responseBuilderMock;
    private readonly Mock<IDiscordVoiceState> _discordVoiceStateMock;
    private readonly RepeatListCommand _repeatListCommand;
    private readonly Mock<ILocalizationService> _localizationServiceMock = new();

    public RepeatListCommandTest()
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
        _lavaLinkServiceMock = new Mock<ILavaLinkService>();
        _messageMock = new Mock<IDiscordMessage>();
        _channelMock = new Mock<IDiscordChannel>();
        _guildMock = new Mock<IDiscordGuild>();
        _responseBuilderMock = new Mock<IResponseBuilder>();

        var userValidationService = new ValidationService(validationLoggerMock.Object);
        _repeatListCommand =
            new RepeatListCommand(_lavaLinkServiceMock.Object, userValidationService, loggerMock.Object,
                _responseBuilderMock.Object, _localizationServiceMock.Object);
    }

    [Fact]
    public async Task ExecuteAsync_TackIsAlreadyRepeating_ShouldSendMessage()
    {
        // Arrange
        const ulong guildId = 123456789UL;
        var isRepeating = new Dictionary<ulong, bool> { { guildId, true } };

        _discordUserMock.SetupGet(du => du.IsBot).Returns(false);
        _discordMemberMock.SetupGet(dm => dm.VoiceState).Returns(_discordVoiceStateMock.Object);

        _discordVoiceStateMock.SetupGet(vs => vs.Channel).Returns(_channelMock.Object);
        _guildMock.SetupGet(g => g.Id).Returns(guildId);
        _guildMock.Setup(g => g.GetMemberAsync(_discordMemberMock.Object.Id)).ReturnsAsync(_discordMemberMock.Object);

        _channelMock.SetupGet(c => c.Guild).Returns(_guildMock.Object);
        _messageMock.SetupGet(m => m.Channel).Returns(_channelMock.Object);
        _messageMock.SetupGet(m => m.Author).Returns(_discordUserMock.Object);

        _lavaLinkServiceMock.SetupGet(l => l.IsRepeating)
            .Returns(isRepeating);

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
        var isRepeatingList = new Dictionary<ulong, bool> { { guildId, true } };
        var isRepeating = new Dictionary<ulong, bool> { { guildId, false } };

        _discordUserMock.SetupGet(du => du.IsBot).Returns(false);
        _discordMemberMock.SetupGet(dm => dm.VoiceState).Returns(_discordVoiceStateMock.Object);

        _discordVoiceStateMock.SetupGet(vs => vs.Channel).Returns(_channelMock.Object);
        _guildMock.SetupGet(g => g.Id).Returns(guildId);
        _guildMock.Setup(g => g.GetMemberAsync(_discordMemberMock.Object.Id)).ReturnsAsync(_discordMemberMock.Object);

        _channelMock.SetupGet(c => c.Guild).Returns(_guildMock.Object);
        _messageMock.SetupGet(m => m.Channel).Returns(_channelMock.Object);
        _messageMock.SetupGet(m => m.Author).Returns(_discordUserMock.Object);

        _lavaLinkServiceMock.SetupGet(l => l.IsRepeatingList)
            .Returns(isRepeatingList);
        _lavaLinkServiceMock.SetupGet(l => l.IsRepeating).Returns(isRepeating);
        _lavaLinkServiceMock.Setup(l => l.GetCurrentTrackList(guildId)).Returns(TestTrackList);
        // Act
        await _repeatListCommand.ExecuteAsync(_messageMock.Object);

        // Assert
        Assert.False(isRepeating[guildId]);
        _responseBuilderMock.Verify(
            r => r.SendSuccessAsync(_messageMock.Object,
                $"{_localizationServiceMock.Object.Get(LocalizationKeys.RepeatListCommandRepeatingOff)}\n {_lavaLinkServiceMock.Object.GetCurrentTrackList(guildId)}"),
            Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_NoRepeat_ShouldEnableRepeat()
    {
        // Arrange
        const ulong guildId = 123456789UL;
        var isRepeatingList = new Dictionary<ulong, bool> { { guildId, false } };
        var isRepeating = new Dictionary<ulong, bool> { { guildId, false } };

        _discordUserMock.SetupGet(du => du.IsBot).Returns(false);
        _discordMemberMock.SetupGet(dm => dm.VoiceState).Returns(_discordVoiceStateMock.Object);

        _discordVoiceStateMock.SetupGet(vs => vs.Channel).Returns(_channelMock.Object);
        _guildMock.SetupGet(g => g.Id).Returns(guildId);
        _guildMock.Setup(g => g.GetMemberAsync(_discordMemberMock.Object.Id)).ReturnsAsync(_discordMemberMock.Object);

        _channelMock.SetupGet(c => c.Guild).Returns(_guildMock.Object);
        _messageMock.SetupGet(m => m.Channel).Returns(_channelMock.Object);
        _messageMock.SetupGet(m => m.Author).Returns(_discordUserMock.Object);

        _lavaLinkServiceMock.SetupGet(l => l.IsRepeating).Returns(isRepeating);
        _lavaLinkServiceMock.SetupGet(l => l.IsRepeatingList).Returns(isRepeatingList);
        _lavaLinkServiceMock.Setup(l => l.GetCurrentTrackList(guildId)).Returns(TestTrackList);

        // Act
        await _repeatListCommand.ExecuteAsync(_messageMock.Object);

        // Assert
        Assert.True(isRepeatingList[guildId]);
        _responseBuilderMock.Verify(
            r => r.SendSuccessAsync(_messageMock.Object,
                $"{_localizationServiceMock.Object.Get(LocalizationKeys.RepeatListCommandRepeatingOn)}\n {_lavaLinkServiceMock.Object.GetCurrentTrackList(guildId)}"),
            Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_UserIsABot_ShouldDoNothing()
    {
        //Arrange
        _discordUserMock.SetupGet(du => du.IsBot).Returns(true);
        _messageMock.SetupGet(m => m.Author).Returns(_discordUserMock.Object);

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

        // Act
        await _repeatListCommand.ExecuteAsync(_messageMock.Object);

        //Assert
        _responseBuilderMock.Verify(r =>
            r.SendValidationErrorAsync(_messageMock.Object, ValidationErrorKeys.UserNotInVoiceChannel));
    }

    [Fact]
    public void Command_Name_And_Description_ShouldReturnCorrectValue_WhenCalled()
    {
        Assert.Equal(RepeatListCommandName, _repeatListCommand.Name);
        Assert.Equal(RepeatListCommandDescriptionValue, _repeatListCommand.Description);
    }
}