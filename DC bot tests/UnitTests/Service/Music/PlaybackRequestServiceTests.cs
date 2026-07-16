using DC_bot.Constants;
using DC_bot.Exceptions.Music;
using DC_bot.Interface.Discord;
using DC_bot.Interface.Service.Localization;
using DC_bot.Interface.Service.Music;
using DC_bot.Interface.Service.Presentation;
using DC_bot.Service.Music.MusicServices;
using Lavalink4NET;
using Lavalink4NET.Players;
using Lavalink4NET.Rest.Entities.Tracks;
using Lavalink4NET.Tracks;
using Microsoft.Extensions.Logging;
using Moq;

namespace DC_bot_tests.UnitTests.Service.Music;

[Trait("Category", "Unit")]
public class PlaybackRequestServiceTests
{
    private const ulong GuildId = 111UL;

    private readonly Mock<IAudioService> _audioServiceMock = new();
    private readonly Mock<IDiscordChannel> _textChannelMock = new();
    private readonly Mock<IDiscordGuild> _guildMock = new();
    private readonly Mock<ILogger<PlaybackRequestService>> _loggerMock = new();
    private readonly Mock<IDiscordMessage> _messageMock = new();
    private readonly Mock<IPlaybackEventHandlerService> _playbackEventHandlerServiceMock = new();
    private readonly Mock<ILavalinkPlayer> _playerMock = new();
    private readonly Mock<IPlayerConnectionService> _playerConnectionServiceMock = new();
    private readonly Mock<IResponseBuilder> _responseBuilderMock = new();
    private readonly PlaybackRequestService _service;
    private readonly Mock<ITrackNotificationService> _trackNotificationServiceMock = new();
    private readonly Mock<ITrackPlaybackService> _trackPlaybackServiceMock = new();
    private readonly Mock<IDiscordChannel> _voiceChannelMock = new();
    private readonly Mock<ILocalizationService> _localizationServiceMock = new();

    public PlaybackRequestServiceTests()
    {
        _loggerMock.Setup(x => x.IsEnabled(It.IsAny<LogLevel>())).Returns(true);

        _guildMock.Setup(g => g.Id).Returns(GuildId);
        _textChannelMock.Setup(c => c.Guild).Returns(_guildMock.Object);
        _voiceChannelMock.Setup(c => c.Guild).Returns(_guildMock.Object);
        _messageMock.SetupGet(m => m.Channel).Returns(_textChannelMock.Object);
        _localizationServiceMock
            .Setup(l => l.Get(It.IsAny<ulong>(), It.IsAny<string>(), It.IsAny<object[]>()))
            .Returns((ulong _, string key, object[] args) => _localizationServiceMock.Object.Get(key, args));

        _service = new PlaybackRequestService(
            _audioServiceMock.Object,
            _responseBuilderMock.Object,
            _localizationServiceMock.Object,
            _trackNotificationServiceMock.Object,
            _playerConnectionServiceMock.Object,
            _playbackEventHandlerServiceMock.Object,
            _trackPlaybackServiceMock.Object,
            _loggerMock.Object);
    }

    [Fact]
    public async Task PlayAsyncUrl_InvalidJoinOrConnection_DoesNothing()
    {
        _playerConnectionServiceMock
            .Setup(p => p.TryJoinAndValidateAsync(_messageMock.Object, _voiceChannelMock.Object, It.IsAny<CancellationToken>()))
            .ReturnsAsync((null, null, 0UL, false));

        await _service.PlayAsyncUrl(_voiceChannelMock.Object, new Uri("https://example.com"), _messageMock.Object,
            TrackSearchMode.YouTube);

        _playbackEventHandlerServiceMock.Verify(
            h => h.RegisterPlaybackFinishedHandler(It.IsAny<ulong>(), It.IsAny<ILavalinkPlayer>(),
                It.IsAny<IDiscordChannel>()),
            Times.Never);
        _trackPlaybackServiceMock.Verify(
            p => p.PlayTheFoundMusicAsync(It.IsAny<TrackLoadResult>(), It.IsAny<ILavalinkPlayer>(),
                It.IsAny<IDiscordChannel>()),
            Times.Never);
    }

    [Fact]
    public async Task PlayAsyncQuery_InvalidJoinOrConnection_DoesNothing()
    {
        _playerConnectionServiceMock
            .Setup(p => p.TryJoinAndValidateAsync(_messageMock.Object, _voiceChannelMock.Object, It.IsAny<CancellationToken>()))
            .ReturnsAsync((null, null, 0UL, false));

        await _service.PlayAsyncQuery(_voiceChannelMock.Object, "test query", _messageMock.Object,
            TrackSearchMode.YouTube);

        _playbackEventHandlerServiceMock.Verify(
            h => h.RegisterPlaybackFinishedHandler(It.IsAny<ulong>(), It.IsAny<ILavalinkPlayer>(),
                It.IsAny<IDiscordChannel>()),
            Times.Never);
        _trackPlaybackServiceMock.Verify(
            p => p.PlayTheFoundMusicAsync(It.IsAny<TrackLoadResult>(), It.IsAny<ILavalinkPlayer>(),
                It.IsAny<IDiscordChannel>()),
            Times.Never);
    }

