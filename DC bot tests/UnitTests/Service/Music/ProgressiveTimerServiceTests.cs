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
    public async Task StartAsync_Logging_Player_NotFound_When_No_Players()
    {
        var guildId = 555UL;
        var messageMock = new Mock<IDiscordMessage>();
        _audioServiceMock.Setup(a => a.Players.Players).Returns(Array.Empty<ILavalinkPlayer>());

        await _timerService.StartAsync(messageMock.Object, guildId);

        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Debug,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains($"No Lavalink player found for guild {guildId}")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
        
        _trackNotificationServiceMock.Verify(
            t => t.BuildNowPlayingEmbed(It.IsAny<ILavaLinkTrack>(), It.IsAny<TimeSpan>(), It.IsAny<TimeSpan>()),
            Times.Never);
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
        var firstPosition = new TaskCompletionSource<TimeSpan>(TaskCreationOptions.RunContinuationsAsynchronously);
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
            .Returns((ILavaLinkTrack _, TimeSpan position, TimeSpan _duration) =>
            {
                firstPosition.TrySetResult(position);
                return new DiscordEmbedBuilder();
            });

        await _timerService.StartAsync(messageMock.Object, guildId);

        var position = await firstPosition.Task.WaitAsync(TimeSpan.FromSeconds(1));

        _timerService.Stop(guildId);
        await Task.Delay(50);

        Assert.True(position < TimeSpan.FromMilliseconds(100));
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

    [Fact]
    public async Task StartAsync_WhenRestartedForSameGuild_StopsPreviousTimerAndKeepsNewTimerRunning()
    {
        var guildId = 789UL;
        var firstMessageMock = new Mock<IDiscordMessage>();
        var secondMessageMock = new Mock<IDiscordMessage>();
        var playerMock = new Mock<ILavalinkPlayer>();
        var firstMessageModified = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var secondMessageModified = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var firstModifyCount = 0;
        var secondModifyCount = 0;
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
        _trackNotificationServiceMock
            .Setup(t => t.BuildNowPlayingEmbed(It.IsAny<ILavaLinkTrack>(), It.IsAny<TimeSpan>(), It.IsAny<TimeSpan>()))
            .Returns(new DiscordEmbedBuilder());

        firstMessageMock.Setup(m => m.Content).Returns("first");
        firstMessageMock.Setup(m => m.ModifyAsync(It.IsAny<DiscordMessageBuilder>()))
            .Returns(() =>
            {
                Interlocked.Increment(ref firstModifyCount);
                firstMessageModified.TrySetResult();
                return Task.CompletedTask;
            });

        secondMessageMock.Setup(m => m.Content).Returns("second");
        secondMessageMock.Setup(m => m.ModifyAsync(It.IsAny<DiscordMessageBuilder>()))
            .Returns(() =>
            {
                Interlocked.Increment(ref secondModifyCount);
                secondMessageModified.TrySetResult();
                return Task.CompletedTask;
            });

        await _timerService.StartAsync(firstMessageMock.Object, guildId);
        await firstMessageModified.Task.WaitAsync(TimeSpan.FromSeconds(1));

        await _timerService.StartAsync(secondMessageMock.Object, guildId);
        await secondMessageModified.Task.WaitAsync(TimeSpan.FromSeconds(1));

        var firstCountAfterRestart = Volatile.Read(ref firstModifyCount);

        await Task.Delay(1200);

        _timerService.Stop(guildId);

        Assert.Equal(firstCountAfterRestart, Volatile.Read(ref firstModifyCount));
        Assert.True(Volatile.Read(ref secondModifyCount) >= 1);
    }

    [Fact]
    public async Task ResumeAsync_WhenTimerWasPaused_ContinuesPreviousMessageFromPausedPosition()
    {
        var guildId = 890UL;
        var messageMock = new Mock<IDiscordMessage>();
        var playerMock = new Mock<ILavalinkPlayer>();
        var pauseReady = new TaskCompletionSource<TimeSpan>(TaskCreationOptions.RunContinuationsAsynchronously);
        var resumedPositionCaptured = new TaskCompletionSource<TimeSpan>(TaskCreationOptions.RunContinuationsAsynchronously);
        var observeResume = 0;
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
        messageMock.Setup(m => m.Content).Returns("progress-message");
        messageMock.Setup(m => m.ModifyAsync(It.IsAny<DiscordMessageBuilder>())).Returns(Task.CompletedTask);
        _trackNotificationServiceMock
            .Setup(t => t.BuildNowPlayingEmbed(It.IsAny<ILavaLinkTrack>(), It.IsAny<TimeSpan>(), It.IsAny<TimeSpan>()))
            .Returns((ILavaLinkTrack _, TimeSpan position, TimeSpan duration) =>
            {
                if (position >= TimeSpan.FromMilliseconds(900))
                {
                    pauseReady.TrySetResult(position);
                }

                if (Volatile.Read(ref observeResume) == 1)
                {
                    resumedPositionCaptured.TrySetResult(position);
                }

                return new DiscordEmbedBuilder();
            });

        await _timerService.StartAsync(messageMock.Object, guildId);

        var pausedPosition = await pauseReady.Task.WaitAsync(TimeSpan.FromSeconds(3));
        _timerService.Pause(guildId);
        await Task.Delay(100);

        Interlocked.Exchange(ref observeResume, 1);
        await _timerService.ResumeAsync(guildId);

        var resumedPosition = await resumedPositionCaptured.Task.WaitAsync(TimeSpan.FromSeconds(1));
        _timerService.Stop(guildId);

        Assert.True(resumedPosition >= pausedPosition);
        messageMock.Verify(m => m.ModifyAsync(It.IsAny<DiscordMessageBuilder>()), Times.AtLeast(3));
    }
}
