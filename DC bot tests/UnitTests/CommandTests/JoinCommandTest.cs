using DC_bot.Commands;
using DC_bot.Interface;
using DC_bot.Service;
using Microsoft.Extensions.Logging;
using Moq;

namespace DC_bot_tests.UnitTests.CommandTests;

public class JoinCommandTest
{
    private readonly Mock<ILavaLinkService> _lavaLinkServiceMock;
    private readonly Mock<IDiscordUser> _discordUserMock;
    private readonly Mock<IDiscordMember> _discordMemberMock;
    private readonly Mock<IDiscordGuild> _guildMock;
    private readonly Mock<IDiscordChannel> _channelMock;
    private readonly Mock<IDiscordMessage> _messageMock;
    private readonly JoinCommand _joinCommand;
    private readonly Mock<ILogger<JoinCommand>> _joinCommandLoggerMock;
    
    public JoinCommandTest()
    {
      
        Mock<ILogger<UserValidationService>> userLogger = new();
        Mock<ILocalizationService> localizationServiceMock = new();
        
        localizationServiceMock.Setup(g => g.Get("join_command_description"))
            .Returns("The bot join to your voice channel and start playing queue music if its exists.");
        localizationServiceMock.Setup(g => g.Get("user_not_in_a_voice_channel"))
            .Returns("You must be in a voice channel!");
        
        _lavaLinkServiceMock = new Mock<ILavaLinkService>();
        _messageMock = new Mock<IDiscordMessage>();
        _discordUserMock = new Mock<IDiscordUser>();
        _discordMemberMock = new Mock<IDiscordMember>();
        _guildMock = new Mock<IDiscordGuild>();
        _channelMock = new Mock<IDiscordChannel>();
        _lavaLinkServiceMock = new Mock<ILavaLinkService>();
        _joinCommandLoggerMock = new Mock<ILogger<JoinCommand>>();
        
        var userValidationService = new UserValidationService(userLogger.Object,localizationServiceMock.Object);
        _joinCommand = new JoinCommand(_lavaLinkServiceMock.Object, userValidationService, _joinCommandLoggerMock.Object, localizationServiceMock.Object);
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
        await _joinCommand.ExecuteAsync(_messageMock.Object);

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
        await _joinCommand.ExecuteAsync(_messageMock.Object);

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
    public async Task JoinCommand_Should_Execute_As_Expected()
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
        
        //Act
        await _joinCommand.ExecuteAsync(_messageMock.Object);
        
        //Assert
        _joinCommandLoggerMock.Verify(
            x => x.Log(
                It.Is<LogLevel>(l => l == LogLevel.Information),
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) =>
                    v.ToString()!.Contains("Join command executed!")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()
            ),
            Times.Once
        );
    }
    
    [Fact]
    public void Command_Name_And_Description_ShouldReturnCorrectValue_WhenCalled()
    {
        Assert.Equal("join", _joinCommand.Name);
        Assert.Equal("The bot join to your voice channel and start playing queue music if its exists.", _joinCommand.Description);
    }
}