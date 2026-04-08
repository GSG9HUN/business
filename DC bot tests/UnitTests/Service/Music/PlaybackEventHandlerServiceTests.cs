using DC_bot.Interface.Discord;
using DC_bot.Interface.Service.Music.MusicServiceInterface;
using DC_bot.Service.Music.MusicServices;
using Lavalink4NET;
using Lavalink4NET.Events;
using Lavalink4NET.Events.Players;
using Lavalink4NET.Players;
using Lavalink4NET.Protocol.Payloads.Events;
using Lavalink4NET.Tracks;
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

    [Fact]
    public async Task RegisterPlaybackFinishedHandler_InvokedHandler_DelegatesToTrackEndedHandlerService()
    {
        var audio = new Mock<IAudioService>();
        var logger = new Mock<ILogger<PlaybackEventHandlerService>>();
        var ended = new Mock<ITrackEndedHandlerService>();
        var player = new Mock<ILavalinkPlayer>();
        var channel = new Mock<IDiscordChannel>();

        AsyncEventHandler<TrackEndedEventArgs>? captured = null;
        audio.SetupAdd(a => a.TrackEnded += It.IsAny<AsyncEventHandler<TrackEndedEventArgs>>())
            .Callback<AsyncEventHandler<TrackEndedEventArgs>>(h => captured = h);

        var service = new PlaybackEventHandlerService(audio.Object, logger.Object, ended.Object);
        service.RegisterPlaybackFinishedHandler(42, player.Object, channel.Object);

        Assert.NotNull(captured);

        var args = new TrackEndedEventArgs(player.Object, new LavalinkTrack { Author = "A", Title = "T", Identifier = "id" },
            TrackEndReason.Finished);

        await captured!(player.Object, args);

        ended.Verify(e => e.HandleTrackEndedAsync(player.Object, args, channel.Object), Times.Once);
    }

    [Fact]
    public async Task CleanupGuildAsync_WhenUnsubscribeThrows_ReturnsFaultedTask()
    {
        var audio = new Mock<IAudioService>();
        var logger = new Mock<ILogger<PlaybackEventHandlerService>>();
        var ended = new Mock<ITrackEndedHandlerService>();
        var player = new Mock<ILavalinkPlayer>();
        var channel = new Mock<IDiscordChannel>();

        audio.SetupRemove(a => a.TrackEnded -= It.IsAny<AsyncEventHandler<TrackEndedEventArgs>>())
            .Throws(new InvalidOperationException("unsubscribe failed"));

        var service = new PlaybackEventHandlerService(audio.Object, logger.Object, ended.Object);
        service.RegisterPlaybackFinishedHandler(7, player.Object, channel.Object);

        await Assert.ThrowsAsync<InvalidOperationException>(() => service.CleanupGuildAsync(7));
    }
}