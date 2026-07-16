using DC_bot.Constants;
using DC_bot.Interface.Discord;
using Moq;

namespace DC_bot_tests.UnitTests.Service.Music.PlaybackControl;

[Trait("Category", "Unit")]
public class PlaybackControlServiceResumeTests : PlaybackControlServiceTestBase
{
    [Fact]
    public async Task ResumeAsync_InvalidPlayer_ReturnsWithoutSideEffects()
    {
        SetupInvalidPlayer();

        await Service.ResumeAsync(MessageMock.Object, MemberMock.Object);

        TrackNotificationServiceMock.Verify(
            n => n.SendSafeAsync(It.IsAny<IDiscordChannel>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        ResponseBuilderMock.Verify(
            r => r.SendValidationErrorAsync(It.IsAny<IDiscordMessage>(), It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task ResumeAsync_NoCurrentTrack_SendsNotification()
    {
        LocalizationServiceMock.Setup(l => l.Get(LocalizationKeys.ResumeCommandError)).Returns("No paused track");
        SetupNoCurrentTrack();
        SetupValidPlayer();

        await Service.ResumeAsync(MessageMock.Object, MemberMock.Object);

        TrackNotificationServiceMock.Verify(
            n => n.SendSafeAsync(TextChannelMock.Object, "No paused track", "ResumeAsync.NoTrack"), Times.Once);
        PlayerMock.Verify(p => p.ResumeAsync(CancellationToken.None), Times.Never);
    }

    [Fact]
    public async Task ResumeAsync_WithCurrentTrack_ResumesPlayerAndProgressiveTimer()
    {
        LocalizationServiceMock.Setup(l => l.Get(LocalizationKeys.ResumeCommandResponse)).Returns("Resumed");
        SetupCurrentTrack();
        SetupValidPlayer();

        await Service.ResumeAsync(MessageMock.Object, MemberMock.Object);

        PlayerMock.Verify(p => p.ResumeAsync(CancellationToken.None), Times.Once);
        ProgressiveTimerServiceMock.Verify(t => t.ResumeAsync(GuildId), Times.Once);
    }

    [Fact]
    public async Task ResumeAsync_ResumeThrows_SendsValidationError()
    {
        SetupCurrentTrack();
        PlayerMock.Setup(p => p.ResumeAsync(CancellationToken.None))
            .ThrowsAsync(new InvalidOperationException("resume fail"));
        SetupValidPlayer();

        await Service.ResumeAsync(MessageMock.Object, MemberMock.Object);

        ResponseBuilderMock.Verify(
            r => r.SendValidationErrorAsync(MessageMock.Object, ValidationErrorKeys.LavalinkError), Times.Once);
    }
}
