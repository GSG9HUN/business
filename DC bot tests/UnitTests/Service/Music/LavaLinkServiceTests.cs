using DC_bot.Constants;
using DC_bot.Exceptions.Music;
using DC_bot.Interface.Discord;
using DC_bot.Interface.Service.Localization;
using DC_bot.Interface.Service.Music.MusicServiceInterface;
using DC_bot.Interface.Service.Music.ProgressiveTimerInterface;
using DC_bot.Interface.Service.Presentation;
using DC_bot.Service.Music;
using DSharpPlus;
using DSharpPlus.Entities;
using Lavalink4NET;
using Lavalink4NET.Players;
using Lavalink4NET.Rest.Entities.Tracks;
using Lavalink4NET.Tracks;
using Microsoft.Extensions.Logging;
using Moq;
using DC_bot_tests.TestHelperFiles;
using DC_bot.Interface;

namespace DC_bot_tests.UnitTests.Service.Music;

[Trait("Category", "Unit")]
public class LavaLinkServiceTests
{
    private const ulong GuildId = 111UL;

    private readonly Mock<IAudioService> _audioServiceMock = new();
    private readonly Mock<ICurrentTrackService> _currentTrackServiceMock = new();
    private readonly Mock<IDiscordChannel> _textChannelMock = new();
    private readonly Mock<IDiscordGuild> _guildMock = new();
    private readonly Mock<ILogger<LavaLinkService>> _loggerMock = new();
    private readonly Mock<IDiscordMember> _memberMock = new();
    private readonly Mock<IDiscordMessage> _messageMock = new();
    private readonly Mock<IMusicQueueService> _musicQueueServiceMock = new();
    private readonly Mock<IPlaybackEventHandlerService> _playbackEventHandlerServiceMock = new();
    private readonly Mock<ILavalinkPlayer> _playerMock = new();
    private readonly Mock<IPlayerConnectionService> _playerConnectionServiceMock = new();
    private readonly Mock<IProgressiveTimerService> _progressiveTimerServiceMock = new();
    private readonly Mock<IRepeatService> _repeatServiceMock = new();
    private readonly Mock<IResponseBuilder> _responseBuilderMock = new();
    private readonly LavaLinkService _service;
    private readonly Mock<ITrackNotificationService> _trackNotificationServiceMock = new();
    private readonly Mock<ITrackPlaybackService> _trackPlaybackServiceMock = new();
    private readonly Mock<IDiscordVoiceState> _voiceStateMock = new();
    private readonly Mock<IDiscordChannel> _voiceChannelMock = new();
    private readonly Mock<ILocalizationService> _localizationServiceMock = new();

    public LavaLinkServiceTests()
    {
        _loggerMock.Setup(x => x.IsEnabled(It.IsAny<LogLevel>())).Returns(true);

        _guildMock.Setup(g => g.Id).Returns(GuildId);
        _textChannelMock.Setup(c => c.Guild).Returns(_guildMock.Object);
        _voiceChannelMock.Setup(c => c.Guild).Returns(_guildMock.Object);
        _messageMock.SetupGet(m => m.Channel).Returns(_textChannelMock.Object);
        _voiceStateMock.SetupGet(v => v.Channel).Returns(_voiceChannelMock.Object);
        _memberMock.SetupGet(m => m.VoiceState).Returns(_voiceStateMock.Object);
        _localizationServiceMock
            .Setup(l => l.Get(It.IsAny<ulong>(), It.IsAny<string>(), It.IsAny<object[]>()))
            .Returns((ulong _, string key, object[] args) => _localizationServiceMock.Object.Get(key, args));

        _service = new LavaLinkService(
            _musicQueueServiceMock.Object,
            _loggerMock.Object,
            _audioServiceMock.Object,
            _responseBuilderMock.Object,
            _localizationServiceMock.Object,
            _repeatServiceMock.Object,
            _currentTrackServiceMock.Object,
            _trackNotificationServiceMock.Object,
            _playerConnectionServiceMock.Object,
            _playbackEventHandlerServiceMock.Object,
            _progressiveTimerServiceMock.Object,
            _trackPlaybackServiceMock.Object);
    }

