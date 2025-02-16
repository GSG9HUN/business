using DC_bot.Commands;
using DC_bot.Interface;
using DC_bot.Service;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace DC_bot.Tests.UnitTests.CommandTests;

public class ViewQueueCommandTest
{
    private readonly Mock<ILavaLinkService> _lavaLinkServiceMock;
    private readonly Mock<IDiscordUser> _discordUserMock;
    private readonly Mock<IDiscordMember> _discordMemberMock;
    private readonly Mock<IDiscordGuild> _guildMock;
    private readonly Mock<IDiscordChannel> _channelMock;
    private readonly Mock<IDiscordMessage> _messageMock;
    private readonly ViewQueueCommand _viewQueueCommand;
    
    public ViewQueueCommandTest()
    {
        Mock<ILogger<ViewQueueCommand>> loggerMock = new();
        Mock<ILogger<UserValidationService>> userLoggerMock = new();

        _messageMock = new Mock<IDiscordMessage>();
        _discordUserMock = new Mock<IDiscordUser>();
        _discordMemberMock = new Mock<IDiscordMember>();
        _guildMock = new Mock<IDiscordGuild>();
        _channelMock = new Mock<IDiscordChannel>();
        _lavaLinkServiceMock = new Mock<ILavaLinkService>();
        var userValidationService = new UserValidationService(userLoggerMock.Object);
        
        _viewQueueCommand =
            new ViewQueueCommand(_lavaLinkServiceMock.Object, userValidationService, loggerMock.Object);
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
        await _viewQueueCommand.ExecuteAsync(_messageMock.Object);

        //Assert

        _lavaLinkServiceMock.Verify(l => l.ViewQueue(It.IsAny<ulong>()), Times.Never);
    }

    [Fact]
    public async Task ExecuteAsync_UserNotIn_VoiceChannel()
    {
        //Arrange
        _discordUserMock.Setup(du => du.Id).Returns(1564123L);
        _discordMemberMock.Setup(dm => dm.IsBot).Returns(false);
        _discordMemberMock.SetupGet(dm => dm.VoiceState).Returns((IDiscordVoiceState)null!);
        _guildMock.Setup(g => g.GetMemberAsync(It.IsAny<ulong>())).ReturnsAsync(_discordMemberMock.Object);
        _channelMock.SetupGet(c => c.Guild).Returns(_guildMock.Object);
        _messageMock.SetupGet(m => m.Author).Returns(_discordUserMock.Object);
        _messageMock.Setup(m => m.Channel).Returns(_channelMock.Object);

        //Act
        await _viewQueueCommand.ExecuteAsync(_messageMock.Object);

        //Assert
        _messageMock.Verify(m => m.RespondAsync("You must be in a voice channel!"), Times.Once);
        _lavaLinkServiceMock.Verify(l => l.ViewQueue(It.IsAny<ulong>()), Times.Never);
    }


    [Fact]
    public async Task ExecuteAsync_Queue_Is_Empty_VoiceChannel()
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

        _lavaLinkServiceMock.Setup(l => l.ViewQueue(It.IsAny<ulong>())).Returns(new List<ILavaLinkTrack>());

        //Act
        await _viewQueueCommand.ExecuteAsync(_messageMock.Object);

        //Assert
        _messageMock.Verify(m => m.RespondAsync("The queue is currently empty."), Times.Once);
        _lavaLinkServiceMock.Verify(l => l.ViewQueue(It.IsAny<ulong>()), Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_Should_Run_Correctly()
    {
        var discordVoiceStateMock = new Mock<IDiscordVoiceState>();
        discordVoiceStateMock.Setup(vs => vs.Channel).Returns(_channelMock.Object);

        var lavaLinkTrackMock = new Mock<ILavaLinkTrack>();
        lavaLinkTrackMock.SetupGet(t => t.Author).Returns("Test author");
        lavaLinkTrackMock.SetupGet(t => t.Title).Returns("Test title");

        _discordUserMock.Setup(du => du.Id).Returns(1564123L);
        _discordMemberMock.Setup(dm => dm.IsBot).Returns(false);
        _discordMemberMock.SetupGet(dm => dm.VoiceState).Returns(discordVoiceStateMock.Object);
        _guildMock.Setup(g => g.GetMemberAsync(It.IsAny<ulong>())).ReturnsAsync(_discordMemberMock.Object);
        _channelMock.SetupGet(c => c.Guild).Returns(_guildMock.Object);
        _messageMock.SetupGet(m => m.Author).Returns(_discordUserMock.Object);
        _messageMock.Setup(m => m.Channel).Returns(_channelMock.Object);

        _lavaLinkServiceMock.Setup(l => l.ViewQueue(It.IsAny<ulong>()))
            .Returns(new List<ILavaLinkTrack> { lavaLinkTrackMock.Object });

        //Act
        await _viewQueueCommand.ExecuteAsync(_messageMock.Object);

        //Assert
        _messageMock.Verify(m => m.RespondAsync("Current Queue:\n1. Test title (Test author)"), Times.Once);
        _lavaLinkServiceMock.Verify(l => l.ViewQueue(It.IsAny<ulong>()), Times.Once);
    }
    
    [Fact]
    public void Command_Name_And_Description_ShouldReturnCorrectValue_WhenCalled()
    {
        Assert.Equal("viewList", _viewQueueCommand.Name);
        Assert.Equal("view the list of tracks.", _viewQueueCommand.Description);
    }
}