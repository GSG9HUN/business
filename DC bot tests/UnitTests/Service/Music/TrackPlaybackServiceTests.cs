using DC_bot.Constants;
using DC_bot.Interface.Discord;
using DC_bot.Interface.Service.Localization;
using DC_bot.Interface.Service.Music.MusicServiceInterface;
using DC_bot.Service.Music.MusicServices;
using DC_bot.Wrapper;
using Lavalink4NET.Players;
using Lavalink4NET.Rest.Entities.Tracks;
using Lavalink4NET.Tracks;
using Microsoft.Extensions.Logging;
using Moq;

namespace DC_bot_tests.UnitTests.Service.Music;

public class TrackPlaybackServiceTests
{
    private readonly Mock<IMusicQueueService> _musicQueueServiceMock = new();
    private readonly Mock<ITrackNotificationService> _trackNotificationServiceMock = new();
    private readonly Mock<ICurrentTrackService> _currentTrackServiceMock = new();
    private readonly Mock<ILocalizationService> _localizationServiceMock = new();
    private readonly Mock<ILogger<TrackPlaybackService>> _loggerMock = new();
    private readonly Mock<ILavalinkPlayer> _playerMock = new();
    private readonly Mock<IDiscordChannel> _textChannelMock = new();
    private readonly Mock<IDiscordGuild> _guildMock = new();
    private readonly TrackPlaybackService _service;

    private const ulong GuildId = 111UL;

    public TrackPlaybackServiceTests()
    {
        _loggerMock.Setup(x => x.IsEnabled(It.IsAny<LogLevel>())).Returns(true);
        _guildMock.Setup(g => g.Id).Returns(GuildId);
        _textChannelMock.Setup(c => c.Guild).Returns(_guildMock.Object);

        _service = new TrackPlaybackService(
            _musicQueueServiceMock.Object,
            _trackNotificationServiceMock.Object,
            _currentTrackServiceMock.Object,
            _localizationServiceMock.Object,
            _loggerMock.Object);
    }

    #region PlayTheFoundMusicAsync Tests

    [Fact]
    public async Task PlayTheFoundMusicAsync_SingleTrack_NoCurrentTrack_PlaysAndNotifies()
    {
        // Arrange
        var track = CreateLavalinkTrack("Song", "Artist");
        var searchQuery = new TrackLoadResult(track, null);

        _playerMock.Setup(p => p.CurrentTrack).Returns((LavalinkTrack?)null);
        _musicQueueServiceMock.Setup(q => q.Dequeue(GuildId)).Returns(track);

        // Act
        await _service.PlayTheFoundMusicAsync(searchQuery, _playerMock.Object, _textChannelMock.Object);

        // Assert
        _musicQueueServiceMock.Verify(q => q.Enqueue(GuildId, It.IsAny<LavaLinkTrackWrapper>()), Times.Once);
        _playerMock.Verify(p => p.PlayAsync(track, It.IsAny<TrackPlayProperties>(), default), Times.Once);
        _trackNotificationServiceMock.Verify(n => n.NotifyNowPlayingAsync(_textChannelMock.Object, track), Times.Once);
        _currentTrackServiceMock.Verify(c => c.SetCurrentTrack(GuildId, track), Times.Once);
    }

    [Fact]
    public async Task PlayTheFoundMusicAsync_SingleTrack_NoCurrentTrack_DequeueReturnsNull_DoesNothing()
    {
        // Arrange
        var track = CreateLavalinkTrack("Song", "Artist");
        var searchQuery = new TrackLoadResult(track, null);

        _playerMock.Setup(p => p.CurrentTrack).Returns((LavalinkTrack?)null);
        _musicQueueServiceMock.Setup(q => q.Dequeue(GuildId)).Returns((LavalinkTrack?)null);

        // Act
        await _service.PlayTheFoundMusicAsync(searchQuery, _playerMock.Object, _textChannelMock.Object);

        // Assert
        _playerMock.Verify(p => p.PlayAsync(It.IsAny<LavalinkTrack>(), It.IsAny<TrackPlayProperties>(), default), Times.Never);
    }

    [Fact]
    public async Task PlayTheFoundMusicAsync_SingleTrack_HasCurrentTrack_AddsToQueue()
    {
        // Arrange
        var existingTrack = CreateLavalinkTrack("Existing", "Artist");
        var newTrack = CreateLavalinkTrack("New", "Artist2");
        var searchQuery = new TrackLoadResult(newTrack, null);

        _playerMock.Setup(p => p.CurrentTrack).Returns(existingTrack);

        _localizationServiceMock
            .Setup(l => l.Get(LocalizationKeys.PlayCommandMusicAddedQueue))
            .Returns("Added to queue:");

        // Act
        await _service.PlayTheFoundMusicAsync(searchQuery, _playerMock.Object, _textChannelMock.Object);

        // Assert
        _musicQueueServiceMock.Verify(q => q.Enqueue(GuildId, It.IsAny<LavaLinkTrackWrapper>()), Times.Once);
        _trackNotificationServiceMock.Verify(
            n => n.SendSafeAsync(_textChannelMock.Object, It.Is<string>(s => s.Contains("Artist2") && s.Contains("New")), It.IsAny<string>()),
            Times.Once);
    }