    [Fact]
    public async Task PlayAsyncUrl_LoadTracksThrows_SendsValidationErrorAndThrowsTrackLoadException()
    {
        var url = new Uri("https://example.com/test");
        _playerConnectionServiceMock
            .Setup(p => p.TryJoinAndValidateAsync(_messageMock.Object, _voiceChannelMock.Object, It.IsAny<CancellationToken>()))
            .ReturnsAsync((_playerMock.Object, _voiceChannelMock.Object, GuildId, true));

        _audioServiceMock
            .Setup(a => a.Tracks.LoadTracksAsync(url.ToString(), TrackSearchMode.YouTube, default,
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("load failed"));

        var ex = await Assert.ThrowsAsync<TrackLoadException>(() =>
            _service.PlayAsyncUrl(_voiceChannelMock.Object, url, _messageMock.Object, TrackSearchMode.YouTube));

        Assert.Equal(url.ToString(), ex.Query);
        _playbackEventHandlerServiceMock.Verify(
            h => h.RegisterPlaybackFinishedHandler(GuildId, _playerMock.Object, _textChannelMock.Object), Times.Once);
        _responseBuilderMock.Verify(
            r => r.SendValidationErrorAsync(_messageMock.Object, ValidationErrorKeys.LavalinkError), Times.Once);
        _trackPlaybackServiceMock.Verify(
            t => t.PlayTheFoundMusicAsync(It.IsAny<TrackLoadResult>(), It.IsAny<ILavalinkPlayer>(),
                It.IsAny<IDiscordChannel>()),
            Times.Never);
    }

    [Fact]
    public async Task PlayAsyncUrl_TrackNotFound_SendsSafeAndThrowsTrackLoadException()
    {
        var url = new Uri("https://example.com/missing");
        _playerConnectionServiceMock
            .Setup(p => p.TryJoinAndValidateAsync(_messageMock.Object, _voiceChannelMock.Object, It.IsAny<CancellationToken>()))
            .ReturnsAsync((_playerMock.Object, _voiceChannelMock.Object, GuildId, true));

        _localizationServiceMock
            .Setup(l => l.Get(LocalizationKeys.PlayCommandFailedToFindMusicUrlError))
            .Returns("Not found:");

        _audioServiceMock
            .Setup(a => a.Tracks.LoadTracksAsync(url.ToString(), TrackSearchMode.YouTube, default,
                It.IsAny<CancellationToken>()))
            .Returns(new ValueTask<TrackLoadResult>(new TrackLoadResult(null, null)));

        var ex = await Assert.ThrowsAsync<TrackLoadException>(() =>
            _service.PlayAsyncUrl(_voiceChannelMock.Object, url, _messageMock.Object, TrackSearchMode.YouTube));

        Assert.Equal(url.ToString(), ex.Query);
        _trackNotificationServiceMock.Verify(
            n => n.SendSafeAsync(_textChannelMock.Object, "Not found: https://example.com/missing",
                "PlayAsyncUrl.NotFound"),
            Times.Once);
        _trackPlaybackServiceMock.Verify(
            t => t.PlayTheFoundMusicAsync(It.IsAny<TrackLoadResult>(), It.IsAny<ILavalinkPlayer>(),
                It.IsAny<IDiscordChannel>()),
            Times.Never);
    }

    [Fact]
    public async Task PlayAsyncUrl_ValidLoad_RegistersHandlerAndDelegatesToTrackPlaybackService()
    {
        var url = new Uri("https://example.com/hit");
        var track = new LavalinkTrack { Author = "Author", Title = "Title", Identifier = "id" };

        _playerConnectionServiceMock
            .Setup(p => p.TryJoinAndValidateAsync(_messageMock.Object, _voiceChannelMock.Object, It.IsAny<CancellationToken>()))
            .ReturnsAsync((_playerMock.Object, _voiceChannelMock.Object, GuildId, true));

        _audioServiceMock
            .Setup(a => a.Tracks.LoadTracksAsync(url.ToString(), TrackSearchMode.YouTube, default,
                It.IsAny<CancellationToken>()))
            .Returns(new ValueTask<TrackLoadResult>(new TrackLoadResult(track, null)));

        await _service.PlayAsyncUrl(_voiceChannelMock.Object, url, _messageMock.Object, TrackSearchMode.YouTube);

        _playbackEventHandlerServiceMock.Verify(
            h => h.RegisterPlaybackFinishedHandler(GuildId, _playerMock.Object, _textChannelMock.Object), Times.Once);
        _trackPlaybackServiceMock.Verify(
            t => t.PlayTheFoundMusicAsync(It.IsAny<TrackLoadResult>(), _playerMock.Object, _textChannelMock.Object),
            Times.Once);
    }

    [Fact]
    public async Task PlayAsyncQuery_LoadTracksThrows_SendsValidationErrorAndThrowsTrackLoadException()
    {
        const string query = "artist song";
        _playerConnectionServiceMock
            .Setup(p => p.TryJoinAndValidateAsync(_messageMock.Object, _voiceChannelMock.Object, It.IsAny<CancellationToken>()))
            .ReturnsAsync((_playerMock.Object, _voiceChannelMock.Object, GuildId, true));

        _audioServiceMock
            .Setup(a => a.Tracks.LoadTracksAsync(query, TrackSearchMode.YouTube, default,
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("load failed"));

        var ex = await Assert.ThrowsAsync<TrackLoadException>(() =>
            _service.PlayAsyncQuery(_voiceChannelMock.Object, query, _messageMock.Object, TrackSearchMode.YouTube));

        Assert.Equal(query, ex.Query);
        _responseBuilderMock.Verify(
            r => r.SendValidationErrorAsync(_messageMock.Object, ValidationErrorKeys.LavalinkError), Times.Once);
        _trackPlaybackServiceMock.Verify(
            t => t.PlayTheFoundMusicAsync(It.IsAny<TrackLoadResult>(), It.IsAny<ILavalinkPlayer>(),
                It.IsAny<IDiscordChannel>()),
            Times.Never);
    }

    [Fact]
    public async Task PlayAsyncQuery_TrackNotFound_SendsSafeAndThrowsTrackLoadException()
    {
        const string query = "nincs ilyen";
        _playerConnectionServiceMock
            .Setup(p => p.TryJoinAndValidateAsync(_messageMock.Object, _voiceChannelMock.Object, It.IsAny<CancellationToken>()))
            .ReturnsAsync((_playerMock.Object, _voiceChannelMock.Object, GuildId, true));

        _localizationServiceMock
            .Setup(l => l.Get(LocalizationKeys.PlayCommandFailedToFindMusicUrlError))
            .Returns("Not found:");

        _audioServiceMock
            .Setup(a => a.Tracks.LoadTracksAsync(query, TrackSearchMode.YouTube, default,
                It.IsAny<CancellationToken>()))
            .Returns(new ValueTask<TrackLoadResult>(new TrackLoadResult(null, null)));

        var ex = await Assert.ThrowsAsync<TrackLoadException>(() =>
            _service.PlayAsyncQuery(_voiceChannelMock.Object, query, _messageMock.Object, TrackSearchMode.YouTube));

        Assert.Equal(query, ex.Query);
        _trackNotificationServiceMock.Verify(
            n => n.SendSafeAsync(_textChannelMock.Object, "Not found: nincs ilyen", "PlayAsyncQuery.NotFound"),
            Times.Once);
    }

    [Fact]
    public async Task PlayAsyncQuery_ValidLoad_RegistersHandlerAndDelegatesToTrackPlaybackService()
    {
        const string query = "artist title";
        var track = new LavalinkTrack { Author = "Author", Title = "Title", Identifier = "id" };

        _playerConnectionServiceMock
            .Setup(p => p.TryJoinAndValidateAsync(_messageMock.Object, _voiceChannelMock.Object, It.IsAny<CancellationToken>()))
            .ReturnsAsync((_playerMock.Object, _voiceChannelMock.Object, GuildId, true));

        _audioServiceMock
            .Setup(a => a.Tracks.LoadTracksAsync(query, TrackSearchMode.YouTube, default,
                It.IsAny<CancellationToken>()))
            .Returns(new ValueTask<TrackLoadResult>(new TrackLoadResult(track, null)));

        await _service.PlayAsyncQuery(_voiceChannelMock.Object, query, _messageMock.Object, TrackSearchMode.YouTube);

        _playbackEventHandlerServiceMock.Verify(
            h => h.RegisterPlaybackFinishedHandler(GuildId, _playerMock.Object, _textChannelMock.Object), Times.Once);
        _trackPlaybackServiceMock.Verify(
            t => t.PlayTheFoundMusicAsync(It.IsAny<TrackLoadResult>(), _playerMock.Object, _textChannelMock.Object),
            Times.Once);
    }
}