    [Fact]
    public async Task LeaveVoiceChannel_ValidConnection_CurrentTrackExists_StopsCleansDisconnects()
    {
        _playerMock.SetupGet(p => p.CurrentTrack).Returns(new LavalinkTrack
        {
            Author = "Artist",
            Title = "Current",
            Identifier = "current-id"
        });

        _playerConnectionServiceMock
            .Setup(p => p.TryGetAndValidateExistingPlayerAsync(_messageMock.Object, _voiceChannelMock.Object))
            .ReturnsAsync((_playerMock.Object, _voiceChannelMock.Object, GuildId, true));

        await _service.LeaveVoiceChannel(_messageMock.Object, _memberMock.Object);

        _playerMock.Verify(p => p.StopAsync(CancellationToken.None), Times.Once);
        _playbackEventHandlerServiceMock.Verify(h => h.CleanupGuildAsync(GuildId), Times.Once);
        _progressiveTimerServiceMock.Verify(t => t.Stop(GuildId), Times.Once);
        _playerMock.Verify(p => p.DisconnectAsync(CancellationToken.None), Times.Once);
    }

    [Fact]
    public async Task LeaveVoiceChannel_InvalidPlayer_DoesNothing()
    {
        _playerConnectionServiceMock
            .Setup(p => p.TryGetAndValidateExistingPlayerAsync(_messageMock.Object, _voiceChannelMock.Object))
            .ReturnsAsync((null, null, 0UL, false));

        await _service.LeaveVoiceChannel(_messageMock.Object, _memberMock.Object);

        _playbackEventHandlerServiceMock.Verify(h => h.CleanupGuildAsync(It.IsAny<ulong>()), Times.Never);
        _progressiveTimerServiceMock.Verify(t => t.Stop(It.IsAny<ulong>()), Times.Never);
        _playerMock.Verify(p => p.DisconnectAsync(CancellationToken.None), Times.Never);
    }

    [Fact]
    public void TrackStarted_AddAndRemove_ForwardedToTrackNotificationService()
    {
        Func<IDiscordChannel, DiscordEmbed, Task> handler = (_, _) => Task.CompletedTask;

        _service.TrackStarted += handler;
        _service.TrackStarted -= handler;

        _trackNotificationServiceMock.VerifyAdd(s => s.TrackStarted += handler, Times.Once);
        _trackNotificationServiceMock.VerifyRemove(s => s.TrackStarted -= handler, Times.Once);
    }

    [Fact]
    public async Task PlayAsyncUrl_InvalidJoinOrConnection_DoesNothing()
    {
        _playerConnectionServiceMock
            .Setup(p => p.TryJoinAndValidateAsync(_messageMock.Object, _voiceChannelMock.Object))
            .ReturnsAsync((null, null, 0UL, false));

        await _service.PlayAsyncUrl(_voiceChannelMock.Object, new Uri("https://example.com"), _messageMock.Object,
            TrackSearchMode.YouTube);

        _playbackEventHandlerServiceMock.Verify(
            h => h.RegisterPlaybackFinishedHandler(It.IsAny<ulong>(), It.IsAny<ILavalinkPlayer>(), It.IsAny<IDiscordChannel>()),
            Times.Never);
        _trackPlaybackServiceMock.Verify(
            p => p.PlayTheFoundMusicAsync(It.IsAny<TrackLoadResult>(), It.IsAny<ILavalinkPlayer>(), It.IsAny<IDiscordChannel>()),
            Times.Never);
    }

    [Fact]
    public async Task PlayAsyncQuery_InvalidJoinOrConnection_DoesNothing()
    {
        _playerConnectionServiceMock
            .Setup(p => p.TryJoinAndValidateAsync(_messageMock.Object, _voiceChannelMock.Object))
            .ReturnsAsync((null, null, 0UL, false));

        await _service.PlayAsyncQuery(_voiceChannelMock.Object, "test query", _messageMock.Object,
            TrackSearchMode.YouTube);

        _playbackEventHandlerServiceMock.Verify(
            h => h.RegisterPlaybackFinishedHandler(It.IsAny<ulong>(), It.IsAny<ILavalinkPlayer>(), It.IsAny<IDiscordChannel>()),
            Times.Never);
        _trackPlaybackServiceMock.Verify(
            p => p.PlayTheFoundMusicAsync(It.IsAny<TrackLoadResult>(), It.IsAny<ILavalinkPlayer>(), It.IsAny<IDiscordChannel>()),
            Times.Never);
    }

