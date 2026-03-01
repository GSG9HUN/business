using DC_bot.Commands;
using DC_bot.Constants;
using DC_bot.Helper;
using DC_bot.Interface;
using DC_bot.Service;
using Lavalink4NET.Rest.Entities.Tracks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;

namespace DC_bot_tests.UnitTests.CommandTests;

public class PlayCommandTest
{
    private const string PlayCommandName = "play";
    private const string PlayCommandDescriptionValue = "Start playing a music.";
    private const string PlayCommandContentNoArgs = "!play";
    private const string PlayCommandContentYouTube = "!play https://www.youtube.com/watch?v=zIRszCXKzGc";
    private const string PlayCommandContentSoundCloud = "!play https://soundcloud.com/madeon/imperium";
    private const string PlayCommandContentSpotify = "!play https://open.spotify.com/track/0DI3WNmIyfi2fS3a15wZDP";
    private const string PlayCommandContentSpotifyAlt = "!play https://open.spotify.com/track/7ouMYWpwJ422jRcDASZB7P";
    private const string PlayCommandContentAppleMusic = "!play https://music.apple.com/us/album/bad-guy/1450695723?i=1450695739";
    private const string PlayCommandContentDeezer = "!play https://www.deezer.com/track/3135556";
    private const string PlayCommandContentYandex = "!play https://music.yandex.ru/album/12345/track/67890";
    private const string PlayCommandContentYouTubeMusic = "!play https://music.youtube.com/watch?v=zIRszCXKzGc";
    private const string PlayCommandContentSearch = "!play madeon imperium";
    private const string PlayCommandContentSearchAlt = "!play legjobb mix ever";
    private const string SearchModeDefault = "yt";

    private readonly Mock<ILavaLinkService> _lavaLinkServiceMock;
    private readonly Mock<IResponseBuilder> _responseBuilderMock;
    private readonly Mock<IDiscordUser> _discordUserMock;
    private readonly Mock<IDiscordMember> _discordMemberMock;
    private readonly Mock<IDiscordGuild> _guildMock;
    private readonly Mock<IDiscordChannel> _channelMock;
    private readonly Mock<IDiscordMessage> _messageMock;
    private readonly PlayCommand _playCommand;

