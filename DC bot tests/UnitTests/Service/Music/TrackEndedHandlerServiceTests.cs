using DC_bot_tests.TestHelperFiles;
using DC_bot.Interface;
using DC_bot.Interface.Discord;
using DC_bot.Interface.Service.Music.MusicServiceInterface;
using DC_bot.Interface.Service.Persistence;
using DC_bot.Service.Music.MusicServices;
using Lavalink4NET.Events.Players;
using Lavalink4NET.Players;
using Lavalink4NET.Protocol.Payloads.Events;
using Lavalink4NET.Tracks;
using Microsoft.Extensions.Logging;
using Moq;

namespace DC_bot_tests.UnitTests.Service.Music;

[Trait("Category", "Unit")]
public class TrackEndedHandlerServiceTests
{
    private const ulong GuildId = 111UL;
    private readonly Mock<ICurrentTrackService> _currentTrackServiceMock = new();
    private readonly Mock<IDiscordGuild> _guildMock = new();
    private readonly Mock<ILogger<TrackEndedHandlerService>> _loggerMock = new();
    private readonly Mock<IMusicQueueService> _musicQueueServiceMock = new();
    private readonly Mock<ILavalinkPlayer> _playerMock = new();
    private readonly Mock<IRepeatService> _repeatServiceMock = new();
    private readonly TrackEndedHandlerService _service;
    private readonly Mock<IDiscordChannel> _textChannelMock = new();
    private readonly Mock<ITrackNotificationService> _trackNotificationServiceMock = new();
    private readonly Mock<ITrackPlaybackService> _trackPlaybackServiceMock = new();
    private readonly Mock<IQueueRepository> _queueRepositoryMock = new();

    public TrackEndedHandlerServiceTests()
    {
        _loggerMock.Setup(x => x.IsEnabled(It.IsAny<LogLevel>())).Returns(true);
        _guildMock.Setup(g => g.Id).Returns(GuildId);
        _textChannelMock.Setup(c => c.Guild).Returns(_guildMock.Object);

        _service = new TrackEndedHandlerService(
            _repeatServiceMock.Object,
            _currentTrackServiceMock.Object,
            _musicQueueServiceMock.Object,
            _trackPlaybackServiceMock.Object,
            _trackNotificationServiceMock.Object,
            _queueRepositoryMock.Object,
            _loggerMock.Object);
    }

    #region HandleTrackEndedAsync - Do nothing when guild ID mismatch
    [Fact]
    public async Task HandleTrackEndedAsync_GuildIdMismatch_DoesNothing()
    {
        var args = CreateTrackEndedEventArgs(TrackEndReason.Finished);
        _playerMock.Setup(p => p.GuildId).Returns(999UL);

        await _service.HandleTrackEndedAsync(_playerMock.Object, args, _textChannelMock.Object);

        _repeatServiceMock.Verify(r => r.IsRepeatingAsync(It.IsAny<ulong>()), Times.Never);
        _musicQueueServiceMock.Verify(q => q.HasTracks(It.IsAny<ulong>()), Times.Never);
        _trackNotificationServiceMock.Verify(n => n.NotifyQueueEmptyAsync(It.IsAny<IDiscordChannel>()), Times.Never);
    }

    #endregion

    #region HandleTrackEndedAsync - Play next from queue

    [Fact]
    public async Task HandleTrackEndedAsync_NoRepeat_QueueHasTracks_PlaysNext()
    {
        var args = CreateTrackEndedEventArgs(TrackEndReason.Finished);

        _repeatServiceMock.Setup(r => r.IsRepeatingAsync(GuildId)).ReturnsAsync(false);
        _musicQueueServiceMock.Setup(q => q.HasTracks(GuildId)).ReturnsAsync(true);

        await _service.HandleTrackEndedAsync(_playerMock.Object, args, _textChannelMock.Object);

        _trackPlaybackServiceMock.Verify(p => p.PlayTrackFromQueueAsync(_playerMock.Object, _textChannelMock.Object),
            Times.Once);
        _trackNotificationServiceMock.Verify(n => n.NotifyQueueEmptyAsync(It.IsAny<IDiscordChannel>()), Times.Never);
    }

    #endregion

    #region HandleTrackEndedAsync - Queue empty notification

    [Fact]
    public async Task HandleTrackEndedAsync_NoRepeat_NoQueue_NotifiesEmpty()
    {
        var args = CreateTrackEndedEventArgs(TrackEndReason.Finished);

        _repeatServiceMock.Setup(r => r.IsRepeatingAsync(GuildId)).ReturnsAsync(false);
        _musicQueueServiceMock.Setup(q => q.HasTracks(GuildId)).ReturnsAsync(false);
        _repeatServiceMock.Setup(r => r.IsRepeatingListAsync(GuildId)).ReturnsAsync(false);

        await _service.HandleTrackEndedAsync(_playerMock.Object, args, _textChannelMock.Object);

        _trackNotificationServiceMock.Verify(n => n.NotifyQueueEmptyAsync(_textChannelMock.Object), Times.Once);
    }

