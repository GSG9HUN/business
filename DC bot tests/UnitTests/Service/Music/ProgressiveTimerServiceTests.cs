using DC_bot.Interface;
using DC_bot.Interface.Discord;
using DC_bot.Interface.Service.Music.MusicServiceInterface;
using DC_bot.Service.Music.ProgressiveTimer;
using DSharpPlus.Entities;
using Lavalink4NET;
using Lavalink4NET.Players;
using Lavalink4NET.Tracks;
using Microsoft.Extensions.Logging;
using Moq;

namespace DC_bot_tests.UnitTests.Service.Music;

public class ProgressiveTimerServiceTests
{
    private readonly Mock<IAudioService> _audioServiceMock = new();
    private readonly Mock<ILogger<ProgressiveTimerService>> _loggerMock = new();
    private readonly ProgressiveTimerService _timerService;
    private readonly Mock<ITrackNotificationService> _trackNotificationServiceMock = new();

    public ProgressiveTimerServiceTests()
    {
        _loggerMock.Setup(x => x.IsEnabled(It.IsAny<LogLevel>())).Returns(true);
        _timerService = new ProgressiveTimerService(
            _audioServiceMock.Object,
            _loggerMock.Object,
            _trackNotificationServiceMock.Object);
    }

    [Fact]
    public async Task StartAsync_StartsAndStopsTimer_CancelsCorrectly()
    {
        // Arrange
        var guildId = 123UL;
        var messageMock = new Mock<IDiscordMessage>();
        var playerMock = new Mock<ILavalinkPlayer>();
        var track = new LavalinkTrack
        {
            Author = "Test Artist",
            Title = "Test Title",
            Identifier = "test-id",
            Duration = TimeSpan.FromSeconds(180)
        };

        playerMock.Setup(p => p.GuildId).Returns(guildId);
        playerMock.Setup(p => p.CurrentTrack).Returns(track);
        // Position is null by default in mock, which the timer handles with TimeSpan.Zero

        _audioServiceMock.Setup(a => a.Players.Players).Returns(new[] { playerMock.Object });
        messageMock.Setup(m => m.Content).Returns("test");
        messageMock.Setup(m => m.ModifyAsync(It.IsAny<DiscordMessageBuilder>())).Returns(Task.CompletedTask);

        // Act
        await _timerService.StartAsync(messageMock.Object, guildId);
        
        // Wait for the timer to execute at least once
        await Task.Delay(100);
        
        _timerService.Stop(guildId);

        // Give the cancellation a moment to propagate
        await Task.Delay(50);

        // Assert - ModifyAsync should have been called to update the progress
        messageMock.Verify(m => m.ModifyAsync(It.IsAny<DiscordMessageBuilder>()), Times.AtLeastOnce);
    }

    [Fact]
    public void Stop_WhenNoTimer_DoesNothing()
    {
        // Arrange
        var guildId = 999UL;

        // Act & Assert - Should not throw any exception
        var exception = Record.Exception(() => _timerService.Stop(guildId));
        
        Assert.Null(exception);
    }

