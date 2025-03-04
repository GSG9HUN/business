using DC_bot.Commands;
using DC_bot.Interface;
using DC_bot.Service;
using Microsoft.Extensions.Logging;
using Moq;

namespace DC_bot_tests.UnitTests.CommandTests;

public class PlayCommandTest
{
    private readonly Mock<ILavaLinkService> _lavaLinkServiceMock;
    private readonly Mock<IDiscordUser> _discordUserMock;
    private readonly Mock<IDiscordMember> _discordMemberMock;
    private readonly Mock<IDiscordGuild> _guildMock;
    private readonly Mock<IDiscordChannel> _channelMock;
    private readonly Mock<IDiscordMessage> _messageMock;
    private readonly PlayCommand _playCommand;

    public PlayCommandTest()
    {
        Mock<ILogger<PlayCommand>> loggerMock = new();
        Mock<ILogger<UserValidationService>> userLogger = new();
        
        _lavaLinkServiceMock = new Mock<ILavaLinkService>();
        _messageMock = new Mock<IDiscordMessage>();
        _discordUserMock = new Mock<IDiscordUser>();
        _discordMemberMock = new Mock<IDiscordMember>();
        _guildMock = new Mock<IDiscordGuild>();
        _channelMock = new Mock<IDiscordChannel>();
        _lavaLinkServiceMock = new Mock<ILavaLinkService>();
        
        var userValidationService = new UserValidationService(userLogger.Object);
        _playCommand = new PlayCommand(_lavaLinkServiceMock.Object, userValidationService, loggerMock.Object);
    }

    [Fact]
    public async Task ExecuteAsync_UserIsBot_ShouldDoNothing()
    {
        //Arrange
        _discordUserMock.Setup(du => du.Id).Returns(1564123L);
        _discordMemberMock.Setup(dm => dm.IsBot).Returns(true);
        _guildMock.Setup(g => g.GetMemberAsync(It.IsAny<ulong>())).ReturnsAsync(_discordMemberMock.Object);
        _channelMock.SetupGet(c => c.Guild).Returns(_guildMock.Object);
        _messageMock.SetupGet(m => m.Author).Returns(_discordUserMock.Object);
        _messageMock.Setup(m => m.Channel).Returns(_channelMock.Object);

        //Act
        await _playCommand.ExecuteAsync(_messageMock.Object);

        //Assert

        _lavaLinkServiceMock.Verify(
            l => l.PlayAsyncQuery(It.IsAny<IDiscordChannel>(), It.IsAny<string>(), It.IsAny<IDiscordChannel>()),
            Times.Never);
        _lavaLinkServiceMock.Verify(
            l => l.PlayAsyncUrl(It.IsAny<IDiscordChannel>(), It.IsAny<Uri>(), It.IsAny<IDiscordChannel>()),
            Times.Never);
    }

    [Fact]
    public async Task ExecuteAsync_UserNotIn_VoiceChannel()
    {
        //Arrange
        _discordUserMock.Setup(du => du.Id).Returns(1564123L);
        _discordMemberMock.Setup(dm => dm.IsBot).Returns(false);
        _discordMemberMock.SetupGet(dm => dm.VoiceState).Returns((IDiscordVoiceState?)null);
        _guildMock.Setup(g => g.GetMemberAsync(It.IsAny<ulong>())).ReturnsAsync(_discordMemberMock.Object);
        _channelMock.SetupGet(c => c.Guild).Returns(_guildMock.Object);
        _messageMock.SetupGet(m => m.Author).Returns(_discordUserMock.Object);
        _messageMock.Setup(m => m.Channel).Returns(_channelMock.Object);

        //Act
        await _playCommand.ExecuteAsync(_messageMock.Object);

        //Assert
        _messageMock.Verify(m => m.RespondAsync("You must be in a voice channel!"), Times.Once);
        _lavaLinkServiceMock.Verify(
            l => l.PlayAsyncQuery(It.IsAny<IDiscordChannel>(), It.IsAny<string>(), It.IsAny<IDiscordChannel>()),
            Times.Never);
        _lavaLinkServiceMock.Verify(
            l => l.PlayAsyncUrl(It.IsAny<IDiscordChannel>(), It.IsAny<Uri>(), It.IsAny<IDiscordChannel>()),
            Times.Never);
    }

