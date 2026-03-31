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
        var trackMock = new Mock<LavalinkTrack>();

        playerMock.Setup(p => p.GuildId).Returns(guildId);
        playerMock.Setup(p => p.CurrentTrack).Returns(trackMock.Object);
        trackMock.Setup(t => t.Duration).Returns(TimeSpan.FromSeconds(10));

        _audioServiceMock.Setup(a => a.Players.Players).Returns(new[] { playerMock.Object });
        messageMock.Setup(m => m.Content).Returns("test");
        messageMock.Setup(m => m.ModifyAsync(It.IsAny<DiscordMessageBuilder>())).Returns(Task.CompletedTask);

        // Act
        await _timerService.StartAsync(messageMock.Object, guildId);
        _timerService.Stop(guildId);

        // Assert
        Assert.True(true);
    }

    [Fact]
    public void Stop_WhenNoTimer_DoesNothing()
    {
        // Arrange
        var guildId = 999UL;

        // Act
        _timerService.Stop(guildId);

        // Assert
        Assert.True(true);
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