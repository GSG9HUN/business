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
}