using DC_bot.Interface;
using DC_bot.Interface.Discord;
using DSharpPlus.Entities;
using Lavalink4NET.Players;
using Lavalink4NET.Tracks;
using Microsoft.Extensions.Logging;
using Moq;

namespace DC_bot_tests.UnitTests.Service.Music.ProgressiveTimer;

[Trait("Category", "Unit")]
public class ProgressiveTimerServiceStartTests : ProgressiveTimerServiceTestBase
{
    [Fact]
    public async Task StartAsync_StartsAndStopsTimer_CancelsCorrectly()
    {
        var guildId = 123UL;
        var messageModified = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var messageMock = CreateMessage(messageModified);
        var playerMock = SetupPlayer(guildId, CreateTrack());

        await TimerService.StartAsync(messageMock.Object, guildId);
        await messageModified.Task.WaitAsync(TimeSpan.FromSeconds(1));

        TimerService.Stop(guildId);

        messageMock.Verify(m => m.ModifyAsync(It.IsAny<DiscordMessageBuilder>()), Times.AtLeastOnce);
        playerMock.VerifyGet(p => p.CurrentTrack, Times.AtLeastOnce);
    }

    [Fact]
    public async Task StartAsync_Logging_Player_NotFound_When_No_Players()
    {
        var guildId = 555UL;
        var messageMock = new Mock<IDiscordMessage>();
        AudioServiceMock.Setup(a => a.Players.Players).Returns(Array.Empty<ILavalinkPlayer>());

        await TimerService.StartAsync(messageMock.Object, guildId);

        LoggerMock.Verify(
            x => x.Log(
                LogLevel.Debug,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, _) => v.ToString()!.Contains($"No Lavalink player found for guild {guildId}")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);

        TrackNotificationServiceMock.Verify(
            t => t.BuildNowPlayingEmbed(It.IsAny<ILavaLinkTrack>(), It.IsAny<TimeSpan>(), It.IsAny<TimeSpan>()),
            Times.Never);
    }

    [Fact]
    public async Task StartAsync_WhenCurrentTrackBecomesNull_StopsTimer()
    {
        var guildId = 234UL;
        var currentTrackReadCount = 0;
        var messageMock = new Mock<IDiscordMessage>();
        var playerMock = new Mock<ILavalinkPlayer>();
        var track = CreateTrack();

        playerMock.Setup(p => p.GuildId).Returns(guildId);
        playerMock.Setup(p => p.CurrentTrack).Returns(() =>
        {
            var reads = Interlocked.Increment(ref currentTrackReadCount);
            return reads == 1 ? track : null;
        });
        AudioServiceMock.Setup(a => a.Players.Players).Returns([playerMock.Object]);

        await TimerService.StartAsync(messageMock.Object, guildId);

        await AsyncTestWaiter.UntilAsync(
            () => Task.FromResult(Volatile.Read(ref currentTrackReadCount) >= 2),
            "Timer did not observe the track ending.",
            timeout: TimeSpan.FromSeconds(1),
            pollInterval: TimeSpan.FromMilliseconds(10));
        TrackNotificationServiceMock.Verify(
            t => t.BuildNowPlayingEmbed(It.IsAny<ILavaLinkTrack>(), It.IsAny<TimeSpan>(), It.IsAny<TimeSpan>()),
            Times.Never);
    }

    [Fact]
    public async Task StartAsync_WhenModifyAsyncThrows_HandlesExceptionAndCleansUp()
    {
        var guildId = 345UL;
        var modifyAttempted = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var messageMock = new Mock<IDiscordMessage>();
        SetupPlayer(guildId, CreateTrack());

        messageMock.Setup(m => m.Content).Returns("test");
        messageMock.Setup(m => m.ModifyAsync(It.IsAny<DiscordMessageBuilder>()))
            .Returns(() =>
            {
                modifyAttempted.TrySetResult();
                return Task.FromException(new InvalidOperationException("Message edit failed"));
            });
        TrackNotificationServiceMock
            .Setup(t => t.BuildNowPlayingEmbed(It.IsAny<ILavaLinkTrack>(), It.IsAny<TimeSpan>(), It.IsAny<TimeSpan>()))
            .Returns(new DiscordEmbedBuilder());

        await TimerService.StartAsync(messageMock.Object, guildId);

        await modifyAttempted.Task.WaitAsync(TimeSpan.FromSeconds(1));
        messageMock.Verify(m => m.ModifyAsync(It.IsAny<DiscordMessageBuilder>()), Times.Once);
    }