    [Fact]
    public async Task PauseAsync_NoCurrentTrack_SendsNotification()
    {
        _localizationServiceMock.Setup(l => l.Get(LocalizationKeys.PauseCommandError)).Returns("No track");
        _playerMock.SetupGet(p => p.CurrentTrack).Returns((LavalinkTrack?)null);
        _playerConnectionServiceMock
            .Setup(p => p.TryGetAndValidateExistingPlayerAsync(_messageMock.Object, _voiceChannelMock.Object))
            .ReturnsAsync((_playerMock.Object, _textChannelMock.Object, GuildId, true));

        await _service.PauseAsync(_messageMock.Object, _memberMock.Object);

        _trackNotificationServiceMock.Verify(
            n => n.SendSafeAsync(_textChannelMock.Object, "No track", "PauseAsync.NoTrack"), Times.Once);
        _playerMock.Verify(p => p.PauseAsync(CancellationToken.None), Times.Never);
    }

    [Fact]
    public async Task PauseAsync_WithCurrentTrack_PausesPlayer()
    {
        _localizationServiceMock.Setup(l => l.Get(LocalizationKeys.PauseCommandResponse)).Returns("Paused");
        _playerMock.SetupGet(p => p.CurrentTrack).Returns(new LavalinkTrack { Author = "Test author", Title = "Track", Identifier = "id" });
        _playerConnectionServiceMock
            .Setup(p => p.TryGetAndValidateExistingPlayerAsync(_messageMock.Object, _voiceChannelMock.Object))
            .ReturnsAsync((_playerMock.Object, _textChannelMock.Object, GuildId, true));

        await _service.PauseAsync(_messageMock.Object, _memberMock.Object);

        _playerMock.Verify(p => p.PauseAsync(CancellationToken.None), Times.Once);
    }

    [Fact]
    public async Task ResumeAsync_NoCurrentTrack_SendsNotification()
    {
        _localizationServiceMock.Setup(l => l.Get(LocalizationKeys.ResumeCommandError)).Returns("No paused track");
        _playerMock.SetupGet(p => p.CurrentTrack).Returns((LavalinkTrack?)null);
        _playerConnectionServiceMock
            .Setup(p => p.TryGetAndValidateExistingPlayerAsync(_messageMock.Object, _voiceChannelMock.Object))
            .ReturnsAsync((_playerMock.Object, _textChannelMock.Object, GuildId, true));

        await _service.ResumeAsync(_messageMock.Object, _memberMock.Object);

        _trackNotificationServiceMock.Verify(
            n => n.SendSafeAsync(_textChannelMock.Object, "No paused track", "ResumeAsync.NoTrack"), Times.Once);
        _playerMock.Verify(p => p.ResumeAsync(CancellationToken.None), Times.Never);
    }

    [Fact]
    public async Task ResumeAsync_WithCurrentTrack_ResumesPlayer()
    {
        _localizationServiceMock.Setup(l => l.Get(LocalizationKeys.ResumeCommandResponse)).Returns("Resumed");
        _playerMock.SetupGet(p => p.CurrentTrack).Returns(new LavalinkTrack { Author = "Test author", Title = "Track", Identifier = "id" });
        _playerConnectionServiceMock
            .Setup(p => p.TryGetAndValidateExistingPlayerAsync(_messageMock.Object, _voiceChannelMock.Object))
            .ReturnsAsync((_playerMock.Object, _textChannelMock.Object, GuildId, true));

        await _service.ResumeAsync(_messageMock.Object, _memberMock.Object);

        _playerMock.Verify(p => p.ResumeAsync(CancellationToken.None), Times.Once);
    }

    [Fact]
    public async Task SkipAsync_NoCurrentTrackAndQueueEmpty_SendsNotification()
    {
        _localizationServiceMock.Setup(l => l.Get(LocalizationKeys.SkipCommandError)).Returns("Nothing to skip");
        _playerMock.SetupGet(p => p.CurrentTrack).Returns((LavalinkTrack?)null);
        _musicQueueServiceMock.Setup(q => q.HasTracks(GuildId)).ReturnsAsync(false);

        _playerConnectionServiceMock
            .Setup(p => p.TryGetAndValidateExistingPlayerAsync(_messageMock.Object, _voiceChannelMock.Object))
            .ReturnsAsync((_playerMock.Object, _textChannelMock.Object, GuildId, true));

        await _service.SkipAsync(_messageMock.Object, _memberMock.Object);

        _trackNotificationServiceMock.Verify(
            n => n.SendSafeAsync(_textChannelMock.Object, "Nothing to skip", "SkipAsync.NoTrack"), Times.Once);
        _playerMock.Verify(p => p.StopAsync(CancellationToken.None), Times.Never);
    }

