using DC_bot.Interface.Discord;
using DC_bot.Interface.Service.Music.MusicServiceInterface;
using DC_bot.Service.Music.MusicServices;
using Lavalink4NET;
using Lavalink4NET.Events;
using Lavalink4NET.Events.Players;
using Lavalink4NET.Players;
using Microsoft.Extensions.Logging;
using Moq;

namespace DC_bot_tests.UnitTests.Service.Music;

public class PlaybackEventHandlerServiceTests
{
    [Fact]
    public void RegisterPlaybackFinishedHandler_FirstTime_SubscribesOnce()
    {
        var audio = new Mock<IAudioService>();
        var logger = new Mock<ILogger<PlaybackEventHandlerService>>();
        var ended = new Mock<ITrackEndedHandlerService>();
        var player = new Mock<ILavalinkPlayer>();
        var channel = new Mock<IDiscordChannel>();

        var service = new PlaybackEventHandlerService(audio.Object, logger.Object, ended.Object);

        service.RegisterPlaybackFinishedHandler(1, player.Object, channel.Object);

        audio.VerifyAdd(a => a.TrackEnded += It.IsAny<AsyncEventHandler<TrackEndedEventArgs>>(), Times.Once);
    }

    [Fact]
    public void RegisterPlaybackFinishedHandler_SecondTimeForSameGuild_DoesNotResubscribe()
    {
        var audio = new Mock<IAudioService>();
        var logger = new Mock<ILogger<PlaybackEventHandlerService>>();
        var ended = new Mock<ITrackEndedHandlerService>();
        var player = new Mock<ILavalinkPlayer>();
        var channel = new Mock<IDiscordChannel>();

        var service = new PlaybackEventHandlerService(audio.Object, logger.Object, ended.Object);

        service.RegisterPlaybackFinishedHandler(1, player.Object, channel.Object);
        service.RegisterPlaybackFinishedHandler(1, player.Object, channel.Object);

        audio.VerifyAdd(a => a.TrackEnded += It.IsAny<AsyncEventHandler<TrackEndedEventArgs>>(), Times.Once);
    }

    [Fact]
    public async Task CleanupGuildAsync_ExistingGuild_Unsubscribes()
    {
        var audio = new Mock<IAudioService>();
        var logger = new Mock<ILogger<PlaybackEventHandlerService>>();
        var ended = new Mock<ITrackEndedHandlerService>();
        var player = new Mock<ILavalinkPlayer>();
        var channel = new Mock<IDiscordChannel>();

        var service = new PlaybackEventHandlerService(audio.Object, logger.Object, ended.Object);
        service.RegisterPlaybackFinishedHandler(2, player.Object, channel.Object);

        await service.CleanupGuildAsync(2);

        audio.VerifyRemove(a => a.TrackEnded -= It.IsAny<AsyncEventHandler<TrackEndedEventArgs>>(), Times.Once);
    }

    [Fact]
    public async Task CleanupGuildAsync_UnknownGuild_DoesNothing()
    {
        var audio = new Mock<IAudioService>();
        var logger = new Mock<ILogger<PlaybackEventHandlerService>>();
        var ended = new Mock<ITrackEndedHandlerService>();

        var service = new PlaybackEventHandlerService(audio.Object, logger.Object, ended.Object);

        await service.CleanupGuildAsync(999);

        audio.VerifyRemove(a => a.TrackEnded -= It.IsAny<AsyncEventHandler<TrackEndedEventArgs>>(), Times.Never);
    }
}