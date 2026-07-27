using DC_bot.Interface.Discord;
using DC_bot.Interface.Service.Localization;
using DC_bot.Interface.Service.Music;
using DC_bot.Interface.Service.Music.ProgressiveTimerInterface;
using DC_bot.Interface.Service.Presentation;
using DC_bot.Service.Music.MusicServices;
using Lavalink4NET.Players;
using Lavalink4NET.Tracks;
using Microsoft.Extensions.Logging;
using Moq;

namespace DC_bot_tests.UnitTests.Service.Music.PlaybackControl;

public abstract class PlaybackControlServiceTestBase
{
    protected const ulong GuildId = 111UL;

    protected readonly Mock<IDiscordChannel> TextChannelMock = new();
    protected readonly Mock<IDiscordGuild> GuildMock = new();
    protected readonly Mock<ILogger<PlaybackControlService>> LoggerMock = new();
    protected readonly Mock<IDiscordMember> MemberMock = new();
    protected readonly Mock<IDiscordMessage> MessageMock = new();
    protected readonly Mock<IMusicQueueService> MusicQueueServiceMock = new();
    protected readonly Mock<IPlaybackEventHandlerService> PlaybackEventHandlerServiceMock = new();
    protected readonly Mock<ILavalinkPlayer> PlayerMock = new();
    protected readonly Mock<IPlayerConnectionService> PlayerConnectionServiceMock = new();
    protected readonly Mock<IProgressiveTimerService> ProgressiveTimerServiceMock = new();
    protected readonly Mock<IResponseBuilder> ResponseBuilderMock = new();
    protected readonly PlaybackControlService Service;
    protected readonly Mock<ITrackNotificationService> TrackNotificationServiceMock = new();
    protected readonly Mock<IDiscordVoiceState> VoiceStateMock = new();
    protected readonly Mock<IDiscordChannel> VoiceChannelMock = new();
    protected readonly Mock<ILocalizationService> LocalizationServiceMock = new();

    protected PlaybackControlServiceTestBase()
    {
        LoggerMock.Setup(x => x.IsEnabled(It.IsAny<LogLevel>())).Returns(true);

        GuildMock.Setup(g => g.Id).Returns(GuildId);
        TextChannelMock.Setup(c => c.Guild).Returns(GuildMock.Object);
        VoiceChannelMock.Setup(c => c.Guild).Returns(GuildMock.Object);
        MessageMock.SetupGet(m => m.Channel).Returns(TextChannelMock.Object);
        VoiceStateMock.SetupGet(v => v.Channel).Returns(VoiceChannelMock.Object);
        MemberMock.SetupGet(m => m.VoiceState).Returns(VoiceStateMock.Object);
        LocalizationServiceMock
            .Setup(l => l.Get(It.IsAny<ulong>(), It.IsAny<string>(), It.IsAny<object[]>()))
            .Returns((ulong _, string key, object[] args) => LocalizationServiceMock.Object.Get(key, args));

        Service = new PlaybackControlService(
            MusicQueueServiceMock.Object,
            ResponseBuilderMock.Object,
            LocalizationServiceMock.Object,
            TrackNotificationServiceMock.Object,
            PlayerConnectionServiceMock.Object,
            PlaybackEventHandlerServiceMock.Object,
            ProgressiveTimerServiceMock.Object,
            LoggerMock.Object);
    }

    protected void SetupInvalidPlayer()
    {
        PlayerConnectionServiceMock
            .Setup(p => p.TryGetAndValidateExistingPlayerAsync(
                MessageMock.Object,
                VoiceChannelMock.Object,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((null, null, 0UL, false));
    }

    protected void SetupValidPlayer(IDiscordChannel? channel = null)
    {
        PlayerConnectionServiceMock
            .Setup(p => p.TryGetAndValidateExistingPlayerAsync(
                MessageMock.Object,
                VoiceChannelMock.Object,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((PlayerMock.Object, channel ?? TextChannelMock.Object, GuildId, true));
    }

    protected void SetupCurrentTrack()
    {
        PlayerMock.SetupGet(p => p.CurrentTrack).Returns(new LavalinkTrack
        {
            Author = "Test author",
            Title = "Track",
            Identifier = "id"
        });
    }

    protected void SetupNoCurrentTrack()
    {
        PlayerMock.SetupGet(p => p.CurrentTrack).Returns((LavalinkTrack?)null);
    }
}