    [Fact]
    public async Task StartAsync_WithNullPosition_UsesZeroStart()
    {
        var guildId = 456UL;
        var firstPosition = new TaskCompletionSource<TimeSpan>(TaskCreationOptions.RunContinuationsAsynchronously);
        var messageMock = CreateMessage();
        SetupPlayer(guildId, CreateTrack());

        TrackNotificationServiceMock
            .Setup(t => t.BuildNowPlayingEmbed(It.IsAny<ILavaLinkTrack>(), It.IsAny<TimeSpan>(), It.IsAny<TimeSpan>()))
            .Returns((ILavaLinkTrack _, TimeSpan position, TimeSpan _) =>
            {
                firstPosition.TrySetResult(position);
                return new DiscordEmbedBuilder();
            });

        await TimerService.StartAsync(messageMock.Object, guildId);

        var position = await firstPosition.Task.WaitAsync(TimeSpan.FromSeconds(1));
        TimerService.Stop(guildId);

        Assert.Equal(TimeSpan.Zero, position);
    }

    [Fact]
    public async Task StartAsync_WhenPositionExceedsDuration_CapsAtDuration()
    {
        var guildId = 567UL;
        var track = CreateTrack(TimeSpan.FromSeconds(5));
        var durationPosition = new TaskCompletionSource<TimeSpan>(TaskCreationOptions.RunContinuationsAsynchronously);
        var messageMock = CreateMessage();
        SetupPlayer(guildId, track);

        TrackNotificationServiceMock
            .Setup(t => t.BuildNowPlayingEmbed(It.IsAny<ILavaLinkTrack>(), It.IsAny<TimeSpan>(), It.IsAny<TimeSpan>()))
            .Returns((ILavaLinkTrack _, TimeSpan position, TimeSpan _) =>
            {
                if (position == track.Duration)
                {
                    durationPosition.TrySetResult(position);
                }

                return new DiscordEmbedBuilder();
            });

        await TimerService.StartAsync(messageMock.Object, guildId);
        var session = ProgressTicker.LatestSession;
        await session.WaitForDelayAsync();

        session.AdvanceTo(TimeSpan.FromSeconds(10));

        var cappedPosition = await durationPosition.Task.WaitAsync(TimeSpan.FromSeconds(1));
        Assert.Equal(track.Duration, cappedPosition);
    }

    [Fact]
    public async Task StartAsync_WhenInitialCurrentTrackIsNull_ReturnsWithoutStartingTimer()
    {
        var guildId = 678UL;
        var messageMock = new Mock<IDiscordMessage>();
        var playerMock = SetupPlayer(guildId, null);

        await TimerService.StartAsync(messageMock.Object, guildId);

        playerMock.VerifyGet(p => p.CurrentTrack, Times.Once);
        messageMock.Verify(m => m.ModifyAsync(It.IsAny<DiscordMessageBuilder>()), Times.Never);
        TrackNotificationServiceMock.Verify(
            t => t.BuildNowPlayingEmbed(It.IsAny<ILavaLinkTrack>(), It.IsAny<TimeSpan>(), It.IsAny<TimeSpan>()),
            Times.Never);
    }

    private Mock<ILavalinkPlayer> SetupPlayer(ulong guildId, LavalinkTrack? track)
    {
        var playerMock = new Mock<ILavalinkPlayer>();
        playerMock.Setup(p => p.GuildId).Returns(guildId);
        playerMock.Setup(p => p.CurrentTrack).Returns(track);
        AudioServiceMock.Setup(a => a.Players.Players).Returns([playerMock.Object]);
        return playerMock;
    }

    private static Mock<IDiscordMessage> CreateMessage(TaskCompletionSource? modified = null)
    {
        var messageMock = new Mock<IDiscordMessage>();
        messageMock.Setup(m => m.Content).Returns("test");
        messageMock.Setup(m => m.ModifyAsync(It.IsAny<DiscordMessageBuilder>()))
            .Returns(() =>
            {
                modified?.TrySetResult();
                return Task.CompletedTask;
            });
        return messageMock;
    }
}
