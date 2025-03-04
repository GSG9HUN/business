using DC_bot.Commands;
using DC_bot.Interface;
using DC_bot.Service;
using Microsoft.Extensions.Logging;
using Moq;

namespace DC_bot_tests.UnitTests.CommandTests;

public class PauseCommandTest
{
    private readonly Mock<ILavaLinkService> _lavaLinkServiceMock;
    private readonly Mock<IDiscordUser> _discordUserMock;
    private readonly Mock<IDiscordMember> _discordMemberMock;
    private readonly Mock<IDiscordGuild> _guildMock;
    private readonly Mock<IDiscordChannel> _channelMock;
    private readonly Mock<IDiscordMessage> _messageMock;
    private readonly PauseCommand _pauseCommand;

    public PauseCommandTest()
    {
        Mock<ILogger<UserValidationService>> userServiceLogger = new();
        Mock<ILogger<PauseCommand>> loggerMock = new();
        
        _messageMock = new Mock<IDiscordMessage>();
        _discordUserMock = new Mock<IDiscordUser>();
        _discordMemberMock = new Mock<IDiscordMember>();
        _guildMock = new Mock<IDiscordGuild>();
        _channelMock = new Mock<IDiscordChannel>();
        _lavaLinkServiceMock = new Mock<ILavaLinkService>();
        
        var userValidationService = new UserValidationService(userServiceLogger.Object);
        _pauseCommand = new PauseCommand(_lavaLinkServiceMock.Object, userValidationService, loggerMock.Object);
    }

    [Fact]
    public async Task ExecuteAsync_UserIsBot_ShouldDoNothing()
    {
        // Arrange
        _discordUserMock.SetupGet(du => du.Id).Returns(123456789L);
        _discordUserMock.SetupGet(du => du.IsBot).Returns(true);

        _guildMock.Setup(g => g.GetMemberAsync(It.IsAny<ulong>())).ReturnsAsync(_discordMemberMock.Object);

        _channelMock.SetupGet(c => c.Guild).Returns(_guildMock.Object);

        _messageMock.SetupGet(m => m.Author).Returns(_discordUserMock.Object);
        _messageMock.SetupGet(m => m.Channel).Returns(_channelMock.Object);

        // Act
        await _pauseCommand.ExecuteAsync(_messageMock.Object);

        // Assert
        _lavaLinkServiceMock.Verify(l => l.PauseAsync(It.IsAny<IDiscordChannel>()), Times.Never);
    }

    [Fact]
    public async Task ExecuteAsync_UserNotInVoiceChannel_ShouldSendErrorMessage()
    {
        // Arrange
        IDiscordVoiceState? mockDiscordVoiceState = null;

        _discordUserMock.SetupGet(du => du.Id).Returns(123456789L);
        _discordUserMock.SetupGet(du => du.IsBot).Returns(false);

        _discordMemberMock.Setup(dm => dm.VoiceState).Returns(mockDiscordVoiceState);

        _guildMock.Setup(g => g.GetMemberAsync(It.IsAny<ulong>())).ReturnsAsync(_discordMemberMock.Object);

        _channelMock.SetupGet(c => c.Guild).Returns(_guildMock.Object);

        _messageMock.SetupGet(m => m.Author).Returns(_discordUserMock.Object);
        _messageMock.SetupGet(m => m.Channel).Returns(_channelMock.Object);

        //Act
        await _pauseCommand.ExecuteAsync(_messageMock.Object);

        // Assert

        _messageMock.Verify(m => m.RespondAsync("You must be in a voice channel!"), Times.Once);
        _lavaLinkServiceMock.Verify(l => l.PauseAsync(It.IsAny<IDiscordChannel>()), Times.Never);
    }

    [Fact]
    public async Task ExecuteAsync_UserInVoiceChannel_ShouldPauseMusic()
    {
        // Arrange
        var voiceChannel = new Mock<IDiscordVoiceState>();

        _discordUserMock.SetupGet(du => du.Id).Returns(123456789L);

        _discordMemberMock.SetupGet(dm => dm.IsBot).Returns(false);
        _discordMemberMock.Setup(dm => dm.VoiceState).Returns(voiceChannel.Object);

        _guildMock.Setup(g => g.GetMemberAsync(It.IsAny<ulong>())).ReturnsAsync(_discordMemberMock.Object);

        _channelMock.SetupGet(c => c.Guild).Returns(_guildMock.Object);

        _messageMock.SetupGet(m => m.Author).Returns(_discordUserMock.Object);
        _messageMock.SetupGet(m => m.Channel).Returns(_channelMock.Object);

        voiceChannel.SetupGet(vc => vc.Channel).Returns(_channelMock.Object);
        // Act
        await _pauseCommand.ExecuteAsync(_messageMock.Object);

        // Assert
        _lavaLinkServiceMock.Verify(l => l.PauseAsync(_channelMock.Object), Times.Once);
    }

    [Fact]
    public void Command_Name_And_Description_ShouldReturnCorrectValue_WhenCalled()
    {
        Assert.Equal("pause", _pauseCommand.Name);
        Assert.Equal("Pause the current music.", _pauseCommand.Description);
    }
}