using DC_bot.Interface;
using DC_bot.Interface.Discord;
using DSharpPlus.Entities;
using Lavalink4NET.Players;
using Moq;

namespace DC_bot_tests.UnitTests.Service.Music.ProgressiveTimer;

[Trait("Category", "Unit")]
public class ProgressiveTimerServiceLifecycleTests : ProgressiveTimerServiceTestBase
{
    [Fact]
    public void Stop_WhenNoTimer_DoesNothing()
    {
        var exception = Record.Exception(() => TimerService.Stop(999UL));

        Assert.Null(exception);
    }

    [Fact]
    public async Task StartAsync_WhenRestartedForSameGuild_StopsPreviousTimerAndKeepsNewTimerRunning()
    {
        var guildId = 789UL;
        var firstMessageModified = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var secondMessageModified = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var firstModifyCount = 0;
        var secondModifyCount = 0;
        var firstMessageMock = CreateMessage("first", firstMessageModified, () => firstModifyCount++);
        var secondMessageMock = CreateMessage("second", secondMessageModified, () => secondModifyCount++);
        var playerMock = new Mock<ILavalinkPlayer>();
        playerMock.Setup(p => p.GuildId).Returns(guildId);
        playerMock.Setup(p => p.CurrentTrack).Returns(CreateTrack());
        AudioServiceMock.Setup(a => a.Players.Players).Returns([playerMock.Object]);
        TrackNotificationServiceMock
            .Setup(t => t.BuildNowPlayingEmbed(It.IsAny<ILavaLinkTrack>(), It.IsAny<TimeSpan>(), It.IsAny<TimeSpan>()))
            .Returns(new DiscordEmbedBuilder());

        await TimerService.StartAsync(firstMessageMock.Object, guildId);
        await firstMessageModified.Task.WaitAsync(TimeSpan.FromSeconds(1));
        var firstSession = ProgressTicker.LatestSession;
        await firstSession.WaitForDelayAsync();

        await TimerService.StartAsync(secondMessageMock.Object, guildId);
        await secondMessageModified.Task.WaitAsync(TimeSpan.FromSeconds(1));

        firstSession.AdvanceTo(TimeSpan.FromSeconds(1));
        TimerService.Stop(guildId);

        Assert.Equal(1, Volatile.Read(ref firstModifyCount));
        Assert.Equal(1, Volatile.Read(ref secondModifyCount));
    }

    [Fact]
    public async Task ResumeAsync_WhenTimerWasPaused_ContinuesPreviousMessageFromPausedPosition()
    {
        var guildId = 890UL;
        var secondPositionCaptured = new TaskCompletionSource<TimeSpan>(TaskCreationOptions.RunContinuationsAsynchronously);
        var resumedPositionCaptured = new TaskCompletionSource<TimeSpan>(TaskCreationOptions.RunContinuationsAsynchronously);
        var observeResume = 0;
        var messageMock = CreateMessage("progress-message");
        var playerMock = new Mock<ILavalinkPlayer>();
        var track = CreateTrack();

        playerMock.Setup(p => p.GuildId).Returns(guildId);
        playerMock.Setup(p => p.CurrentTrack).Returns(track);
        AudioServiceMock.Setup(a => a.Players.Players).Returns([playerMock.Object]);
        TrackNotificationServiceMock
            .Setup(t => t.BuildNowPlayingEmbed(It.IsAny<ILavaLinkTrack>(), It.IsAny<TimeSpan>(), It.IsAny<TimeSpan>()))
            .Returns((ILavaLinkTrack _, TimeSpan position, TimeSpan _) =>
            {
                if (position >= TimeSpan.FromSeconds(1) && Volatile.Read(ref observeResume) == 0)
                {
                    secondPositionCaptured.TrySetResult(position);
                }

                if (Volatile.Read(ref observeResume) == 1)
                {
                    resumedPositionCaptured.TrySetResult(position);
                }

                return new DiscordEmbedBuilder();
            });

        await TimerService.StartAsync(messageMock.Object, guildId);
        var firstSession = ProgressTicker.LatestSession;
        await firstSession.WaitForDelayAsync();
        firstSession.AdvanceTo(TimeSpan.FromSeconds(1));

        var pausedPosition = await secondPositionCaptured.Task.WaitAsync(TimeSpan.FromSeconds(1));
        TimerService.Pause(guildId);

        Interlocked.Exchange(ref observeResume, 1);
        await TimerService.ResumeAsync(guildId);

        var resumedPosition = await resumedPositionCaptured.Task.WaitAsync(TimeSpan.FromSeconds(1));
        TimerService.Stop(guildId);

        Assert.True(resumedPosition >= pausedPosition);
        messageMock.Verify(m => m.ModifyAsync(It.IsAny<DiscordMessageBuilder>()), Times.AtLeast(3));
    }

    private static Mock<IDiscordMessage> CreateMessage(
        string content,
        TaskCompletionSource? modified = null,
        Action? onModified = null)
    {
        var messageMock = new Mock<IDiscordMessage>();
        messageMock.Setup(m => m.Content).Returns(content);
        messageMock.Setup(m => m.ModifyAsync(It.IsAny<DiscordMessageBuilder>()))
            .Returns(() =>
            {
                onModified?.Invoke();
                modified?.TrySetResult();
                return Task.CompletedTask;
            });
        return messageMock;
    }
}
