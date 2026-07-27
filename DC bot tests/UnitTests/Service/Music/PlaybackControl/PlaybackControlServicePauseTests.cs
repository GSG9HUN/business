using DC_bot.Constants;
using DC_bot.Interface.Discord;
using Moq;

namespace DC_bot_tests.UnitTests.Service.Music.PlaybackControl;

[Trait("Category", "Unit")]
public class PlaybackControlServicePauseTests : PlaybackControlServiceTestBase
{
    [Fact]
    public async Task PauseAsync_InvalidPlayer_ReturnsWithoutSideEffects()
    {
        SetupInvalidPlayer();

        await Service.PauseAsync(MessageMock.Object, MemberMock.Object);

        TrackNotificationServiceMock.Verify(
            n => n.SendSafeAsync(It.IsAny<IDiscordChannel>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        ResponseBuilderMock.Verify(
            r => r.SendValidationErrorAsync(It.IsAny<IDiscordMessage>(), It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task PauseAsync_NoCurrentTrack_SendsNotification()
    {
        LocalizationServiceMock.Setup(l => l.Get(LocalizationKeys.PauseCommandError)).Returns("No track");
        SetupNoCurrentTrack();
        SetupValidPlayer();

        await Service.PauseAsync(MessageMock.Object, MemberMock.Object);

        TrackNotificationServiceMock.Verify(
            n => n.SendSafeAsync(TextChannelMock.Object, "No track", "PauseAsync.NoTrack"), Times.Once);
        PlayerMock.Verify(p => p.PauseAsync(CancellationToken.None), Times.Never);
    }

    [Fact]
    public async Task PauseAsync_WithCurrentTrack_PausesPlayerAndProgressiveTimer()
    {
        LocalizationServiceMock.Setup(l => l.Get(LocalizationKeys.PauseCommandResponse)).Returns("Paused");
        SetupCurrentTrack();
        SetupValidPlayer();

        await Service.PauseAsync(MessageMock.Object, MemberMock.Object);

        PlayerMock.Verify(p => p.PauseAsync(CancellationToken.None), Times.Once);
        ProgressiveTimerServiceMock.Verify(t => t.Pause(GuildId), Times.Once);
    }

    [Fact]
    public async Task PauseAsync_PauseThrows_SendsValidationError()
    {
        SetupCurrentTrack();
        PlayerMock.Setup(p => p.PauseAsync(CancellationToken.None))
            .ThrowsAsync(new InvalidOperationException("pause fail"));
        SetupValidPlayer();

        await Service.PauseAsync(MessageMock.Object, MemberMock.Object);

        ResponseBuilderMock.Verify(
            r => r.SendValidationErrorAsync(MessageMock.Object, ValidationErrorKeys.LavalinkError), Times.Once);
    }
}
