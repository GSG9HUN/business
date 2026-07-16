using DC_bot.Constants;
using DC_bot.Interface;
using DC_bot.Interface.Discord;
using DC_bot.Interface.Service.Music;
using DC_bot.Interface.Service.Presentation;
using DC_bot.Service.Music;
using DSharpPlus.Entities;
using Lavalink4NET.Players;
using Lavalink4NET.Rest.Entities.Tracks;
using Lavalink4NET.Tracks;
using Microsoft.Extensions.Logging;
using Moq;

namespace DC_bot_tests.UnitTests.Service.Music;

[Trait("Category", "Unit")]
public class LavaLinkServiceTests
{
    private const ulong GuildId = 111UL;

    private readonly Mock<ICurrentTrackService> _currentTrackServiceMock = new();
    private readonly Mock<IDiscordChannel> _textChannelMock = new();
    private readonly Mock<IDiscordGuild> _guildMock = new();
    private readonly Mock<ILogger<LavaLinkService>> _loggerMock = new();
    private readonly Mock<IDiscordMember> _memberMock = new();
    private readonly Mock<IDiscordMessage> _messageMock = new();
    private readonly Mock<IMusicQueueService> _musicQueueServiceMock = new();
    private readonly Mock<ILavalinkNodeConnectionService> _lavalinkNodeConnectionServiceMock = new();
    private readonly Mock<IPlaybackControlService> _playbackControlServiceMock = new();
    private readonly Mock<IPlaybackEventHandlerService> _playbackEventHandlerServiceMock = new();
    private readonly Mock<ILavalinkPlayer> _playerMock = new();
    private readonly Mock<IPlayerConnectionService> _playerConnectionServiceMock = new();
    private readonly Mock<IPlaybackRequestService> _playbackRequestServiceMock = new();
    private readonly Mock<IRepeatService> _repeatServiceMock = new();
    private readonly Mock<IResponseBuilder> _responseBuilderMock = new();
    private readonly LavaLinkService _service;
    private readonly Mock<ITrackNotificationService> _trackNotificationServiceMock = new();
    private readonly Mock<IDiscordVoiceState> _voiceStateMock = new();
    private readonly Mock<IDiscordChannel> _voiceChannelMock = new();

    public LavaLinkServiceTests()
    {
        _loggerMock.Setup(x => x.IsEnabled(It.IsAny<LogLevel>())).Returns(true);

        _guildMock.Setup(g => g.Id).Returns(GuildId);
        _textChannelMock.Setup(c => c.Guild).Returns(_guildMock.Object);
        _voiceChannelMock.Setup(c => c.Guild).Returns(_guildMock.Object);
        _messageMock.SetupGet(m => m.Channel).Returns(_textChannelMock.Object);
        _voiceStateMock.SetupGet(v => v.Channel).Returns(_voiceChannelMock.Object);
        _memberMock.SetupGet(m => m.VoiceState).Returns(_voiceStateMock.Object);

        _service = new LavaLinkService(
            _musicQueueServiceMock.Object,
            _loggerMock.Object,
            _responseBuilderMock.Object,
            _repeatServiceMock.Object,
            _currentTrackServiceMock.Object,
            _trackNotificationServiceMock.Object,
            _playerConnectionServiceMock.Object,
            _playbackEventHandlerServiceMock.Object,
            _playbackRequestServiceMock.Object,
            _lavalinkNodeConnectionServiceMock.Object,
            _playbackControlServiceMock.Object);
    }

