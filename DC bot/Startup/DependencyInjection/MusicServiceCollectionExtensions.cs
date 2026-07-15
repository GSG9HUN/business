using DC_bot.Interface.Service.Music;
using DC_bot.Interface.Service.Music.MusicServiceInterface;
using DC_bot.Interface.Service.Music.PlaylistServiceInterface;
using DC_bot.Interface.Service.Music.ProgressiveTimerInterface;
using DC_bot.Service.Music;
using DC_bot.Service.Music.MusicServices;
using DC_bot.Service.Music.PlaylistService;
using DC_bot.Service.Music.ProgressiveTimer;
using Microsoft.Extensions.DependencyInjection;

namespace DC_bot.Startup.DependencyInjection;

public static class MusicServiceCollectionExtensions
{
    public static IServiceCollection AddMusicServices(this IServiceCollection services)
    {
        return services
            .AddSingleton<ITrackSerializer, LavalinkTrackSerializer>()
            .AddSingleton<IRepeatService, RepeatService>()
            .AddSingleton<ICurrentTrackService, CurrentTrackService>()
            .AddSingleton<ITrackNotificationService, TrackNotificationService>()
            .AddSingleton<ITrackFormatterService, TrackFormatterService>()
            .AddSingleton<IPlayerConnectionService, PlayerConnectionService>()
            .AddSingleton<ILavalinkNodeConnectionService, LavalinkNodeConnectionService>()
            .AddSingleton<IPlaybackEventHandlerService, PlaybackEventHandlerService>()
            .AddSingleton<IPlaybackControlService, PlaybackControlService>()
            .AddSingleton<IPlaybackRequestService, PlaybackRequestService>()
            .AddSingleton<ITrackPlaybackService, TrackPlaybackService>()
            .AddSingleton<ITrackEndedHandlerService, TrackEndedHandlerService>()
            .AddSingleton<ILavaLinkService, LavaLinkService>()
            .AddSingleton<IMusicQueueService, MusicQueueService>()
            .AddSingleton<IProgressiveTimerService, ProgressiveTimerService>()
            .AddSingleton<IPlaylistService, PlaylistService>()
            .AddSingleton<ITrackSearchResolverService, TrackSearchResolverService>();
    }
}