    #endregion

    #region HandleTrackEndedAsync - Reason filtering

    [Theory]
    [InlineData(TrackEndReason.Replaced)]
    [InlineData(TrackEndReason.LoadFailed)]
    [InlineData(TrackEndReason.Cleanup)]
    public async Task HandleTrackEndedAsync_NonFinishedReason_DoesNothing(TrackEndReason reason)
    {
        var args = CreateTrackEndedEventArgs(reason);

        await _service.HandleTrackEndedAsync(_playerMock.Object, args, _textChannelMock.Object);

        _repeatServiceMock.Verify(r => r.IsRepeatingAsync(It.IsAny<ulong>()), Times.Never);
        _musicQueueServiceMock.Verify(q => q.HasTracks(It.IsAny<ulong>()), Times.Never);
        _trackNotificationServiceMock.Verify(n => n.NotifyQueueEmptyAsync(It.IsAny<IDiscordChannel>()), Times.Never);
    }

    [Theory]
    [InlineData(TrackEndReason.Finished)]
    [InlineData(TrackEndReason.Stopped)]
    public async Task HandleTrackEndedAsync_FinishedOrStopped_ProcessesTrackEnd(TrackEndReason reason)
    {
        var args = CreateTrackEndedEventArgs(reason);
        _repeatServiceMock.Setup(r => r.IsRepeatingAsync(GuildId)).ReturnsAsync(false);
        _musicQueueServiceMock.Setup(q => q.HasTracks(GuildId)).ReturnsAsync(false);
        _repeatServiceMock.Setup(r => r.IsRepeatingListAsync(GuildId)).ReturnsAsync(false);

        await _service.HandleTrackEndedAsync(_playerMock.Object, args, _textChannelMock.Object);

        _trackNotificationServiceMock.Verify(n => n.NotifyQueueEmptyAsync(_textChannelMock.Object), Times.Once);
    }

    #endregion

    #region HandleTrackEndedAsync - Repeat current track

    [Fact]
    public async Task HandleTrackEndedAsync_RepeatOn_WithCurrentTrack_RepeatsTrack()
    {
        var track = TrackTestHelper.CreateTrackWrapper("Repeat Me");
        var args = CreateTrackEndedEventArgs(TrackEndReason.Finished);

        _repeatServiceMock.Setup(r => r.IsRepeatingAsync(GuildId)).ReturnsAsync(true);
        _currentTrackServiceMock
            .Setup(c => c.GetCurrentTrackAsync(GuildId, CancellationToken.None))
            .ReturnsAsync(track);

        await _service.HandleTrackEndedAsync(_playerMock.Object, args, _textChannelMock.Object);

        _playerMock.Verify(p => p.PlayAsync(track.ToLavalinkTrack(), It.IsAny<TrackPlayProperties>(), CancellationToken.None), Times.Once);
        _musicQueueServiceMock.Verify(q => q.HasTracks(It.IsAny<ulong>()), Times.Never);
    }

    [Fact]
    public async Task HandleTrackEndedAsync_RepeatOn_NoCurrentTrack_FallsThrough()
    {
        var args = CreateTrackEndedEventArgs(TrackEndReason.Finished);

        _repeatServiceMock.Setup(r => r.IsRepeatingAsync(GuildId)).ReturnsAsync(true);
        _currentTrackServiceMock
            .Setup(c => c.GetCurrentTrackAsync(GuildId, CancellationToken.None))
            .ReturnsAsync((ILavaLinkTrack?)null);

        _musicQueueServiceMock.Setup(q => q.HasTracks(GuildId)).ReturnsAsync(false);
        _repeatServiceMock.Setup(r => r.IsRepeatingListAsync(GuildId)).ReturnsAsync(false);

        await _service.HandleTrackEndedAsync(_playerMock.Object, args, _textChannelMock.Object);

        _playerMock.Verify(p => p.PlayAsync(It.IsAny<LavalinkTrack>(), It.IsAny<TrackPlayProperties>(), CancellationToken.None),
            Times.Never);
        _trackNotificationServiceMock.Verify(n => n.NotifyQueueEmptyAsync(_textChannelMock.Object), Times.Once);
    }

    #endregion

    #region HandleTrackEndedAsync - Repeat list

