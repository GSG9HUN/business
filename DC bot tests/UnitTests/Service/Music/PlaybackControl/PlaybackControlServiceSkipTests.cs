using DC_bot.Constants;
using DC_bot.Interface.Discord;
using Moq;

namespace DC_bot_tests.UnitTests.Service.Music.PlaybackControl;

[Trait("Category", "Unit")]
public class PlaybackControlServiceSkipTests : PlaybackControlServiceTestBase
{
    [Fact]
    public async Task SkipAsync_InvalidPlayer_ReturnsWithoutSideEffects()
    {
        SetupInvalidPlayer();

        await Service.SkipAsync(MessageMock.Object, MemberMock.Object);

        MusicQueueServiceMock.Verify(q => q.HasTracks(It.IsAny<ulong>()), Times.Never);
        ResponseBuilderMock.Verify(
            r => r.SendValidationErrorAsync(It.IsAny<IDiscordMessage>(), It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task SkipAsync_NoCurrentTrackAndQueueEmpty_SendsNotification()
    {
        LocalizationServiceMock.Setup(l => l.Get(LocalizationKeys.SkipCommandError)).Returns("Nothing to skip");
        SetupNoCurrentTrack();
        MusicQueueServiceMock.Setup(q => q.HasTracks(GuildId)).ReturnsAsync(false);
        SetupValidPlayer();

        await Service.SkipAsync(MessageMock.Object, MemberMock.Object);

        TrackNotificationServiceMock.Verify(
            n => n.SendSafeAsync(TextChannelMock.Object, "Nothing to skip", "SkipAsync.NoTrack"), Times.Once);
        PlayerMock.Verify(p => p.StopAsync(CancellationToken.None), Times.Never);
    }

    [Fact]
    public async Task SkipAsync_WithCurrentTrack_StopsPlayerAndProgressiveTimer()
    {
        var calls = new List<string>();
        SetupCurrentTrack();
        ProgressiveTimerServiceMock
            .Setup(t => t.Stop(GuildId))
            .Callback(() => calls.Add("timer-stop"));
        PlayerMock
            .Setup(p => p.StopAsync(CancellationToken.None))
            .Callback(() => calls.Add("player-stop"))
            .Returns(new ValueTask());
        SetupValidPlayer();

        await Service.SkipAsync(MessageMock.Object, MemberMock.Object);

        PlayerMock.Verify(p => p.StopAsync(CancellationToken.None), Times.Once);
        ProgressiveTimerServiceMock.Verify(t => t.Stop(GuildId), Times.Once);
        Assert.Equal(["timer-stop", "player-stop"], calls);
    }

    [Fact]
    public async Task SkipAsync_NoCurrentTrackButQueueHasTracks_StopsAndStopsTimer()
    {
        SetupNoCurrentTrack();
        MusicQueueServiceMock.Setup(q => q.HasTracks(GuildId)).ReturnsAsync(true);
        SetupValidPlayer();

        await Service.SkipAsync(MessageMock.Object, MemberMock.Object);

        PlayerMock.Verify(p => p.StopAsync(CancellationToken.None), Times.Once);
        ProgressiveTimerServiceMock.Verify(t => t.Stop(GuildId), Times.Once);
    }

    [Fact]
    public async Task SkipAsync_StopThrows_SendsValidationError()
    {
        SetupCurrentTrack();
        PlayerMock.Setup(p => p.StopAsync(CancellationToken.None))
            .ThrowsAsync(new InvalidOperationException("stop fail"));
        SetupValidPlayer();

        await Service.SkipAsync(MessageMock.Object, MemberMock.Object);

        ResponseBuilderMock.Verify(
            r => r.SendValidationErrorAsync(MessageMock.Object, ValidationErrorKeys.LavalinkError), Times.Once);
    }
}
