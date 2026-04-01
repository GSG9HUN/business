using DC_bot_tests.TestHelperFiles;
using DC_bot.Interface;
using DC_bot.Interface.Discord;
using DC_bot.Interface.Service.Music.MusicServiceInterface;
using DC_bot.Service.Music.MusicServices;
using Lavalink4NET.Events.Players;
using Lavalink4NET.Players;
using Lavalink4NET.Protocol.Payloads.Events;
using Lavalink4NET.Tracks;
using Microsoft.Extensions.Logging;
using Moq;

namespace DC_bot_tests.UnitTests.Service.Music;

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
            _loggerMock.Object);
    }

    #region HandleTrackEndedAsync - Do nothing when guild ID mismatch
    [Fact]
    public async Task HandleTrackEndedAsync_GuildIdMismatch_DoesNothing()
    {
        // Arrange
        var args = CreateTrackEndedEventArgs(TrackEndReason.Finished);
        _playerMock.Setup(p => p.GuildId).Returns(999UL); // Different guild ID

        // Act
        await _service.HandleTrackEndedAsync(_playerMock.Object, args, _textChannelMock.Object);

        // Assert - should not call any services since guild ID doesn't match
        _repeatServiceMock.Verify(r => r.IsRepeating(It.IsAny<ulong>()), Times.Never);
        _musicQueueServiceMock.Verify(q => q.HasTracks(It.IsAny<ulong>()), Times.Never);
        _trackNotificationServiceMock.Verify(n => n.NotifyQueueEmptyAsync(It.IsAny<IDiscordChannel>()), Times.Never);
    }

    #endregion

    #region HandleTrackEndedAsync - Play next from queue

    [Fact]
    public async Task HandleTrackEndedAsync_NoRepeat_QueueHasTracks_PlaysNext()
    {
        // Arrange
        var args = CreateTrackEndedEventArgs(TrackEndReason.Finished);

        _repeatServiceMock.Setup(r => r.IsRepeating(GuildId)).Returns(false);
        _musicQueueServiceMock.Setup(q => q.HasTracks(GuildId)).Returns(true);

        // Act
        await _service.HandleTrackEndedAsync(_playerMock.Object, args, _textChannelMock.Object);

        // Assert
        _trackPlaybackServiceMock.Verify(p => p.PlayTrackFromQueueAsync(_playerMock.Object, _textChannelMock.Object),
            Times.Once);
        _trackNotificationServiceMock.Verify(n => n.NotifyQueueEmptyAsync(It.IsAny<IDiscordChannel>()), Times.Never);
    }

    #endregion

    #region HandleTrackEndedAsync - Queue empty notification

    [Fact]
    public async Task HandleTrackEndedAsync_NoRepeat_NoQueue_NotifiesEmpty()
    {
        // Arrange
        var args = CreateTrackEndedEventArgs(TrackEndReason.Finished);

        _repeatServiceMock.Setup(r => r.IsRepeating(GuildId)).Returns(false);
        _musicQueueServiceMock.Setup(q => q.HasTracks(GuildId)).Returns(false);
        _repeatServiceMock.Setup(r => r.IsRepeatingList(GuildId)).Returns(false);

        // Act
        await _service.HandleTrackEndedAsync(_playerMock.Object, args, _textChannelMock.Object);

        // Assert
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
        // Arrange
        var args = CreateTrackEndedEventArgs(reason);

        // Act
        await _service.HandleTrackEndedAsync(_playerMock.Object, args, _textChannelMock.Object);

        // Assert
        _repeatServiceMock.Verify(r => r.IsRepeating(It.IsAny<ulong>()), Times.Never);
        _musicQueueServiceMock.Verify(q => q.HasTracks(It.IsAny<ulong>()), Times.Never);
        _trackNotificationServiceMock.Verify(n => n.NotifyQueueEmptyAsync(It.IsAny<IDiscordChannel>()), Times.Never);
    }

    [Theory]
    [InlineData(TrackEndReason.Finished)]
    [InlineData(TrackEndReason.Stopped)]
    public async Task HandleTrackEndedAsync_FinishedOrStopped_ProcessesTrackEnd(TrackEndReason reason)
    {
        // Arrange
        var args = CreateTrackEndedEventArgs(reason);
        _repeatServiceMock.Setup(r => r.IsRepeating(GuildId)).Returns(false);
        _musicQueueServiceMock.Setup(q => q.HasTracks(GuildId)).Returns(false);
        _repeatServiceMock.Setup(r => r.IsRepeatingList(GuildId)).Returns(false);

        // Act
        await _service.HandleTrackEndedAsync(_playerMock.Object, args, _textChannelMock.Object);

        // Assert - should reach NotifyQueueEmptyAsync since no repeat, no queue
        _trackNotificationServiceMock.Verify(n => n.NotifyQueueEmptyAsync(_textChannelMock.Object), Times.Once);
    }

    #endregion

    #region HandleTrackEndedAsync - Repeat current track

    [Fact]
    public async Task HandleTrackEndedAsync_RepeatOn_WithCurrentTrack_RepeatsTrack()
    {
        // Arrange
        var track = TrackTestHelper.CreateTrackWrapper("Repeat Me");
        var args = CreateTrackEndedEventArgs(TrackEndReason.Finished);
        ILavaLinkTrack? currentTrack = track;

        _repeatServiceMock.Setup(r => r.IsRepeating(GuildId)).Returns(true);
        _currentTrackServiceMock
            .Setup(c => c.TryGetCurrentTrack(GuildId, out currentTrack))
            .Returns(true);

        // Act
        await _service.HandleTrackEndedAsync(_playerMock.Object, args, _textChannelMock.Object);

        // Assert
        _playerMock.Verify(p => p.PlayAsync(track.ToLavalinkTrack(), It.IsAny<TrackPlayProperties>(), default), Times.Once);
        _musicQueueServiceMock.Verify(q => q.HasTracks(It.IsAny<ulong>()), Times.Never);
    }

    [Fact]
    public async Task HandleTrackEndedAsync_RepeatOn_NoCurrentTrack_FallsThrough()
    {
        // Arrange
        ILavaLinkTrack? nullTrack = null;
        var args = CreateTrackEndedEventArgs(TrackEndReason.Finished);

        _repeatServiceMock.Setup(r => r.IsRepeating(GuildId)).Returns(true);
        _currentTrackServiceMock
            .Setup(c => c.TryGetCurrentTrack(GuildId, out nullTrack))
            .Returns(false);

        _musicQueueServiceMock.Setup(q => q.HasTracks(GuildId)).Returns(false);
        _repeatServiceMock.Setup(r => r.IsRepeatingList(GuildId)).Returns(false);

        // Act
        await _service.HandleTrackEndedAsync(_playerMock.Object, args, _textChannelMock.Object);

        // Assert - should return early from TryRepeatCurrentTrack (track is null), then fall through to notify empty
        _playerMock.Verify(p => p.PlayAsync(It.IsAny<LavalinkTrack>(), It.IsAny<TrackPlayProperties>(), default),
            Times.Never);
    }

    #endregion

    #region HandleTrackEndedAsync - Repeat list

    [Fact]
    public async Task HandleTrackEndedAsync_RepeatList_RequeuesAndPlays()
    {
        // Arrange
        var track1 = TrackTestHelper.CreateTrackWrapper("Song1", "Artist1", "id", 100);
        var track2 = TrackTestHelper.CreateTrackWrapper("Song2", "Artist2", "id", 100);

        var repeatableQueue = new List<ILavaLinkTrack> { track1, track2 };
        var args = CreateTrackEndedEventArgs(TrackEndReason.Finished);

        _repeatServiceMock.Setup(r => r.IsRepeating(GuildId)).Returns(false);
        _musicQueueServiceMock.Setup(q => q.HasTracks(GuildId)).Returns(false);
        _repeatServiceMock.Setup(r => r.IsRepeatingList(GuildId)).Returns(true);
        _musicQueueServiceMock.Setup(q => q.GetRepeatableQueue(GuildId)).Returns(repeatableQueue);

        // Act
        await _service.HandleTrackEndedAsync(_playerMock.Object, args, _textChannelMock.Object);

        // Assert
        _musicQueueServiceMock.Verify(q => q.Enqueue(GuildId, It.IsAny<ILavaLinkTrack>()), Times.Exactly(2));
        _trackPlaybackServiceMock.Verify(p => p.PlayTrackFromQueueAsync(_playerMock.Object, _textChannelMock.Object),
            Times.Once);
        _trackNotificationServiceMock.Verify(n => n.NotifyQueueEmptyAsync(It.IsAny<IDiscordChannel>()), Times.Never);
    }

    [Fact]
    public async Task HandleTrackEndedAsync_RepeatList_QueueStillHasTracks_DoesNotRequeue()
    {
        // Arrange
        var args = CreateTrackEndedEventArgs(TrackEndReason.Finished);

        _repeatServiceMock.Setup(r => r.IsRepeating(GuildId)).Returns(false);
        _musicQueueServiceMock.Setup(q => q.HasTracks(GuildId)).Returns(false);
        _repeatServiceMock.Setup(r => r.IsRepeatingList(GuildId)).Returns(true);

        _musicQueueServiceMock.SetupSequence(q => q.HasTracks(GuildId))
            .Returns(false)
            .Returns(true);

        // Act
        await _service.HandleTrackEndedAsync(_playerMock.Object, args, _textChannelMock.Object);

        // Assert - should not requeue because HasTracks returned true inside TryRepeatListAndPlayAsync
        _musicQueueServiceMock.Verify(q => q.GetRepeatableQueue(It.IsAny<ulong>()), Times.Never);
    }

    #endregion

    #region Helper Methods

    private static TrackEndedEventArgs CreateTrackEndedEventArgs(TrackEndReason reason)
    {
        var track = new LavalinkTrack
        {
            Author ="Test", 
            Title = "Artist", 
            Identifier = "id",
            Duration = TimeSpan.FromSeconds(120)
        };
        var playerMock = new Mock<ILavalinkPlayer>();
        return new TrackEndedEventArgs(playerMock.Object, track, reason);
    }
    #endregion
}