    [Fact]
    public async Task Init_DelegatesToRepeatService()
    {
        await _service.Init(GuildId);

        _repeatServiceMock.Verify(r => r.InitAsync(GuildId), Times.Once);
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
    public async Task ConnectAsync_DelegatesToLavalinkNodeConnectionService()
    {
        await _service.ConnectAsync();

        _lavalinkNodeConnectionServiceMock.Verify(s => s.ConnectAsync(), Times.Once);
    }

    [Fact]
    public async Task PlayAsyncUrl_DelegatesToPlaybackRequestService()
    {
        var url = new Uri("https://example.com");

        await _service.PlayAsyncUrl(_voiceChannelMock.Object, url, _messageMock.Object,
            TrackSearchMode.YouTube);

        _playbackRequestServiceMock.Verify(
            p => p.PlayAsyncUrl(_voiceChannelMock.Object, url, _messageMock.Object, TrackSearchMode.YouTube),
            Times.Once);
    }

    [Fact]
    public async Task PlayAsyncQuery_DelegatesToPlaybackRequestService()
    {
        await _service.PlayAsyncQuery(_voiceChannelMock.Object, "test query", _messageMock.Object,
            TrackSearchMode.YouTube);

        _playbackRequestServiceMock.Verify(
            p => p.PlayAsyncQuery(_voiceChannelMock.Object, "test query", _messageMock.Object, TrackSearchMode.YouTube),
            Times.Once);
    }

    [Fact]
    public async Task PauseAsync_DelegatesToPlaybackControlService()
    {
        await _service.PauseAsync(_messageMock.Object, _memberMock.Object);

        _playbackControlServiceMock.Verify(
            p => p.PauseAsync(_messageMock.Object, _memberMock.Object),
            Times.Once);
    }

    [Fact]
    public async Task ResumeAsync_DelegatesToPlaybackControlService()
    {
        await _service.ResumeAsync(_messageMock.Object, _memberMock.Object);

        _playbackControlServiceMock.Verify(
            p => p.ResumeAsync(_messageMock.Object, _memberMock.Object),
            Times.Once);
    }

    [Fact]
    public async Task SkipAsync_DelegatesToPlaybackControlService()
    {
        await _service.SkipAsync(_messageMock.Object, _memberMock.Object);

        _playbackControlServiceMock.Verify(
            p => p.SkipAsync(_messageMock.Object, _memberMock.Object),
            Times.Once);
    }

    [Fact]
    public async Task LeaveVoiceChannel_DelegatesToPlaybackControlService()
    {
        await _service.LeaveVoiceChannel(_messageMock.Object, _memberMock.Object);

        _playbackControlServiceMock.Verify(
            p => p.LeaveVoiceChannel(_messageMock.Object, _memberMock.Object),
            Times.Once);
    }

    [Fact]
    public async Task StartPlayingQueue_InvalidJoin_DoesNothing()
    {
        _playerConnectionServiceMock
            .Setup(p => p.TryJoinAndValidateAsync(_messageMock.Object, _voiceChannelMock.Object, It.IsAny<CancellationToken>()))
            .ReturnsAsync((null, null, 0UL, false));

        await _service.StartPlayingQueue(_messageMock.Object, _textChannelMock.Object, _memberMock.Object);

        _playbackEventHandlerServiceMock.Verify(
            h => h.RegisterPlaybackFinishedHandler(It.IsAny<ulong>(), It.IsAny<ILavalinkPlayer>(),
                It.IsAny<IDiscordChannel>()),
            Times.Never);
        _musicQueueServiceMock.Verify(q => q.Dequeue(It.IsAny<ulong>()), Times.Never);
        _playerMock.Verify(p => p.PlayAsync(It.IsAny<LavalinkTrack>(), It.IsAny<TrackPlayProperties>(),
            CancellationToken.None), Times.Never);
        _trackNotificationServiceMock.Verify(
            n => n.NotifyNowPlayingAsync(It.IsAny<IDiscordChannel>(), It.IsAny<ILavaLinkTrack>(), It.IsAny<TimeSpan>(),
                It.IsAny<TimeSpan>()), Times.Never);
        _currentTrackServiceMock.Verify(c => c.SetCurrentTrackAsync(It.IsAny<ulong>(), It.IsAny<ILavaLinkTrack>(),
            CancellationToken.None), Times.Never);
    }

    [Fact]
    public async Task StartPlayingQueue_QueueEmpty_RegistersHandlerButDoesNotPlay()
    {
        _playerConnectionServiceMock
            .Setup(p => p.TryJoinAndValidateAsync(_messageMock.Object, _voiceChannelMock.Object, It.IsAny<CancellationToken>()))
            .ReturnsAsync((_playerMock.Object, _voiceChannelMock.Object, GuildId, true));

        _musicQueueServiceMock.Setup(q => q.Dequeue(GuildId)).ReturnsAsync((ILavaLinkTrack?)null);

        await _service.StartPlayingQueue(_messageMock.Object, _textChannelMock.Object, _memberMock.Object);

        _playbackEventHandlerServiceMock.Verify(
            h => h.RegisterPlaybackFinishedHandler(GuildId, _playerMock.Object, _textChannelMock.Object), Times.Once);
        _playerMock.Verify(p => p.PlayAsync(It.IsAny<LavalinkTrack>(), It.IsAny<TrackPlayProperties>(),
            CancellationToken.None), Times.Never);
        _trackNotificationServiceMock.Verify(
            n => n.NotifyNowPlayingAsync(It.IsAny<IDiscordChannel>(), It.IsAny<ILavaLinkTrack>(), It.IsAny<TimeSpan>(),
                It.IsAny<TimeSpan>()), Times.Never);
        _currentTrackServiceMock.Verify(c => c.SetCurrentTrackAsync(It.IsAny<ulong>(), It.IsAny<ILavaLinkTrack>(),
            CancellationToken.None), Times.Never);
    }

    [Fact]
    public async Task StartPlayingQueue_PlayAsyncThrows_SendsValidationError_AndDoesNotSetCurrentTrack()
    {
        var nextTrack = TrackTestHelper.CreateTrackWrapper("Artist", "Title", "id", 120);

        _playerConnectionServiceMock
            .Setup(p => p.TryJoinAndValidateAsync(_messageMock.Object, _voiceChannelMock.Object, It.IsAny<CancellationToken>()))
            .ReturnsAsync((_playerMock.Object, _voiceChannelMock.Object, GuildId, true));

        _musicQueueServiceMock.Setup(q => q.Dequeue(GuildId)).ReturnsAsync(nextTrack);
        _playerMock.Setup(p => p.PlayAsync(nextTrack.ToLavalinkTrack(), It.IsAny<TrackPlayProperties>(),
                CancellationToken.None))
            .ThrowsAsync(new InvalidOperationException("play failed"));

        await _service.StartPlayingQueue(_messageMock.Object, _textChannelMock.Object, _memberMock.Object);

        _responseBuilderMock.Verify(
            r => r.SendValidationErrorAsync(_messageMock.Object, ValidationErrorKeys.LavalinkError), Times.Once);
        _trackNotificationServiceMock.Verify(
            n => n.NotifyNowPlayingAsync(It.IsAny<IDiscordChannel>(), It.IsAny<ILavaLinkTrack>(), It.IsAny<TimeSpan>(),
                It.IsAny<TimeSpan>()), Times.Never);
        _currentTrackServiceMock.Verify(c => c.SetCurrentTrackAsync(It.IsAny<ulong>(), It.IsAny<ILavaLinkTrack>(),
            CancellationToken.None), Times.Never);
    }

    [Fact]
    public async Task StartPlayingQueue_Success_PlaysNotifiesAndSetsCurrentTrack()
    {
        var nextTrack = TrackTestHelper.CreateTrackWrapper("Artist", "Title", "id", 120);

        _playerConnectionServiceMock
            .Setup(p => p.TryJoinAndValidateAsync(_messageMock.Object, _voiceChannelMock.Object, It.IsAny<CancellationToken>()))
            .ReturnsAsync((_playerMock.Object, _voiceChannelMock.Object, GuildId, true));

        _musicQueueServiceMock.Setup(q => q.Dequeue(GuildId)).ReturnsAsync(nextTrack);

        await _service.StartPlayingQueue(_messageMock.Object, _textChannelMock.Object, _memberMock.Object);

        _playbackEventHandlerServiceMock.Verify(
            h => h.RegisterPlaybackFinishedHandler(GuildId, _playerMock.Object, _textChannelMock.Object), Times.Once);
        _playerMock.Verify(p => p.PlayAsync(nextTrack.ToLavalinkTrack(), It.IsAny<TrackPlayProperties>(),
            CancellationToken.None), Times.Once);
        _trackNotificationServiceMock.Verify(
            n => n.NotifyNowPlayingAsync(_textChannelMock.Object, nextTrack, TimeSpan.Zero, nextTrack.Duration),
            Times.Once);
        _currentTrackServiceMock.Verify(c => c.SetCurrentTrackAsync(GuildId, nextTrack, CancellationToken.None),
            Times.Once);
        _responseBuilderMock.Verify(
            r => r.SendValidationErrorAsync(It.IsAny<IDiscordMessage>(), It.IsAny<string>()), Times.Never);
    }
}
