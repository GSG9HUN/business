using DC_bot.Commands;
using DC_bot.Constants;
using DC_bot.Interface;
using DC_bot.Service;
using Lavalink4NET.Rest.Entities.Tracks;
using Microsoft.Extensions.Logging;
using Moq;

namespace DC_bot_tests.UnitTests.CommandTests;

public class JoinCommandTest
{
    private const string JoinCommandName = "join";
    private const string JoinCommandDescriptionValue = "The bot join to your voice channel and start playing queue music if its exists.";

    private readonly Mock<ILavaLinkService> _lavaLinkServiceMock;
    private readonly Mock<IResponseBuilder> _responseBuilderMock;
    private readonly Mock<IDiscordUser> _discordUserMock;
    private readonly Mock<IDiscordMember> _discordMemberMock;
    private readonly Mock<IDiscordGuild> _guildMock;
    private readonly Mock<IDiscordChannel> _channelMock;
    private readonly Mock<IDiscordMessage> _messageMock;
    private readonly JoinCommand _joinCommand;
    private readonly Mock<ILogger<JoinCommand>> _joinCommandLoggerMock;

    public JoinCommandTest()
    {
        Mock<ILogger<ValidationService>> validationLoggerMock = new();
        Mock<ILocalizationService> localizationServiceMock = new();

        localizationServiceMock.Setup(g => g.Get(LocalizationKeys.JoinCommandDescription))
            .Returns(JoinCommandDescriptionValue);

        _lavaLinkServiceMock = new Mock<ILavaLinkService>();
        _messageMock = new Mock<IDiscordMessage>();
        _discordUserMock = new Mock<IDiscordUser>();
        _discordMemberMock = new Mock<IDiscordMember>();
        _guildMock = new Mock<IDiscordGuild>();
        _channelMock = new Mock<IDiscordChannel>();
        _lavaLinkServiceMock = new Mock<ILavaLinkService>();
        _joinCommandLoggerMock = new Mock<ILogger<JoinCommand>>();
        _responseBuilderMock = new Mock<IResponseBuilder>();

        var userValidationService = new ValidationService(validationLoggerMock.Object);
        _joinCommand = new JoinCommand(_lavaLinkServiceMock.Object, userValidationService,
            _joinCommandLoggerMock.Object, _responseBuilderMock.Object, localizationServiceMock.Object);
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
            l => l.PlayAsyncQuery(It.IsAny<IDiscordChannel>(), It.IsAny<string>(), It.IsAny<IDiscordMessage>(),
                It.IsAny<TrackSearchMode>()),
            Times.Never);
        _lavaLinkServiceMock.Verify(
            l => l.PlayAsyncUrl(It.IsAny<IDiscordChannel>(), It.IsAny<Uri>(), It.IsAny<IDiscordMessage>(),
                It.IsAny<TrackSearchMode>()),
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
        _responseBuilderMock.Verify(r => r.SendValidationErrorAsync(_messageMock.Object, ValidationErrorKeys.UserNotInVoiceChannel),
            Times.Once);
        _lavaLinkServiceMock.Verify(
            l => l.PlayAsyncQuery(It.IsAny<IDiscordChannel>(), It.IsAny<string>(), It.IsAny<IDiscordMessage>(),
                It.IsAny<TrackSearchMode>()),
            Times.Never);
        _lavaLinkServiceMock.Verify(
            l => l.PlayAsyncUrl(It.IsAny<IDiscordChannel>(), It.IsAny<Uri>(), It.IsAny<IDiscordMessage>(),
                It.IsAny<TrackSearchMode>()),
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
        Assert.Equal(JoinCommandName, _joinCommand.Name);
        Assert.Equal(JoinCommandDescriptionValue, _joinCommand.Description);
    }
}