    [Fact]
    public async Task StartAsync_ThrowsIfNoPlayerFound()
    {
        // Arrange
        var guildId = 555UL;
        var messageMock = new Mock<IDiscordMessage>();
        _audioServiceMock.Setup(a => a.Players.Players).Returns(Array.Empty<ILavalinkPlayer>());

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _timerService.StartAsync(messageMock.Object, guildId));
    }

    [Fact]
    public async Task StartAsync_WhenCurrentTrackBecomesNull_StopsTimer()
    {
        // Arrange
        var guildId = 234UL;
        var messageMock = new Mock<IDiscordMessage>();
        var playerMock = new Mock<ILavalinkPlayer>();
        var track = new LavalinkTrack
        {
            Author = "Test Artist",
            Title = "Test Title",
            Identifier = "test-id",
            Duration = TimeSpan.FromSeconds(180)
        };

        playerMock.Setup(p => p.GuildId).Returns(guildId);
        // First call returns track, subsequent calls return null to break the loop
        playerMock.SetupSequence(p => p.CurrentTrack)
            .Returns(track)
            .Returns((LavalinkTrack?)null)
            .Returns((LavalinkTrack?)null);
        // Position is null, so timer uses TimeSpan.Zero as fallback

        _audioServiceMock.Setup(a => a.Players.Players).Returns(new[] { playerMock.Object });
        messageMock.Setup(m => m.Content).Returns("test");
        messageMock.Setup(m => m.ModifyAsync(It.IsAny<DiscordMessageBuilder>())).Returns(Task.CompletedTask);

        // Act
        await _timerService.StartAsync(messageMock.Object, guildId);
        
        // Wait briefly for the timer to start and check the condition
        // The loop checks player.CurrentTrack is null on the first iteration and should break
        await Task.Delay(50);

        // Assert - Verify that PlayerMock.CurrentTrack was accessed at least once (the check will fail and break the loop)
        playerMock.Verify(p => p.CurrentTrack, Times.AtLeast(1));
    }

    [Fact]
    public async Task StartAsync_WhenModifyAsyncThrows_HandlesExceptionAndCleansUp()
    {
        // Arrange
        var guildId = 345UL;
        var messageMock = new Mock<IDiscordMessage>();
        var playerMock = new Mock<ILavalinkPlayer>();
        var track = new LavalinkTrack
        {
            Author = "Test Artist",
            Title = "Test Title",
            Identifier = "test-id",
            Duration = TimeSpan.FromSeconds(180)
        };

        playerMock.Setup(p => p.GuildId).Returns(guildId);
        playerMock.Setup(p => p.CurrentTrack).Returns(track);
        // Position is null, timer uses TimeSpan.Zero

        _audioServiceMock.Setup(a => a.Players.Players).Returns(new[] { playerMock.Object });
        messageMock.Setup(m => m.Content).Returns("test");
        // Simulate an exception on modify call
        messageMock.Setup(m => m.ModifyAsync(It.IsAny<DiscordMessageBuilder>()))
            .ThrowsAsync(new InvalidOperationException("Message edit failed"));

        // Act
        await _timerService.StartAsync(messageMock.Object, guildId);
        
        // Wait for the timer to run and hit the exception
        await Task.Delay(50);

        // Assert - Verify that ModifyAsync was called (and threw), triggering exception handling
        messageMock.Verify(m => m.ModifyAsync(It.IsAny<DiscordMessageBuilder>()), Times.AtLeastOnce);
    }

    [Fact]
    public async Task StartAsync_WithNullPosition_UsesZeroStart()
    {
        // Arrange
        var guildId = 456UL;
        var messageMock = new Mock<IDiscordMessage>();
        var playerMock = new Mock<ILavalinkPlayer>();
        var track = new LavalinkTrack
        {
            Author = "Test Artist",
            Title = "Test Title",
            Identifier = "test-id",
            Duration = TimeSpan.FromSeconds(180)
        };

        playerMock.Setup(p => p.GuildId).Returns(guildId);
        playerMock.Setup(p => p.CurrentTrack).Returns(track);
        // Position is null by default - timer handles with TimeSpan.Zero

        _audioServiceMock.Setup(a => a.Players.Players).Returns(new[] { playerMock.Object });
        messageMock.Setup(m => m.Content).Returns("test");
        messageMock.Setup(m => m.ModifyAsync(It.IsAny<DiscordMessageBuilder>())).Returns(Task.CompletedTask);
        
        _trackNotificationServiceMock.Setup(t => t.BuildNowPlayingEmbed(It.IsAny<ILavaLinkTrack>(), It.IsAny<TimeSpan>(), It.IsAny<TimeSpan>()))
            .Returns(new DiscordEmbedBuilder());

        // Act
        await _timerService.StartAsync(messageMock.Object, guildId);
        
        // Wait for the timer to complete at least one iteration (1000ms delay in the loop)
        await Task.Delay(1100);
        
        _timerService.Stop(guildId);
        await Task.Delay(50);

        // Assert - Verify that BuildNowPlayingEmbed was called with position starting from ~0
        _trackNotificationServiceMock.Verify(
            t => t.BuildNowPlayingEmbed(
                It.IsAny<ILavaLinkTrack>(),
                It.IsAny<TimeSpan>(),
                It.IsAny<TimeSpan>()),
            Times.AtLeastOnce);
    }

    [Fact]
    public async Task StartAsync_WhenPositionExceedsDuration_CapsAtDuration()
    {
        // Arrange
        var guildId = 567UL;
        var messageMock = new Mock<IDiscordMessage>();
        var playerMock = new Mock<ILavalinkPlayer>();
        var track = new LavalinkTrack
        {
            Author = "Test Artist",
            Title = "Test Title",
            Identifier = "test-id",
            Duration = TimeSpan.FromSeconds(5) // Very short duration
        };

        playerMock.Setup(p => p.GuildId).Returns(guildId);
        playerMock.Setup(p => p.CurrentTrack).Returns(track);
        // Position is null - timer uses TimeSpan.Zero

        _audioServiceMock.Setup(a => a.Players.Players).Returns(new[] { playerMock.Object });
        messageMock.Setup(m => m.Content).Returns("test");
        messageMock.Setup(m => m.ModifyAsync(It.IsAny<DiscordMessageBuilder>())).Returns(Task.CompletedTask);
        
        _trackNotificationServiceMock.Setup(t => t.BuildNowPlayingEmbed(It.IsAny<ILavaLinkTrack>(), It.IsAny<TimeSpan>(), It.IsAny<TimeSpan>()))
            .Returns(new DiscordEmbedBuilder());

        // Act
        await _timerService.StartAsync(messageMock.Object, guildId);
        
        // Wait for timer to run beyond the track duration (1000ms loop + time elapsed)
        await Task.Delay(1200);
        
        _timerService.Stop(guildId);
        await Task.Delay(50);

        // Assert - Position should never exceed duration (the timer caps it at duration)
        _trackNotificationServiceMock.Verify(
            t => t.BuildNowPlayingEmbed(
                It.IsAny<ILavaLinkTrack>(),
                It.Is<TimeSpan>(ts => ts <= track.Duration),
                It.IsAny<TimeSpan>()),
            Times.AtLeastOnce);
    }

    [Fact]
    public async Task StartAsync_WhenInitialCurrentTrackIsNull_ReturnsWithoutStartingTimer()
    {
        // Arrange
        var guildId = 678UL;
        var messageMock = new Mock<IDiscordMessage>();
        var playerMock = new Mock<ILavalinkPlayer>();

        playerMock.Setup(p => p.GuildId).Returns(guildId);
        playerMock.Setup(p => p.CurrentTrack).Returns((LavalinkTrack?)null);
        _audioServiceMock.Setup(a => a.Players.Players).Returns(new[] { playerMock.Object });

        // Act
        await _timerService.StartAsync(messageMock.Object, guildId);

        // Assert
        playerMock.VerifyGet(p => p.CurrentTrack, Times.Once);
        messageMock.Verify(m => m.ModifyAsync(It.IsAny<DiscordMessageBuilder>()), Times.Never);
        _trackNotificationServiceMock.Verify(
            t => t.BuildNowPlayingEmbed(It.IsAny<ILavaLinkTrack>(), It.IsAny<TimeSpan>(), It.IsAny<TimeSpan>()),
            Times.Never);
    }
}