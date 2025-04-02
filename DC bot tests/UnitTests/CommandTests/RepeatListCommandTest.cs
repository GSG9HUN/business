using DC_bot.Commands;
using DC_bot.Interface;
using DC_bot.Service;
using Microsoft.Extensions.Logging;
using Moq;

namespace DC_bot_tests.UnitTests.CommandTests;

public class RepeatListCommandTest
{
    private readonly Mock<ILavaLinkService> _lavaLinkServiceMock;
    private readonly Mock<IDiscordMessage> _messageMock;
    private readonly Mock<IDiscordChannel> _channelMock;
    private readonly Mock<IDiscordGuild> _guildMock;
    private readonly Mock<IDiscordUser> _discordUserMock;
    private readonly Mock<IDiscordMember> _discordMemberMock;
    private readonly Mock<IDiscordVoiceState> _discordVoiceStateMock;
    private readonly RepeatListCommand _repeatListCommand;

    public RepeatListCommandTest()
    {
        Mock<ILogger<RepeatListCommand>> loggerMock = new();
        Mock<ILogger<ValidationService>> validationLoggerMock = new();
        Mock<ILocalizationService> localizationServiceMock = new();
        
        localizationServiceMock.Setup(g => g.Get("repeat_list_command_description"))
            .Returns("Repeats the current track list.");
        
        localizationServiceMock.Setup(g => g.Get("repeat_list_command_repeating_on"))
            .Returns("Repeat is on for current list:");
        
        localizationServiceMock.Setup(g => g.Get("repeat_list_command_repeating_off"))
            .Returns("Repeating is off for the list:");
        
        localizationServiceMock.Setup(g => g.Get("repeat_list_command_track_already_repeating"))
            .Returns("This track is already repeating.");
        
        localizationServiceMock.Setup(g => g.Get("user_not_in_a_voice_channel"))
            .Returns("You must be in a voice channel!");
        
        _discordUserMock = new Mock<IDiscordUser>();
        _discordMemberMock = new Mock<IDiscordMember>();
        _discordVoiceStateMock = new Mock<IDiscordVoiceState>();
        _lavaLinkServiceMock = new Mock<ILavaLinkService>();
        _messageMock = new Mock<IDiscordMessage>();
        _channelMock = new Mock<IDiscordChannel>();
        _guildMock = new Mock<IDiscordGuild>();
        
        var userValidationService = new ValidationService(localizationServiceMock.Object,validationLoggerMock.Object);
        _repeatListCommand =
            new RepeatListCommand(_lavaLinkServiceMock.Object, userValidationService, loggerMock.Object,localizationServiceMock.Object);
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
        _channelMock.Verify(c => c.SendMessageAsync("This track is already repeating."), Times.Once);
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
        _lavaLinkServiceMock.Setup(l => l.GetCurrentTrackList(guildId)).Returns("test track list");
        // Act
        await _repeatListCommand.ExecuteAsync(_messageMock.Object);

        // Assert
        Assert.False(isRepeating[guildId]);
        _channelMock.Verify(c => c.SendMessageAsync("Repeating is off for the list:\n test track list"), Times.Once);
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
        _lavaLinkServiceMock.Setup(l => l.GetCurrentTrackList(guildId)).Returns("test track list");

        // Act
        await _repeatListCommand.ExecuteAsync(_messageMock.Object);

        // Assert
        Assert.True(isRepeatingList[guildId]);
        _channelMock.Verify(c => c.SendMessageAsync("Repeat is on for current list:\n test track list"), Times.Once);
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
        _messageMock.Verify(m => m.RespondAsync(It.IsAny<string>()),Times.Never);
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
        _messageMock.Verify(m => m.RespondAsync("You must be in a voice channel!"),Times.Once);
    }

    [Fact]
    public void Command_Name_And_Description_ShouldReturnCorrectValue_WhenCalled()
    {
        Assert.Equal("repeatList", _repeatListCommand.Name);
        Assert.Equal("Repeats the current track list.", _repeatListCommand.Description);
    }
}