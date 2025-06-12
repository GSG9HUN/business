using DC_bot.Commands;
using DC_bot.Interface;
using DC_bot.Service;
using Microsoft.Extensions.Logging;
using Moq;

namespace DC_bot_tests.UnitTests.CommandTests;

public class RepeatCommandTest
{
    private readonly Mock<ILavaLinkService> _lavaLinkServiceMock;
    private readonly Mock<IDiscordMessage> _messageMock;
    private readonly Mock<IDiscordChannel> _channelMock;
    private readonly Mock<IDiscordGuild> _guildMock;
    private readonly Mock<IDiscordUser> _discordUserMock;
    private readonly Mock<IDiscordMember> _discordMemberMock;
    private readonly Mock<IResponseBuilder> _responseBuilderMock;
    private readonly Mock<IDiscordVoiceState> _discordVoiceStateMock;
    private readonly Mock<ILocalizationService> _localizationServiceMock = new();
    private readonly RepeatCommand _repeatCommand;

    public RepeatCommandTest()
    {
        Mock<ILogger<RepeatCommand>> loggerMock = new();
        Mock<ILogger<ValidationService>> validationLoggerMock = new();

        _localizationServiceMock.Setup(g => g.Get("repeat_command_description"))
            .Returns("Repeats a specified track infinitely.");

        _localizationServiceMock.Setup(g => g.Get("repeat_list_command_track_already_repeating"))
            .Returns("This track is already repeating.");

        _localizationServiceMock.Setup(g => g.Get("repeat_command_repeating_on"))
            .Returns("Repeat is on for :");

        _localizationServiceMock.Setup(g => g.Get("repeat_command_repeating_off"))
            .Returns("Repeating is off.");

        _lavaLinkServiceMock = new Mock<ILavaLinkService>();
        _messageMock = new Mock<IDiscordMessage>();
        _channelMock = new Mock<IDiscordChannel>();
        _guildMock = new Mock<IDiscordGuild>();
        _discordUserMock = new Mock<IDiscordUser>();
        _discordMemberMock = new Mock<IDiscordMember>();
        _discordVoiceStateMock = new Mock<IDiscordVoiceState>();
        _responseBuilderMock = new Mock<IResponseBuilder>();

        var userValidationService = new ValidationService(validationLoggerMock.Object);
        _repeatCommand = new RepeatCommand(_lavaLinkServiceMock.Object, userValidationService, loggerMock.Object,
            _responseBuilderMock.Object, _localizationServiceMock.Object);
    }

    [Fact]
    public async Task ExecuteAsync_ListIsAlreadyRepeating_ShouldSendMessage()
    {
        // Arrange
        const ulong guildId = 123456789UL;
        var isRepeatingList = new Dictionary<ulong, bool> { { guildId, true } };

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

        // Act
        await _repeatCommand.ExecuteAsync(_messageMock.Object);

        // Assert
        _responseBuilderMock.Verify(r => r.SendSuccessAsync(_messageMock.Object,
            _localizationServiceMock.Object.Get("repeat_command_list_already_repeating")));
    }

    [Fact]
    public async Task ExecuteAsync_TrackIsRepeating_ShouldTurnOffRepeat()
    {
        // Arrange
        const ulong guildId = 123456789UL;
        var isRepeatingList = new Dictionary<ulong, bool> { { guildId, false } };
        var isRepeating = new Dictionary<ulong, bool> { { guildId, true } };

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

        // Act
        await _repeatCommand.ExecuteAsync(_messageMock.Object);

        // Assert
        Assert.False(isRepeating[guildId]);
        _responseBuilderMock.Verify(
            r => r.SendSuccessAsync(_messageMock.Object,
                $"{_localizationServiceMock.Object.Get("repeat_command_repeating_off")}"), Times.Once);
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
        _lavaLinkServiceMock.Setup(l => l.GetCurrentTrack(guildId)).Returns("Test Track");

        // Act
        await _repeatCommand.ExecuteAsync(_messageMock.Object);

        // Assert
        Assert.True(isRepeating[guildId]);
        _responseBuilderMock.Verify(
            r => r.SendSuccessAsync(_messageMock.Object,
                $"{_localizationServiceMock.Object.Get("repeat_command_repeating_on")} {_lavaLinkServiceMock.Object.GetCurrentTrack(guildId)}"),
            Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_UserIsABot_ShouldDoNothing()
    {
        //Arrange
        _discordUserMock.SetupGet(du => du.IsBot).Returns(true);
        _messageMock.SetupGet(m => m.Author).Returns(_discordUserMock.Object);

        // Act
        await _repeatCommand.ExecuteAsync(_messageMock.Object);

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
        await _repeatCommand.ExecuteAsync(_messageMock.Object);

        //Assert
        _responseBuilderMock.Verify(r =>
            r.SendValidationErrorAsync(_messageMock.Object, "user_not_in_a_voice_channel"));
    }

    [Fact]
    public void Command_Name_And_Description_ShouldReturnCorrectValue_WhenCalled()
    {
        Assert.Equal("repeat", _repeatCommand.Name);
        Assert.Equal("Repeats a specified track infinitely.", _repeatCommand.Description);
    }
}