    [Fact]
    public async Task HandleTrackEndedAsync_RepeatList_RequeuesAndPlays()
    {
        var track1 = TrackTestHelper.CreateTrackWrapper("Song1", "Artist1");
        var track2 = TrackTestHelper.CreateTrackWrapper("Song2", "Artist2");

        var repeatableQueue = new List<ILavaLinkTrack> { track1, track2 };
        var args = CreateTrackEndedEventArgs(TrackEndReason.Finished);

        _repeatServiceMock.Setup(r => r.IsRepeatingAsync(GuildId)).ReturnsAsync(false);
        _musicQueueServiceMock.Setup(q => q.HasTracks(GuildId)).ReturnsAsync(false);
        _repeatServiceMock.Setup(r => r.IsRepeatingListAsync(GuildId)).ReturnsAsync(true);
        _musicQueueServiceMock.Setup(q => q.GetRepeatableQueue(GuildId)).ReturnsAsync(repeatableQueue);

        await _service.HandleTrackEndedAsync(_playerMock.Object, args, _textChannelMock.Object);

        _musicQueueServiceMock.Verify(q => q.EnqueueMany(
                GuildId,
                It.Is<IReadOnlyCollection<ILavaLinkTrack>>(tracks => tracks.Count == 2)),
            Times.Once);
        _musicQueueServiceMock.Verify(q => q.Enqueue(GuildId, It.IsAny<ILavaLinkTrack>()), Times.Never);
        _trackPlaybackServiceMock.Verify(p => p.PlayTrackFromQueueAsync(_playerMock.Object, _textChannelMock.Object),
            Times.Once);
        _trackNotificationServiceMock.Verify(n => n.NotifyQueueEmptyAsync(It.IsAny<IDiscordChannel>()), Times.Never);
    }

    [Fact]
    public async Task HandleTrackEndedAsync_RepeatList_QueueStillHasTracks_DoesNotRequeue()
    {
        var args = CreateTrackEndedEventArgs(TrackEndReason.Finished);

        _repeatServiceMock.Setup(r => r.IsRepeatingAsync(GuildId)).ReturnsAsync(false);
        _musicQueueServiceMock.Setup(q => q.HasTracks(GuildId)).ReturnsAsync(false);
        _repeatServiceMock.Setup(r => r.IsRepeatingListAsync(GuildId)).ReturnsAsync(true);

        _musicQueueServiceMock.SetupSequence(q => q.HasTracks(GuildId))
            .ReturnsAsync(false)
            .ReturnsAsync(true);

        await _service.HandleTrackEndedAsync(_playerMock.Object, args, _textChannelMock.Object);

        _musicQueueServiceMock.Verify(q => q.GetRepeatableQueue(It.IsAny<ulong>()), Times.Never);
    }


    [Fact]
    public async Task HandleTrackEndedAsync_RepeatListEnabled_ButSnapshotEmpty_NotifiesQueueEmpty()
    {
        var args = CreateTrackEndedEventArgs(TrackEndReason.Finished);

        _repeatServiceMock.Setup(r => r.IsRepeatingAsync(GuildId)).ReturnsAsync(false);
        _repeatServiceMock.Setup(r => r.IsRepeatingListAsync(GuildId)).ReturnsAsync(true);
        _musicQueueServiceMock.SetupSequence(q => q.HasTracks(GuildId))
            .ReturnsAsync(false)
            .ReturnsAsync(false);
        _musicQueueServiceMock.Setup(q => q.GetRepeatableQueue(GuildId))
            .ReturnsAsync(Array.Empty<ILavaLinkTrack>());

        await _service.HandleTrackEndedAsync(_playerMock.Object, args, _textChannelMock.Object);

        _musicQueueServiceMock.Verify(q => q.Enqueue(It.IsAny<ulong>(), It.IsAny<ILavaLinkTrack>()), Times.Never);
        _trackPlaybackServiceMock.Verify(p => p.PlayTrackFromQueueAsync(It.IsAny<ILavalinkPlayer>(), It.IsAny<IDiscordChannel>()), Times.Never);
        _trackNotificationServiceMock.Verify(n => n.NotifyQueueEmptyAsync(_textChannelMock.Object), Times.Once);
    }

    #endregion

    #region Helper Methods

    private static TrackEndedEventArgs CreateTrackEndedEventArgs(TrackEndReason reason)
    {
        var track = new LavalinkTrack
        {
            Author = "Test",
            Title = "Artist",
            Identifier = "id",
            Duration = TimeSpan.FromSeconds(120)
        };
        var playerMock = new Mock<ILavalinkPlayer>();
        return new TrackEndedEventArgs(playerMock.Object, track, reason);
    }
    #endregion
}