    public PlayCommandTest()
    {
        Mock<ILogger<PlayCommand>> loggerMock = new();
        Mock<ILogger<ValidationService>> validationLoggerMock = new();
        Mock<ILocalizationService> localizationServiceMock = new();

        localizationServiceMock.Setup(g => g.Get(LocalizationKeys.PlayCommandDescription))
            .Returns(PlayCommandDescriptionValue);

        _responseBuilderMock = new Mock<IResponseBuilder>();
        _messageMock = new Mock<IDiscordMessage>();
        _discordUserMock = new Mock<IDiscordUser>();
        _discordMemberMock = new Mock<IDiscordMember>();
        _guildMock = new Mock<IDiscordGuild>();
        _channelMock = new Mock<IDiscordChannel>();
        _lavaLinkServiceMock = new Mock<ILavaLinkService>();
        var options = new SearchResolverOptions { DefaultQueryMode = SearchModeDefault };
        var wrapped = Options.Create(options);
        var trackSearchResolverServiceMock = new TrackSearchResolverService(wrapped);

        var userValidationService = new ValidationService(validationLoggerMock.Object);
        _playCommand = new PlayCommand(_lavaLinkServiceMock.Object, userValidationService, _responseBuilderMock.Object,
            loggerMock.Object, trackSearchResolverServiceMock, localizationServiceMock.Object);
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
        await _playCommand.ExecuteAsync(_messageMock.Object);

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
        _messageMock.SetupGet(m => m.Content).Returns(PlayCommandContentNoArgs);

        //Act
        await _playCommand.ExecuteAsync(_messageMock.Object);

        //Assert
        _responseBuilderMock.Verify(r => r.SendUsageAsync(_messageMock.Object, PlayCommandName), Times.Once);
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
    public async Task ExecuteAsync_UserProvidedURL_ShouldCall_PlayAsyncURL_With_Youtube_Search_Mode()
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
        _messageMock.SetupGet(m => m.Content).Returns(PlayCommandContentYouTube);

        //Act
        await _playCommand.ExecuteAsync(_messageMock.Object);

        //Assert
        _lavaLinkServiceMock.Verify(
            l => l.PlayAsyncQuery(It.IsAny<IDiscordChannel>(), It.IsAny<string>(), It.IsAny<IDiscordMessage>(),
                TrackSearchMode.YouTube),
            Times.Never);
        _lavaLinkServiceMock.Verify(
            l => l.PlayAsyncUrl(It.IsAny<IDiscordChannel>(), It.IsAny<Uri>(), It.IsAny<IDiscordMessage>(),
                TrackSearchMode.YouTube), Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_UserProvidedURL_ShouldCall_PlayAsyncURL_With_SoundCloud_Search_Mode()
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
        _messageMock.SetupGet(m => m.Content).Returns(PlayCommandContentSoundCloud);

        //Act
        await _playCommand.ExecuteAsync(_messageMock.Object);

        //Assert
        _lavaLinkServiceMock.Verify(
            l => l.PlayAsyncQuery(It.IsAny<IDiscordChannel>(), It.IsAny<string>(), It.IsAny<IDiscordMessage>(),
                TrackSearchMode.SoundCloud),
            Times.Never);
        _lavaLinkServiceMock.Verify(
            l => l.PlayAsyncUrl(It.IsAny<IDiscordChannel>(), It.IsAny<Uri>(), It.IsAny<IDiscordMessage>(),
                TrackSearchMode.SoundCloud), Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_UserProvidedURL_ShouldCall_PlayAsyncURL_With_Spotify_Search_Mode()
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
        _messageMock.SetupGet(m => m.Content).Returns(PlayCommandContentSpotifyAlt);

        //Act
        await _playCommand.ExecuteAsync(_messageMock.Object);

        //Assert
        _lavaLinkServiceMock.Verify(
            l => l.PlayAsyncQuery(It.IsAny<IDiscordChannel>(), It.IsAny<string>(), It.IsAny<IDiscordMessage>(),
                TrackSearchMode.Spotify),
            Times.Never);
        _lavaLinkServiceMock.Verify(
            l => l.PlayAsyncUrl(It.IsAny<IDiscordChannel>(), It.IsAny<Uri>(), It.IsAny<IDiscordMessage>(),
                TrackSearchMode.Spotify), Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_UserProvidedURL_ShouldCall_PlayAsyncURL_With_AppleMusic_Search_Mode()
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
        _messageMock.SetupGet(m => m.Content).Returns(PlayCommandContentAppleMusic);

        //Act
        await _playCommand.ExecuteAsync(_messageMock.Object);

        //Assert
        _lavaLinkServiceMock.Verify(
            l => l.PlayAsyncQuery(It.IsAny<IDiscordChannel>(), It.IsAny<string>(), It.IsAny<IDiscordMessage>(),
                TrackSearchMode.AppleMusic),
            Times.Never);
        _lavaLinkServiceMock.Verify(
            l => l.PlayAsyncUrl(It.IsAny<IDiscordChannel>(), It.IsAny<Uri>(), It.IsAny<IDiscordMessage>(),
                TrackSearchMode.AppleMusic), Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_UserProvidedURL_ShouldCall_PlayAsyncURL_With_Deezer_Search_Mode()
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
        _messageMock.SetupGet(m => m.Content).Returns(PlayCommandContentDeezer);

        //Act
        await _playCommand.ExecuteAsync(_messageMock.Object);

        //Assert
        _lavaLinkServiceMock.Verify(
            l => l.PlayAsyncQuery(It.IsAny<IDiscordChannel>(), It.IsAny<string>(), It.IsAny<IDiscordMessage>(),
                TrackSearchMode.Deezer),
            Times.Never);
        _lavaLinkServiceMock.Verify(
            l => l.PlayAsyncUrl(It.IsAny<IDiscordChannel>(), It.IsAny<Uri>(), It.IsAny<IDiscordMessage>(),
                TrackSearchMode.Deezer), Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_UserProvidedURL_ShouldCall_PlayAsyncURL_With_YandexMusic_Search_Mode()
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
        _messageMock.SetupGet(m => m.Content).Returns(PlayCommandContentYandex);

        //Act
        await _playCommand.ExecuteAsync(_messageMock.Object);

        //Assert
        _lavaLinkServiceMock.Verify(
            l => l.PlayAsyncQuery(It.IsAny<IDiscordChannel>(), It.IsAny<string>(), It.IsAny<IDiscordMessage>(),
                TrackSearchMode.YandexMusic),
            Times.Never);
        _lavaLinkServiceMock.Verify(
            l => l.PlayAsyncUrl(It.IsAny<IDiscordChannel>(), It.IsAny<Uri>(), It.IsAny<IDiscordMessage>(),
                TrackSearchMode.YandexMusic), Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_UserProvidedURL_ShouldCall_PlayAsyncURL_With_YoutubeMusic_Search_Mode()
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
        _messageMock.SetupGet(m => m.Content).Returns(PlayCommandContentYouTubeMusic);

        //Act
        await _playCommand.ExecuteAsync(_messageMock.Object);

        //Assert
        _lavaLinkServiceMock.Verify(
            l => l.PlayAsyncQuery(It.IsAny<IDiscordChannel>(), It.IsAny<string>(), It.IsAny<IDiscordMessage>(),
                TrackSearchMode.YouTubeMusic),
            Times.Never);
        _lavaLinkServiceMock.Verify(
            l => l.PlayAsyncUrl(It.IsAny<IDiscordChannel>(), It.IsAny<Uri>(), It.IsAny<IDiscordMessage>(),
                TrackSearchMode.YouTubeMusic), Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_UserProvidedTitle_ShouldCall_PlayAsyncQuery_With_Youtube_Search_Mode()
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
        _messageMock.SetupGet(m => m.Content).Returns(PlayCommandContentSearchAlt);

        //Act
        await _playCommand.ExecuteAsync(_messageMock.Object);

        //Assert
        _lavaLinkServiceMock.Verify(
            l => l.PlayAsyncQuery(It.IsAny<IDiscordChannel>(), It.IsAny<string>(), It.IsAny<IDiscordMessage>(),
                TrackSearchMode.YouTube),
            Times.Once);
        _lavaLinkServiceMock.Verify(
            l => l.PlayAsyncUrl(It.IsAny<IDiscordChannel>(), It.IsAny<Uri>(), It.IsAny<IDiscordMessage>(),
                TrackSearchMode.YouTube),
            Times.Never);
    }

    [Fact]
    public void Command_Name_And_Description_ShouldReturnCorrectValue_WhenCalled()
    {
        Assert.Equal(PlayCommandName, _playCommand.Name);
        Assert.Equal(PlayCommandDescriptionValue, _playCommand.Description);
    }
}