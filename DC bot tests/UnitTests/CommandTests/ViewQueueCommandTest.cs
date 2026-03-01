using System.Collections.Generic;
using System.Threading.Tasks;
using DC_bot.Commands;
using DC_bot.Interface;
using DC_bot.Service;
using DSharpPlus.Entities;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace DC_bot_tests.UnitTests.CommandTests;

public class ViewQueueCommandTest
{
    private readonly Mock<ILavaLinkService> _lavaLinkServiceMock;
    private readonly Mock<IDiscordUser> _discordUserMock;
    private readonly Mock<IDiscordMember> _discordMemberMock;
    private readonly Mock<IDiscordGuild> _guildMock;
    private readonly Mock<IDiscordChannel> _channelMock;
    private readonly Mock<IDiscordMessage> _messageMock;
    private readonly ViewQueueCommand _viewQueueCommand;
    private readonly Mock<IResponseBuilder> _responseBuilderMock;

    public ViewQueueCommandTest()
    {
        Mock<ILogger<ViewQueueCommand>> loggerMock = new();
        Mock<ILogger<ValidationService>> validationLoggerMock = new();
        Mock<ILocalizationService> localizationServiceMock = new();

        localizationServiceMock.Setup(g => g.Get("view_list_command_description"))
            .Returns("View the list of tracks.");

        localizationServiceMock.Setup(g => g.Get("view_list_command_embed_title"))
            .Returns("Playlist");

        localizationServiceMock.Setup(g => g.Get("view_list_command_response", 1))
            .Returns("... and more 1 track.");

        _messageMock = new Mock<IDiscordMessage>();
        _discordUserMock = new Mock<IDiscordUser>();
        _discordMemberMock = new Mock<IDiscordMember>();
        _guildMock = new Mock<IDiscordGuild>();
        _channelMock = new Mock<IDiscordChannel>();
        _lavaLinkServiceMock = new Mock<ILavaLinkService>();
        _responseBuilderMock = new Mock<IResponseBuilder>();

        var userValidationService = new ValidationService(validationLoggerMock.Object);

        _viewQueueCommand =
            new ViewQueueCommand(_lavaLinkServiceMock.Object, userValidationService, loggerMock.Object,
                _responseBuilderMock.Object,
                localizationServiceMock.Object);
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
        _responseBuilderMock.Verify(r => r.SendValidationErrorAsync(_messageMock.Object, "user_not_in_a_voice_channel"),
            Times.Once);
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
        _responseBuilderMock.Verify(r => r.SendCommandErrorResponse(_messageMock.Object, "view_list"), Times.Once);
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
        _messageMock.Verify(m => m.RespondAsync(It.Is<DiscordEmbed>(embed =>
            embed.Title == "Playlist" &&
            embed.Fields.Count == 1 &&
            embed.Fields[0].Name == "Test title" &&
            embed.Fields[0].Value == "🎵 Test author"
        )), Times.Once);
        _lavaLinkServiceMock.Verify(l => l.ViewQueue(It.IsAny<ulong>()), Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_Should_Display_Footer_When_Queue_Has_More_Than_10_Tracks()
    {
        // Arrange
        var discordVoiceStateMock = new Mock<IDiscordVoiceState>();
        discordVoiceStateMock.Setup(vs => vs.Channel).Returns(_channelMock.Object);

        var tracks = new List<ILavaLinkTrack>();

        for (int i = 1; i <= 11; i++)
        {
            var trackMock = new Mock<ILavaLinkTrack>();
            trackMock.SetupGet(t => t.Author).Returns($"Author {i}");
            trackMock.SetupGet(t => t.Title).Returns($"Title {i}");
            tracks.Add(trackMock.Object);
        }

        _discordUserMock.Setup(du => du.Id).Returns(1564123L);
        _discordMemberMock.Setup(dm => dm.IsBot).Returns(false);
        _discordMemberMock.SetupGet(dm => dm.VoiceState).Returns(discordVoiceStateMock.Object);
        _guildMock.Setup(g => g.GetMemberAsync(It.IsAny<ulong>())).ReturnsAsync(_discordMemberMock.Object);
        _channelMock.SetupGet(c => c.Guild).Returns(_guildMock.Object);
        _messageMock.SetupGet(m => m.Author).Returns(_discordUserMock.Object);
        _messageMock.Setup(m => m.Channel).Returns(_channelMock.Object);

        _lavaLinkServiceMock.Setup(l => l.ViewQueue(It.IsAny<ulong>())).Returns(tracks);

        // Act
        await _viewQueueCommand.ExecuteAsync(_messageMock.Object);

        // Assert
        _messageMock.Verify(m => m.RespondAsync(It.Is<DiscordEmbed>(embed =>
                embed.Title == "Playlist" &&
                embed.Fields.Count == 10 && // Csak 10 track-et kell megjelenítenie
                embed.Fields[0].Name == "Title 1" &&
                embed.Fields[0].Value == "🎵 Author 1" &&
                embed.Fields[9].Name == "Title 10" &&
                embed.Fields[9].Value == "🎵 Author 10" &&
                embed.Footer.Text == "... and more 1 track." // Footerben jelezze, hogy még egy track van
        )), Times.Once);

        _lavaLinkServiceMock.Verify(l => l.ViewQueue(It.IsAny<ulong>()), Times.Once);
    }

    [Fact]
    public void Command_Name_And_Description_ShouldReturnCorrectValue_WhenCalled()
    {
        Assert.Equal("viewList", _viewQueueCommand.Name);
        Assert.Equal("View the list of tracks.", _viewQueueCommand.Description);
    }
}