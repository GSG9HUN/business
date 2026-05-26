using DC_bot.Constants;
using DC_bot.Interface.Discord;
using DC_bot.Interface.Service.Localization;
using DC_bot.Interface.Service.Music.MusicServiceInterface;
using DC_bot.Interface.Service.Music.ProgressiveTimerInterface;
using DC_bot.Interface.Service.Presentation;
using DC_bot.Service.Music.MusicServices;
using Lavalink4NET.Players;
using Lavalink4NET.Tracks;
using Microsoft.Extensions.Logging;
using Moq;

namespace DC_bot_tests.UnitTests.Service.Music;

[Trait("Category", "Unit")]
public class PlaybackControlServiceTests
{
    private const ulong GuildId = 111UL;

    private readonly Mock<IDiscordChannel> _textChannelMock = new();
    private readonly Mock<IDiscordGuild> _guildMock = new();
    private readonly Mock<ILogger<PlaybackControlService>> _loggerMock = new();
    private readonly Mock<IDiscordMember> _memberMock = new();
    private readonly Mock<IDiscordMessage> _messageMock = new();
    private readonly Mock<IMusicQueueService> _musicQueueServiceMock = new();
    private readonly Mock<IPlaybackEventHandlerService> _playbackEventHandlerServiceMock = new();
    private readonly Mock<ILavalinkPlayer> _playerMock = new();
    private readonly Mock<IPlayerConnectionService> _playerConnectionServiceMock = new();
    private readonly Mock<IProgressiveTimerService> _progressiveTimerServiceMock = new();
    private readonly Mock<IResponseBuilder> _responseBuilderMock = new();
    private readonly PlaybackControlService _service;
    private readonly Mock<ITrackNotificationService> _trackNotificationServiceMock = new();
    private readonly Mock<IDiscordVoiceState> _voiceStateMock = new();
    private readonly Mock<IDiscordChannel> _voiceChannelMock = new();
    private readonly Mock<ILocalizationService> _localizationServiceMock = new();

    public PlaybackControlServiceTests()
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

        _service = new PlaybackControlService(
            _musicQueueServiceMock.Object,
            _responseBuilderMock.Object,
            _localizationServiceMock.Object,
            _trackNotificationServiceMock.Object,
            _playerConnectionServiceMock.Object,
            _playbackEventHandlerServiceMock.Object,
            _progressiveTimerServiceMock.Object,
            _loggerMock.Object);
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
        _playerMock.SetupGet(p => p.CurrentTrack).Returns(new LavalinkTrack
        {
            Author = "Test author",
            Title = "Track",
            Identifier = "id"
        });
        _playerConnectionServiceMock
            .Setup(p => p.TryGetAndValidateExistingPlayerAsync(_messageMock.Object, _voiceChannelMock.Object))
            .ReturnsAsync((_playerMock.Object, _textChannelMock.Object, GuildId, true));

        await _service.PauseAsync(_messageMock.Object, _memberMock.Object);

        _playerMock.Verify(p => p.PauseAsync(CancellationToken.None), Times.Once);
    }

    [Fact]
    public async Task PauseAsync_PauseThrows_SendsValidationError()
    {
        _playerMock.SetupGet(p => p.CurrentTrack).Returns(new LavalinkTrack { Author = "A", Title = "T", Identifier = "id" });
        _playerMock.Setup(p => p.PauseAsync(CancellationToken.None))
            .ThrowsAsync(new InvalidOperationException("pause fail"));
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
        _playerMock.SetupGet(p => p.CurrentTrack).Returns(new LavalinkTrack
        {
            Author = "Test author",
            Title = "Track",
            Identifier = "id"
        });
        _playerConnectionServiceMock
            .Setup(p => p.TryGetAndValidateExistingPlayerAsync(_messageMock.Object, _voiceChannelMock.Object))
            .ReturnsAsync((_playerMock.Object, _textChannelMock.Object, GuildId, true));

        await _service.ResumeAsync(_messageMock.Object, _memberMock.Object);

        _playerMock.Verify(p => p.ResumeAsync(CancellationToken.None), Times.Once);
    }

    [Fact]
    public async Task ResumeAsync_ResumeThrows_SendsValidationError()
    {
        _playerMock.SetupGet(p => p.CurrentTrack).Returns(new LavalinkTrack { Author = "A", Title = "T", Identifier = "id" });
        _playerMock.Setup(p => p.ResumeAsync(CancellationToken.None))
            .ThrowsAsync(new InvalidOperationException("resume fail"));
        _playerConnectionServiceMock
            .Setup(p => p.TryGetAndValidateExistingPlayerAsync(_messageMock.Object, _voiceChannelMock.Object))
            .ReturnsAsync((_playerMock.Object, _textChannelMock.Object, GuildId, true));

        await _service.ResumeAsync(_messageMock.Object, _memberMock.Object);

        _responseBuilderMock.Verify(
            r => r.SendValidationErrorAsync(_messageMock.Object, ValidationErrorKeys.LavalinkError), Times.Once);
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
        _playerMock.SetupGet(p => p.CurrentTrack).Returns(new LavalinkTrack
        {
            Author = "Test author",
            Title = "Track",
            Identifier = "id"
        });
        _playerConnectionServiceMock
            .Setup(p => p.TryGetAndValidateExistingPlayerAsync(_messageMock.Object, _voiceChannelMock.Object))
            .ReturnsAsync((_playerMock.Object, _textChannelMock.Object, GuildId, true));

        await _service.SkipAsync(_messageMock.Object, _memberMock.Object);

        _playerMock.Verify(p => p.StopAsync(CancellationToken.None), Times.Once);
        _progressiveTimerServiceMock.Verify(t => t.Stop(GuildId), Times.Once);
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
    public async Task SkipAsync_StopThrows_SendsValidationError()
    {
        _playerMock.SetupGet(p => p.CurrentTrack).Returns(new LavalinkTrack { Author = "A", Title = "T", Identifier = "id" });
        _playerMock.Setup(p => p.StopAsync(CancellationToken.None))
            .ThrowsAsync(new InvalidOperationException("stop fail"));
        _playerConnectionServiceMock
            .Setup(p => p.TryGetAndValidateExistingPlayerAsync(_messageMock.Object, _voiceChannelMock.Object))
            .ReturnsAsync((_playerMock.Object, _textChannelMock.Object, GuildId, true));

        await _service.SkipAsync(_messageMock.Object, _memberMock.Object);

        _responseBuilderMock.Verify(
            r => r.SendValidationErrorAsync(_messageMock.Object, ValidationErrorKeys.LavalinkError), Times.Once);
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
        _playerMock.Setup(p => p.DisconnectAsync(CancellationToken.None))
            .ThrowsAsync(new InvalidOperationException("disconnect fail"));

        _playerConnectionServiceMock
            .Setup(p => p.TryGetAndValidateExistingPlayerAsync(_messageMock.Object, _voiceChannelMock.Object))
            .ReturnsAsync((_playerMock.Object, _voiceChannelMock.Object, GuildId, true));

        await _service.LeaveVoiceChannel(_messageMock.Object, _memberMock.Object);

        _responseBuilderMock.Verify(
            r => r.SendValidationErrorAsync(_messageMock.Object, ValidationErrorKeys.LavalinkError), Times.Once);
    }
}