    [Fact]
    public async Task ExecuteAsync_UserNotProvided_URL_Or_Title()
    {
        //Arrange
        var discordVoiceStateMock = new Mock<IDiscordVoiceState>();
        discordVoiceStateMock.Setup(vs => vs.Channel).Returns(_channelMock.Object);
        
        _discordUserMock.Setup(du => du.Id).Returns(1564123L);
        _discordMemberMock.Setup(dm => dm.IsBot).Returns(false);
        _discordMemberMock.SetupGet(dm => dm.VoiceState).Returns(discordVoiceStateMock.Object);
        _guildMock.Setup(g => g.GetMemberAsync(It.IsAny<ulong>())).ReturnsAsync(_discordMemberMock.Object);
        _channelMock.SetupGet(c => c.Guild).Returns(_guildMock.Object);
        _messageMock.SetupGet(m => m.Author).Returns(_discordUserMock.Object);
        _messageMock.Setup(m => m.Channel).Returns(_channelMock.Object);
        _messageMock.SetupGet(m => m.Content).Returns("!play");

        //Act
        await _playCommand.ExecuteAsync(_messageMock.Object);

        //Assert
        _messageMock.Verify(m => m.RespondAsync("Please provide URL."), Times.Once);
        _lavaLinkServiceMock.Verify(
            l => l.PlayAsyncQuery(It.IsAny<IDiscordChannel>(), It.IsAny<string>(), It.IsAny<IDiscordChannel>()),
            Times.Never);
        _lavaLinkServiceMock.Verify(
            l => l.PlayAsyncUrl(It.IsAny<IDiscordChannel>(), It.IsAny<Uri>(), It.IsAny<IDiscordChannel>()),
            Times.Never);
    }

    [Fact]
    public async Task ExecuteAsync_UserProvidedURL_ShouldCall_PlayAsyncURL()
    {
        //Arrange
        var discordVoiceStateMock = new Mock<IDiscordVoiceState>();
        discordVoiceStateMock.Setup(vs => vs.Channel).Returns(_channelMock.Object);
        
        _discordUserMock.Setup(du => du.Id).Returns(1564123L);
        _discordMemberMock.Setup(dm => dm.IsBot).Returns(false);
        _discordMemberMock.SetupGet(dm => dm.VoiceState).Returns(discordVoiceStateMock.Object);
        _guildMock.Setup(g => g.GetMemberAsync(It.IsAny<ulong>())).ReturnsAsync(_discordMemberMock.Object);
        _channelMock.SetupGet(c => c.Guild).Returns(_guildMock.Object);
        _messageMock.SetupGet(m => m.Author).Returns(_discordUserMock.Object);
        _messageMock.Setup(m => m.Channel).Returns(_channelMock.Object);
        _messageMock.SetupGet(m => m.Content).Returns("!play https://www.youtube.com/watch?v=zIRszCXKzGc");

        //Act
        await _playCommand.ExecuteAsync(_messageMock.Object);

        //Assert
        _lavaLinkServiceMock.Verify(
            l => l.PlayAsyncQuery(It.IsAny<IDiscordChannel>(), It.IsAny<string>(), It.IsAny<IDiscordChannel>()),
            Times.Never);
        _lavaLinkServiceMock.Verify(
            l => l.PlayAsyncUrl(It.IsAny<IDiscordChannel>(), It.IsAny<Uri>(), It.IsAny<IDiscordChannel>()), Times.Once);
    }


    [Fact]
    public async Task ExecuteAsync_UserProvidedTitle_ShouldCall_PlayAsyncQuery()
    {
        //Arrange
        var discordVoiceStateMock = new Mock<IDiscordVoiceState>();
        discordVoiceStateMock.Setup(vs => vs.Channel).Returns(_channelMock.Object);
        
        _discordUserMock.Setup(du => du.Id).Returns(1564123L);
        _discordMemberMock.Setup(dm => dm.IsBot).Returns(false);
        _discordMemberMock.SetupGet(dm => dm.VoiceState).Returns(discordVoiceStateMock.Object);
        _guildMock.Setup(g => g.GetMemberAsync(It.IsAny<ulong>())).ReturnsAsync(_discordMemberMock.Object);
        _channelMock.SetupGet(c => c.Guild).Returns(_guildMock.Object);
        _messageMock.SetupGet(m => m.Author).Returns(_discordUserMock.Object);
        _messageMock.Setup(m => m.Channel).Returns(_channelMock.Object);
        _messageMock.SetupGet(m => m.Content).Returns("!play legjobb mix ever");

        //Act
        await _playCommand.ExecuteAsync(_messageMock.Object);

        //Assert
        _lavaLinkServiceMock.Verify(
            l => l.PlayAsyncQuery(It.IsAny<IDiscordChannel>(), It.IsAny<string>(), It.IsAny<IDiscordChannel>()),
            Times.Once);
        _lavaLinkServiceMock.Verify(
            l => l.PlayAsyncUrl(It.IsAny<IDiscordChannel>(), It.IsAny<Uri>(), It.IsAny<IDiscordChannel>()),
            Times.Never);
    }

    [Fact]
    public void Command_Name_And_Description_ShouldReturnCorrectValue_WhenCalled()
    {
        Assert.Equal("play", _playCommand.Name);
        Assert.Equal("Start playing a music.", _playCommand.Description);
    }
}