    [Fact]
    public async Task SkipAsync_WithCurrentTrack_StopsPlayerAndProgressiveTimer()
    {
        _playerMock.SetupGet(p => p.CurrentTrack).Returns(new LavalinkTrack { Author = "Test author", Title = "Track", Identifier = "id" });
        _playerConnectionServiceMock
            .Setup(p => p.TryGetAndValidateExistingPlayerAsync(_messageMock.Object, _voiceChannelMock.Object))
            .ReturnsAsync((_playerMock.Object, _textChannelMock.Object, GuildId, true));

        await _service.SkipAsync(_messageMock.Object, _memberMock.Object);

        _playerMock.Verify(p => p.StopAsync(CancellationToken.None), Times.Once);
        _progressiveTimerServiceMock.Verify(t => t.Stop(GuildId), Times.Once);
    }

    [Fact]
    public async Task PlayAsyncUrl_LoadTracksThrows_SendsValidationErrorAndThrowsTrackLoadException()
    {
        var url = new Uri("https://example.com/test");
        _playerConnectionServiceMock
            .Setup(p => p.TryJoinAndValidateAsync(_messageMock.Object, _voiceChannelMock.Object))
            .ReturnsAsync((_playerMock.Object, _voiceChannelMock.Object, GuildId, true));

        _audioServiceMock
            .Setup(a => a.Tracks.LoadTracksAsync(url.ToString(), TrackSearchMode.YouTube, default, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("load failed"));

        var ex = await Assert.ThrowsAsync<TrackLoadException>(() =>
            _service.PlayAsyncUrl(_voiceChannelMock.Object, url, _messageMock.Object, TrackSearchMode.YouTube));

        Assert.Equal(url.ToString(), ex.Query);
        _playbackEventHandlerServiceMock.Verify(
            h => h.RegisterPlaybackFinishedHandler(GuildId, _playerMock.Object, _textChannelMock.Object), Times.Once);
        _responseBuilderMock.Verify(
            r => r.SendValidationErrorAsync(_messageMock.Object, ValidationErrorKeys.LavalinkError), Times.Once);
        _trackPlaybackServiceMock.Verify(
            t => t.PlayTheFoundMusicAsync(It.IsAny<TrackLoadResult>(), It.IsAny<ILavalinkPlayer>(), It.IsAny<IDiscordChannel>()),
            Times.Never);
    }

    [Fact]
    public async Task PlayAsyncUrl_TrackNotFound_SendsSafeAndThrowsTrackLoadException()
    {
        var url = new Uri("https://example.com/missing");
        _playerConnectionServiceMock
            .Setup(p => p.TryJoinAndValidateAsync(_messageMock.Object, _voiceChannelMock.Object))
            .ReturnsAsync((_playerMock.Object, _voiceChannelMock.Object, GuildId, true));

        _localizationServiceMock
            .Setup(l => l.Get(LocalizationKeys.PlayCommandFailedToFindMusicUrlError))
            .Returns("Not found:");

        _audioServiceMock
            .Setup(a => a.Tracks.LoadTracksAsync(url.ToString(), TrackSearchMode.YouTube, default, It.IsAny<CancellationToken>()))
            .Returns(new ValueTask<TrackLoadResult>(new TrackLoadResult(null, null)));

        var ex = await Assert.ThrowsAsync<TrackLoadException>(() =>
            _service.PlayAsyncUrl(_voiceChannelMock.Object, url, _messageMock.Object, TrackSearchMode.YouTube));

        Assert.Equal(url.ToString(), ex.Query);
        _trackNotificationServiceMock.Verify(
            n => n.SendSafeAsync(_textChannelMock.Object, "Not found: https://example.com/missing", "PlayAsyncUrl.NotFound"),
            Times.Once);
        _trackPlaybackServiceMock.Verify(
            t => t.PlayTheFoundMusicAsync(It.IsAny<TrackLoadResult>(), It.IsAny<ILavalinkPlayer>(), It.IsAny<IDiscordChannel>()),
            Times.Never);
    }

    [Fact]
    public async Task PlayAsyncUrl_ValidLoad_RegistersHandlerAndDelegatesToTrackPlaybackService()
    {
        var url = new Uri("https://example.com/hit");
        var track = new LavalinkTrack { Author = "Author", Title = "Title", Identifier = "id" };

        _playerConnectionServiceMock
            .Setup(p => p.TryJoinAndValidateAsync(_messageMock.Object, _voiceChannelMock.Object))
            .ReturnsAsync((_playerMock.Object, _voiceChannelMock.Object, GuildId, true));

        _audioServiceMock
            .Setup(a => a.Tracks.LoadTracksAsync(url.ToString(), TrackSearchMode.YouTube, default, It.IsAny<CancellationToken>()))
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
            .Setup(p => p.TryJoinAndValidateAsync(_messageMock.Object, _voiceChannelMock.Object))
            .ReturnsAsync((_playerMock.Object, _voiceChannelMock.Object, GuildId, true));

        _audioServiceMock
            .Setup(a => a.Tracks.LoadTracksAsync(query, TrackSearchMode.YouTube, default, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("load failed"));

        var ex = await Assert.ThrowsAsync<TrackLoadException>(() =>
            _service.PlayAsyncQuery(_voiceChannelMock.Object, query, _messageMock.Object, TrackSearchMode.YouTube));

        Assert.Equal(query, ex.Query);
        _responseBuilderMock.Verify(
            r => r.SendValidationErrorAsync(_messageMock.Object, ValidationErrorKeys.LavalinkError), Times.Once);
        _trackPlaybackServiceMock.Verify(
            t => t.PlayTheFoundMusicAsync(It.IsAny<TrackLoadResult>(), It.IsAny<ILavalinkPlayer>(), It.IsAny<IDiscordChannel>()),
            Times.Never);
    }

    [Fact]
    public async Task PlayAsyncQuery_TrackNotFound_SendsSafeAndThrowsTrackLoadException()
    {
        const string query = "nincs ilyen";
        _playerConnectionServiceMock
            .Setup(p => p.TryJoinAndValidateAsync(_messageMock.Object, _voiceChannelMock.Object))
            .ReturnsAsync((_playerMock.Object, _voiceChannelMock.Object, GuildId, true));

        _localizationServiceMock
            .Setup(l => l.Get(LocalizationKeys.PlayCommandFailedToFindMusicUrlError))
            .Returns("Not found:");

        _audioServiceMock
            .Setup(a => a.Tracks.LoadTracksAsync(query, TrackSearchMode.YouTube, default, It.IsAny<CancellationToken>()))
            .Returns(new ValueTask<TrackLoadResult>(new TrackLoadResult(null, null)));

        var ex = await Assert.ThrowsAsync<TrackLoadException>(() =>
            _service.PlayAsyncQuery(_voiceChannelMock.Object, query, _messageMock.Object, TrackSearchMode.YouTube));

        Assert.Equal(query, ex.Query);
        _trackNotificationServiceMock.Verify(
            n => n.SendSafeAsync(_textChannelMock.Object, "Not found: nincs ilyen", "PlayAsyncQuery.NotFound"), Times.Once);
    }

    [Fact]
    public async Task PlayAsyncQuery_ValidLoad_RegistersHandlerAndDelegatesToTrackPlaybackService()
    {
        const string query = "artist title";
        var track = new LavalinkTrack { Author = "Author", Title = "Title", Identifier = "id" };

        _playerConnectionServiceMock
            .Setup(p => p.TryJoinAndValidateAsync(_messageMock.Object, _voiceChannelMock.Object))
            .ReturnsAsync((_playerMock.Object, _voiceChannelMock.Object, GuildId, true));

        _audioServiceMock
            .Setup(a => a.Tracks.LoadTracksAsync(query, TrackSearchMode.YouTube, default, It.IsAny<CancellationToken>()))
            .Returns(new ValueTask<TrackLoadResult>(new TrackLoadResult(track, null)));

        await _service.PlayAsyncQuery(_voiceChannelMock.Object, query, _messageMock.Object, TrackSearchMode.YouTube);

        _playbackEventHandlerServiceMock.Verify(
            h => h.RegisterPlaybackFinishedHandler(GuildId, _playerMock.Object, _textChannelMock.Object), Times.Once);
        _trackPlaybackServiceMock.Verify(
            t => t.PlayTheFoundMusicAsync(It.IsAny<TrackLoadResult>(), _playerMock.Object, _textChannelMock.Object),
            Times.Once);
    }

    [Fact]
    public async Task PauseAsync_InvalidPlayer_ReturnsWithoutSideEffects()
    {
        _playerConnectionServiceMock
            .Setup(p => p.TryGetAndValidateExistingPlayerAsync(_messageMock.Object, _voiceChannelMock.Object))
            .ReturnsAsync((null, null, 0UL, false));

        await _service.PauseAsync(_messageMock.Object, _memberMock.Object);

        _trackNotificationServiceMock.Verify(
            n => n.SendSafeAsync(It.IsAny<IDiscordChannel>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        _responseBuilderMock.Verify(
            r => r.SendValidationErrorAsync(It.IsAny<IDiscordMessage>(), It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task PauseAsync_PauseThrows_SendsValidationError()
    {
        _playerMock.SetupGet(p => p.CurrentTrack).Returns(new LavalinkTrack { Author = "A", Title = "T", Identifier = "id" });
        _playerMock.Setup(p => p.PauseAsync(CancellationToken.None)).ThrowsAsync(new InvalidOperationException("pause fail"));
        _playerConnectionServiceMock
            .Setup(p => p.TryGetAndValidateExistingPlayerAsync(_messageMock.Object, _voiceChannelMock.Object))
            .ReturnsAsync((_playerMock.Object, _textChannelMock.Object, GuildId, true));

        await _service.PauseAsync(_messageMock.Object, _memberMock.Object);

        _responseBuilderMock.Verify(
            r => r.SendValidationErrorAsync(_messageMock.Object, ValidationErrorKeys.LavalinkError), Times.Once);
    }

    [Fact]
    public async Task ResumeAsync_InvalidPlayer_ReturnsWithoutSideEffects()
    {
        _playerConnectionServiceMock
            .Setup(p => p.TryGetAndValidateExistingPlayerAsync(_messageMock.Object, _voiceChannelMock.Object))
            .ReturnsAsync((null, null, 0UL, false));

        await _service.ResumeAsync(_messageMock.Object, _memberMock.Object);

        _trackNotificationServiceMock.Verify(
            n => n.SendSafeAsync(It.IsAny<IDiscordChannel>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        _responseBuilderMock.Verify(
            r => r.SendValidationErrorAsync(It.IsAny<IDiscordMessage>(), It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task ResumeAsync_ResumeThrows_SendsValidationError()
    {
        _playerMock.SetupGet(p => p.CurrentTrack).Returns(new LavalinkTrack { Author = "A", Title = "T", Identifier = "id" });
        _playerMock.Setup(p => p.ResumeAsync(CancellationToken.None)).ThrowsAsync(new InvalidOperationException("resume fail"));
        _playerConnectionServiceMock
            .Setup(p => p.TryGetAndValidateExistingPlayerAsync(_messageMock.Object, _voiceChannelMock.Object))
            .ReturnsAsync((_playerMock.Object, _textChannelMock.Object, GuildId, true));

        await _service.ResumeAsync(_messageMock.Object, _memberMock.Object);

        _responseBuilderMock.Verify(
            r => r.SendValidationErrorAsync(_messageMock.Object, ValidationErrorKeys.LavalinkError), Times.Once);
    }

    [Fact]
    public async Task SkipAsync_NoCurrentTrackButQueueHasTracks_StopsAndStopsTimer()
    {
        _playerMock.SetupGet(p => p.CurrentTrack).Returns((LavalinkTrack?)null);
        _musicQueueServiceMock.Setup(q => q.HasTracks(GuildId)).ReturnsAsync(true);
        _playerConnectionServiceMock
            .Setup(p => p.TryGetAndValidateExistingPlayerAsync(_messageMock.Object, _voiceChannelMock.Object))
            .ReturnsAsync((_playerMock.Object, _textChannelMock.Object, GuildId, true));

        await _service.SkipAsync(_messageMock.Object, _memberMock.Object);

        _playerMock.Verify(p => p.StopAsync(CancellationToken.None), Times.Once);
        _progressiveTimerServiceMock.Verify(t => t.Stop(GuildId), Times.Once);
    }

    [Fact]
    public async Task SkipAsync_InvalidPlayer_ReturnsWithoutSideEffects()
    {
        _playerConnectionServiceMock
            .Setup(p => p.TryGetAndValidateExistingPlayerAsync(_messageMock.Object, _voiceChannelMock.Object))
            .ReturnsAsync((null, null, 0UL, false));

        await _service.SkipAsync(_messageMock.Object, _memberMock.Object);

        _musicQueueServiceMock.Verify(q => q.HasTracks(It.IsAny<ulong>()), Times.Never);
        _responseBuilderMock.Verify(
            r => r.SendValidationErrorAsync(It.IsAny<IDiscordMessage>(), It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task SkipAsync_StopThrows_SendsValidationError()
    {
        _playerMock.SetupGet(p => p.CurrentTrack).Returns(new LavalinkTrack { Author = "A", Title = "T", Identifier = "id" });
        _playerMock.Setup(p => p.StopAsync(CancellationToken.None)).ThrowsAsync(new InvalidOperationException("stop fail"));
        _playerConnectionServiceMock
            .Setup(p => p.TryGetAndValidateExistingPlayerAsync(_messageMock.Object, _voiceChannelMock.Object))
            .ReturnsAsync((_playerMock.Object, _textChannelMock.Object, GuildId, true));

        await _service.SkipAsync(_messageMock.Object, _memberMock.Object);

        _responseBuilderMock.Verify(
            r => r.SendValidationErrorAsync(_messageMock.Object, ValidationErrorKeys.LavalinkError), Times.Once);
    }

    [Fact]
    public async Task LeaveVoiceChannel_ValidConnection_NoCurrentTrack_CleansDisconnectsWithoutStop()
    {
        _playerMock.SetupGet(p => p.CurrentTrack).Returns((LavalinkTrack?)null);
        _playerConnectionServiceMock
            .Setup(p => p.TryGetAndValidateExistingPlayerAsync(_messageMock.Object, _voiceChannelMock.Object))
            .ReturnsAsync((_playerMock.Object, _voiceChannelMock.Object, GuildId, true));

        await _service.LeaveVoiceChannel(_messageMock.Object, _memberMock.Object);

        _playerMock.Verify(p => p.StopAsync(CancellationToken.None), Times.Never);
        _playbackEventHandlerServiceMock.Verify(h => h.CleanupGuildAsync(GuildId), Times.Once);
        _playerMock.Verify(p => p.DisconnectAsync(CancellationToken.None), Times.Once);
    }

    [Fact]
    public async Task LeaveVoiceChannel_DisconnectThrows_SendsValidationError()
    {
        _playerMock.SetupGet(p => p.CurrentTrack).Returns((LavalinkTrack?)null);
        _playerMock.Setup(p => p.DisconnectAsync(CancellationToken.None)).ThrowsAsync(new InvalidOperationException("disconnect fail"));

        _playerConnectionServiceMock
            .Setup(p => p.TryGetAndValidateExistingPlayerAsync(_messageMock.Object, _voiceChannelMock.Object))
            .ReturnsAsync((_playerMock.Object, _voiceChannelMock.Object, GuildId, true));

        await _service.LeaveVoiceChannel(_messageMock.Object, _memberMock.Object);

        _responseBuilderMock.Verify(
            r => r.SendValidationErrorAsync(_messageMock.Object, ValidationErrorKeys.LavalinkError), Times.Once);
    }

    [Fact]
    public async Task StartPlayingQueue_InvalidJoin_DoesNothing()
    {
        _playerConnectionServiceMock
            .Setup(p => p.TryJoinAndValidateAsync(_messageMock.Object, _voiceChannelMock.Object))
            .ReturnsAsync((null, null, 0UL, false));

        await _service.StartPlayingQueue(_messageMock.Object, _textChannelMock.Object, _memberMock.Object);

        _playbackEventHandlerServiceMock.Verify(
            h => h.RegisterPlaybackFinishedHandler(It.IsAny<ulong>(), It.IsAny<ILavalinkPlayer>(), It.IsAny<IDiscordChannel>()),
            Times.Never);
        _musicQueueServiceMock.Verify(q => q.Dequeue(It.IsAny<ulong>()), Times.Never);
        _playerMock.Verify(p => p.PlayAsync(It.IsAny<LavalinkTrack>(), It.IsAny<TrackPlayProperties>(), CancellationToken.None),
            Times.Never);
        _trackNotificationServiceMock.Verify(
            n => n.NotifyNowPlayingAsync(It.IsAny<IDiscordChannel>(), It.IsAny<ILavaLinkTrack>(), It.IsAny<TimeSpan>(),
                It.IsAny<TimeSpan>()), Times.Never);
        _currentTrackServiceMock.Verify(c => c.SetCurrentTrackAsync(It.IsAny<ulong>(), It.IsAny<ILavaLinkTrack>(), CancellationToken.None), Times.Never);
    }

    [Fact]
    public async Task StartPlayingQueue_QueueEmpty_RegistersHandlerButDoesNotPlay()
    {
        _playerConnectionServiceMock
            .Setup(p => p.TryJoinAndValidateAsync(_messageMock.Object, _voiceChannelMock.Object))
            .ReturnsAsync((_playerMock.Object, _voiceChannelMock.Object, GuildId, true));

        _musicQueueServiceMock.Setup(q => q.Dequeue(GuildId)).ReturnsAsync((ILavaLinkTrack?)null);

        await _service.StartPlayingQueue(_messageMock.Object, _textChannelMock.Object, _memberMock.Object);

        _playbackEventHandlerServiceMock.Verify(
            h => h.RegisterPlaybackFinishedHandler(GuildId, _playerMock.Object, _textChannelMock.Object), Times.Once);
        _playerMock.Verify(p => p.PlayAsync(It.IsAny<LavalinkTrack>(), It.IsAny<TrackPlayProperties>(), CancellationToken.None),
            Times.Never);
        _trackNotificationServiceMock.Verify(
            n => n.NotifyNowPlayingAsync(It.IsAny<IDiscordChannel>(), It.IsAny<ILavaLinkTrack>(), It.IsAny<TimeSpan>(),
                It.IsAny<TimeSpan>()), Times.Never);
        _currentTrackServiceMock.Verify(c => c.SetCurrentTrackAsync(It.IsAny<ulong>(), It.IsAny<ILavaLinkTrack>(), CancellationToken.None), Times.Never);
    }

    [Fact]
    public async Task StartPlayingQueue_PlayAsyncThrows_SendsValidationError_AndDoesNotSetCurrentTrack()
    {
        var nextTrack = TrackTestHelper.CreateTrackWrapper("Artist", "Title", "id", 120);

        _playerConnectionServiceMock
            .Setup(p => p.TryJoinAndValidateAsync(_messageMock.Object, _voiceChannelMock.Object))
            .ReturnsAsync((_playerMock.Object, _voiceChannelMock.Object, GuildId, true));

        _musicQueueServiceMock.Setup(q => q.Dequeue(GuildId)).ReturnsAsync(nextTrack);
        _playerMock.Setup(p => p.PlayAsync(nextTrack.ToLavalinkTrack(), It.IsAny<TrackPlayProperties>(), CancellationToken.None))
            .ThrowsAsync(new InvalidOperationException("play failed"));

        await _service.StartPlayingQueue(_messageMock.Object, _textChannelMock.Object, _memberMock.Object);

        _responseBuilderMock.Verify(
            r => r.SendValidationErrorAsync(_messageMock.Object, ValidationErrorKeys.LavalinkError), Times.Once);
        _trackNotificationServiceMock.Verify(
            n => n.NotifyNowPlayingAsync(It.IsAny<IDiscordChannel>(), It.IsAny<ILavaLinkTrack>(), It.IsAny<TimeSpan>(),
                It.IsAny<TimeSpan>()), Times.Never);
        _currentTrackServiceMock.Verify(c => c.SetCurrentTrackAsync(It.IsAny<ulong>(), It.IsAny<ILavaLinkTrack>(), CancellationToken.None), Times.Never);
    }

    [Fact]
    public async Task StartPlayingQueue_Success_PlaysNotifiesAndSetsCurrentTrack()
    {
        var nextTrack = TrackTestHelper.CreateTrackWrapper("Artist", "Title", "id", 120);

        _playerConnectionServiceMock
            .Setup(p => p.TryJoinAndValidateAsync(_messageMock.Object, _voiceChannelMock.Object))
            .ReturnsAsync((_playerMock.Object, _voiceChannelMock.Object, GuildId, true));

        _musicQueueServiceMock.Setup(q => q.Dequeue(GuildId)).ReturnsAsync(nextTrack);

        await _service.StartPlayingQueue(_messageMock.Object, _textChannelMock.Object, _memberMock.Object);

        _playbackEventHandlerServiceMock.Verify(
            h => h.RegisterPlaybackFinishedHandler(GuildId, _playerMock.Object, _textChannelMock.Object), Times.Once);
        _playerMock.Verify(p => p.PlayAsync(nextTrack.ToLavalinkTrack(), It.IsAny<TrackPlayProperties>(), CancellationToken.None),
            Times.Once);
        _trackNotificationServiceMock.Verify(
            n => n.NotifyNowPlayingAsync(_textChannelMock.Object, nextTrack, TimeSpan.Zero, nextTrack.Duration), Times.Once);
        _currentTrackServiceMock.Verify(c => c.SetCurrentTrackAsync(GuildId, nextTrack, CancellationToken.None), Times.Once);
        _responseBuilderMock.Verify(
            r => r.SendValidationErrorAsync(It.IsAny<IDiscordMessage>(), It.IsAny<string>()), Times.Never);
    }
}