    [Fact]
    public async Task PlayTheFoundMusicAsync_Playlist_HasCurrentTrack_NotifiesPlaylistQueued()
    {
        // Arrange
        var track1 = CreateLavalinkTrack("Song1", "Artist1");
        var track2 = CreateLavalinkTrack("Song2", "Artist2");
        var searchQuery = new TrackLoadResult(new[] { track1, track2 }, new PlaylistInformation("Playlist", null, null));

        _playerMock.Setup(p => p.CurrentTrack).Returns(track1);

        _localizationServiceMock
            .Setup(l => l.Get(LocalizationKeys.PlayCommandListAddedQueue))
            .Returns("Playlist added to queue");

        // Act
        await _service.PlayTheFoundMusicAsync(searchQuery, _playerMock.Object, _textChannelMock.Object);

        // Assert
        _musicQueueServiceMock.Verify(q => q.Enqueue(GuildId, It.IsAny<LavaLinkTrackWrapper>()), Times.Exactly(2));
        _trackNotificationServiceMock.Verify(
            n => n.SendSafeAsync(_textChannelMock.Object, "Playlist added to queue", It.IsAny<string>()),
            Times.Once);
    }

    [Fact]
    public async Task PlayTheFoundMusicAsync_PlayAsyncThrows_LogsErrorAndNotifies()
    {
        // Arrange
        var track = CreateLavalinkTrack("Song", "Artist");
        var searchQuery = new TrackLoadResult(track, null);

        _playerMock.Setup(p => p.CurrentTrack).Returns((LavalinkTrack?)null);
        _musicQueueServiceMock.Setup(q => q.Dequeue(GuildId)).Returns(track);
        _playerMock.Setup(p => p.PlayAsync(track, It.IsAny<TrackPlayProperties>(), default))
            .ThrowsAsync(new InvalidOperationException("play failed"));

        _localizationServiceMock
            .Setup(l => l.Get(ValidationErrorKeys.LavalinkError))
            .Returns("Lavalink error");

        // Act
        await _service.PlayTheFoundMusicAsync(searchQuery, _playerMock.Object, _textChannelMock.Object);

        // Assert
        _trackNotificationServiceMock.Verify(
            n => n.SendSafeAsync(_textChannelMock.Object, "Lavalink error", It.IsAny<string>()),
            Times.Once);
    }

    #endregion

    #region TryPlayNextTrackAsync Tests

    [Fact]
    public async Task TryPlayNextTrackAsync_QueueEmpty_DoesNothing()
    {
        // Arrange
        _musicQueueServiceMock.Setup(q => q.Dequeue(GuildId)).Returns((LavalinkTrack?)null);

        // Act
        await _service.TryPlayNextTrackAsync(_playerMock.Object, _textChannelMock.Object, GuildId);

        // Assert
        _playerMock.Verify(p => p.PlayAsync(It.IsAny<LavalinkTrack>(), It.IsAny<TrackPlayProperties>(), default), Times.Never);
    }

    [Fact]
    public async Task TryPlayNextTrackAsync_QueueHasTrack_PlaysAndNotifies()
    {
        // Arrange
        var track = CreateLavalinkTrack("Next", "Artist");
        _musicQueueServiceMock.Setup(q => q.Dequeue(GuildId)).Returns(track);

        // Act
        await _service.TryPlayNextTrackAsync(_playerMock.Object, _textChannelMock.Object, GuildId);

        // Assert
        _playerMock.Verify(p => p.PlayAsync(track, It.IsAny<TrackPlayProperties>(), default), Times.Once);
        _trackNotificationServiceMock.Verify(n => n.NotifyNowPlayingAsync(_textChannelMock.Object, track), Times.Once);
    }

    [Fact]
    public async Task TryPlayNextTrackAsync_PlayAsyncThrows_LogsErrorAndNotifies()
    {
        // Arrange
        var track = CreateLavalinkTrack("Next", "Artist");
        _musicQueueServiceMock.Setup(q => q.Dequeue(GuildId)).Returns(track);
        _playerMock.Setup(p => p.PlayAsync(track, It.IsAny<TrackPlayProperties>(), default))
            .ThrowsAsync(new InvalidOperationException("play failed"));

        _localizationServiceMock
            .Setup(l => l.Get(ValidationErrorKeys.LavalinkError))
            .Returns("Lavalink error");

        // Act
        await _service.TryPlayNextTrackAsync(_playerMock.Object, _textChannelMock.Object, GuildId);

        // Assert
        _trackNotificationServiceMock.Verify(
            n => n.SendSafeAsync(_textChannelMock.Object, "Lavalink error", It.IsAny<string>()),
            Times.Once);
    }

    #endregion

    #region PlayTrackFromQueueAsync Tests

    [Fact]
    public async Task PlayTrackFromQueueAsync_DelegatesToTryPlayNextTrackAsync()
    {
        // Arrange
        var track = CreateLavalinkTrack("Song", "Artist");
        _musicQueueServiceMock.Setup(q => q.Dequeue(GuildId)).Returns(track);

        // Act
        await _service.PlayTrackFromQueueAsync(_playerMock.Object, _textChannelMock.Object);

        // Assert
        _playerMock.Verify(p => p.PlayAsync(track, It.IsAny<TrackPlayProperties>(), default), Times.Once);
    }

    #endregion

    #region Helper Methods

    private static LavalinkTrack CreateLavalinkTrack(string title, string author)
    {
        return new LavalinkTrack
        {
            Title = title,
            Author = author,
            Identifier = Guid.NewGuid().ToString(),
            Duration = TimeSpan.FromMinutes(3)
        };
    }

    #endregion
}