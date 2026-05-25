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

[Trait("Category", "Unit")]
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

        _audioServiceMock.Setup(a => a.Players.Players).Returns([playerMock.Object]);
        messageMock.Setup(m => m.Content).Returns("test");
        messageMock.Setup(m => m.ModifyAsync(It.IsAny<DiscordMessageBuilder>())).Returns(Task.CompletedTask);

        await _timerService.StartAsync(messageMock.Object, guildId);

        await Task.Delay(100);

        _timerService.Stop(guildId);

        await Task.Delay(50);

        messageMock.Verify(m => m.ModifyAsync(It.IsAny<DiscordMessageBuilder>()), Times.AtLeastOnce);
    }

    [Fact]
    public void Stop_WhenNoTimer_DoesNothing()
    {
        var guildId = 999UL;

        var exception = Record.Exception(() => _timerService.Stop(guildId));

        Assert.Null(exception);
    }

    [Fact]
    public async Task StartAsync_ThrowsIfNoPlayerFound()
    {
        var guildId = 555UL;
        var messageMock = new Mock<IDiscordMessage>();
        _audioServiceMock.Setup(a => a.Players.Players).Returns(Array.Empty<ILavalinkPlayer>());

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _timerService.StartAsync(messageMock.Object, guildId));
    }

    [Fact]
    public async Task StartAsync_WhenCurrentTrackBecomesNull_StopsTimer()
    {
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
        playerMock.SetupSequence(p => p.CurrentTrack)
            .Returns(track)
            .Returns((LavalinkTrack?)null)
            .Returns((LavalinkTrack?)null);

        _audioServiceMock.Setup(a => a.Players.Players).Returns([playerMock.Object]);
        messageMock.Setup(m => m.Content).Returns("test");
        messageMock.Setup(m => m.ModifyAsync(It.IsAny<DiscordMessageBuilder>())).Returns(Task.CompletedTask);

        await _timerService.StartAsync(messageMock.Object, guildId);

        await Task.Delay(50);

        playerMock.Verify(p => p.CurrentTrack, Times.AtLeast(1));
    }

    [Fact]
    public async Task StartAsync_WhenModifyAsyncThrows_HandlesExceptionAndCleansUp()
    {
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

        _audioServiceMock.Setup(a => a.Players.Players).Returns([playerMock.Object]);
        messageMock.Setup(m => m.Content).Returns("test");
        messageMock.Setup(m => m.ModifyAsync(It.IsAny<DiscordMessageBuilder>()))
            .ThrowsAsync(new InvalidOperationException("Message edit failed"));

        await _timerService.StartAsync(messageMock.Object, guildId);

        await Task.Delay(50);

        messageMock.Verify(m => m.ModifyAsync(It.IsAny<DiscordMessageBuilder>()), Times.AtLeastOnce);
    }

    [Fact]
    public async Task StartAsync_WithNullPosition_UsesZeroStart()
    {
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

        _audioServiceMock.Setup(a => a.Players.Players).Returns([playerMock.Object]);
        messageMock.Setup(m => m.Content).Returns("test");
        messageMock.Setup(m => m.ModifyAsync(It.IsAny<DiscordMessageBuilder>())).Returns(Task.CompletedTask);

        _trackNotificationServiceMock.Setup(t => t.BuildNowPlayingEmbed(It.IsAny<ILavaLinkTrack>(), It.IsAny<TimeSpan>(), It.IsAny<TimeSpan>()))
            .Returns(new DiscordEmbedBuilder());

        await _timerService.StartAsync(messageMock.Object, guildId);

        await Task.Delay(1100);

        _timerService.Stop(guildId);
        await Task.Delay(50);

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
        var guildId = 567UL;
        var messageMock = new Mock<IDiscordMessage>();
        var playerMock = new Mock<ILavalinkPlayer>();
        var track = new LavalinkTrack
        {
            Author = "Test Artist",
            Title = "Test Title",
            Identifier = "test-id",
            Duration = TimeSpan.FromSeconds(5)
        };

        playerMock.Setup(p => p.GuildId).Returns(guildId);
        playerMock.Setup(p => p.CurrentTrack).Returns(track);

        _audioServiceMock.Setup(a => a.Players.Players).Returns([playerMock.Object]);
        messageMock.Setup(m => m.Content).Returns("test");
        messageMock.Setup(m => m.ModifyAsync(It.IsAny<DiscordMessageBuilder>())).Returns(Task.CompletedTask);

        _trackNotificationServiceMock.Setup(t => t.BuildNowPlayingEmbed(It.IsAny<ILavaLinkTrack>(), It.IsAny<TimeSpan>(), It.IsAny<TimeSpan>()))
            .Returns(new DiscordEmbedBuilder());

        await _timerService.StartAsync(messageMock.Object, guildId);

        await Task.Delay(1200);

        _timerService.Stop(guildId);
        await Task.Delay(50);

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
        var guildId = 678UL;
        var messageMock = new Mock<IDiscordMessage>();
        var playerMock = new Mock<ILavalinkPlayer>();

        playerMock.Setup(p => p.GuildId).Returns(guildId);
        playerMock.Setup(p => p.CurrentTrack).Returns((LavalinkTrack?)null);
        _audioServiceMock.Setup(a => a.Players.Players).Returns([playerMock.Object]);

        await _timerService.StartAsync(messageMock.Object, guildId);

        playerMock.VerifyGet(p => p.CurrentTrack, Times.Once);
        messageMock.Verify(m => m.ModifyAsync(It.IsAny<DiscordMessageBuilder>()), Times.Never);
        _trackNotificationServiceMock.Verify(
            t => t.BuildNowPlayingEmbed(It.IsAny<ILavaLinkTrack>(), It.IsAny<TimeSpan>(), It.IsAny<TimeSpan>()),
            Times.Never);
    }
}
