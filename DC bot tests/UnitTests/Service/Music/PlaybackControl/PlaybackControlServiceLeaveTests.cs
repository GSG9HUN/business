using DC_bot.Constants;
using Moq;

namespace DC_bot_tests.UnitTests.Service.Music.PlaybackControl;

[Trait("Category", "Unit")]
public class PlaybackControlServiceLeaveTests : PlaybackControlServiceTestBase
{
    [Fact]
    public async Task LeaveVoiceChannel_InvalidPlayer_DoesNothing()
    {
        SetupInvalidPlayer();

        await Service.LeaveVoiceChannel(MessageMock.Object, MemberMock.Object);

        PlaybackEventHandlerServiceMock.Verify(h => h.CleanupGuildAsync(It.IsAny<ulong>()), Times.Never);
        ProgressiveTimerServiceMock.Verify(t => t.Stop(It.IsAny<ulong>()), Times.Never);
        PlayerMock.Verify(p => p.DisconnectAsync(CancellationToken.None), Times.Never);
    }

    [Fact]
    public async Task LeaveVoiceChannel_ValidConnection_CurrentTrackExists_StopsCleansDisconnects()
    {
        var calls = new List<string>();
        SetupCurrentTrack();
        PlaybackEventHandlerServiceMock
            .Setup(h => h.CleanupGuildAsync(GuildId))
            .Callback(() => calls.Add("cleanup"))
            .Returns(Task.CompletedTask);
        PlayerMock
            .Setup(p => p.StopAsync(CancellationToken.None))
            .Callback(() => calls.Add("stop"))
            .Returns(new ValueTask());
        PlayerMock
            .Setup(p => p.DisconnectAsync(CancellationToken.None))
            .Callback(() => calls.Add("disconnect"))
            .Returns(new ValueTask());
        SetupValidPlayer(VoiceChannelMock.Object);

        await Service.LeaveVoiceChannel(MessageMock.Object, MemberMock.Object);

        PlayerMock.Verify(p => p.StopAsync(CancellationToken.None), Times.Once);
        PlaybackEventHandlerServiceMock.Verify(h => h.CleanupGuildAsync(GuildId), Times.Once);
        ProgressiveTimerServiceMock.Verify(t => t.Stop(GuildId), Times.Once);
        PlayerMock.Verify(p => p.DisconnectAsync(CancellationToken.None), Times.Once);
        Assert.Equal(["cleanup", "stop", "disconnect"], calls);
    }

    [Fact]
    public async Task LeaveVoiceChannel_ValidConnection_NoCurrentTrack_CleansDisconnectsWithoutStop()
    {
        SetupNoCurrentTrack();
        SetupValidPlayer(VoiceChannelMock.Object);

        await Service.LeaveVoiceChannel(MessageMock.Object, MemberMock.Object);

        PlayerMock.Verify(p => p.StopAsync(CancellationToken.None), Times.Never);
        PlaybackEventHandlerServiceMock.Verify(h => h.CleanupGuildAsync(GuildId), Times.Once);
        PlayerMock.Verify(p => p.DisconnectAsync(CancellationToken.None), Times.Once);
    }

    [Fact]
    public async Task LeaveVoiceChannel_DisconnectThrows_SendsValidationError()
    {
        SetupNoCurrentTrack();
        PlayerMock.Setup(p => p.DisconnectAsync(CancellationToken.None))
            .ThrowsAsync(new InvalidOperationException("disconnect fail"));
        SetupValidPlayer(VoiceChannelMock.Object);

        await Service.LeaveVoiceChannel(MessageMock.Object, MemberMock.Object);

        ResponseBuilderMock.Verify(
            r => r.SendValidationErrorAsync(MessageMock.Object, ValidationErrorKeys.LavalinkError), Times.Once);
    }
}
