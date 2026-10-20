using DC_bot.Configuration;
using DC_bot.Interface.Service.Music;
using DC_bot.Interface.Service.Music.Events;
using DC_bot.Interface.Service.Music.Lavalink;
using DC_bot.Interface.Service.Music.Lifecycle;
using DC_bot.Interface.Service.Music.Playback;
using DC_bot.Interface.Service.Music.PlaylistServiceInterface;
using DC_bot.Interface.Service.Music.Presentation;
using DC_bot.Interface.Service.Music.Queue;
using DC_bot.Interface.Service.Music.Repeat;
using DC_bot.Interface.Service.Music.Search;
using DC_bot.Interface.Service.Music.Tracks;
using DC_bot.Service.Music;
using DC_bot.Service.Music.Events;
using DC_bot.Service.Music.Lavalink;
using DC_bot.Service.Music.Lifecycle;
using DC_bot.Service.Music.Playback;
using DC_bot.Service.Music.PlaylistService;
using DC_bot.Service.Music.Presentation;
using DC_bot.Service.Music.Queue;
using DC_bot.Service.Music.Repeat;
using DC_bot.Service.Music.Tracks;
using Microsoft.Extensions.DependencyInjection;

namespace DC_bot.Startup.DependencyInjection;

public static class MusicServiceCollectionExtensions
{
    public static IServiceCollection AddMusicServices(this IServiceCollection services)
    {
        services.AddOptions<PlaylistOptions>();
        services.AddOptions<PlaybackLifecycleOptions>()
            .Configure(options =>
            {
                options.JoinDeadline = TimeSpan.Parse(
                    Environment.GetEnvironmentVariable("MUSIC_JOIN_DEADLINE")
                    ?? throw new InvalidOperationException("MUSIC_JOIN_DEADLINE is required."),
                    System.Globalization.CultureInfo.InvariantCulture);
                options.LeaveDeadline = TimeSpan.Parse(
                    Environment.GetEnvironmentVariable("MUSIC_LEAVE_DEADLINE")
                    ?? throw new InvalidOperationException("MUSIC_LEAVE_DEADLINE is required."),
                    System.Globalization.CultureInfo.InvariantCulture);
            })
            .Validate(options => options.JoinDeadline > TimeSpan.Zero
                                 && options.JoinDeadline.TotalMilliseconds <= uint.MaxValue - 1,
                "Join deadline must be a positive supported timer duration.")
            .Validate(options => options.LeaveDeadline > TimeSpan.Zero
                                 && options.LeaveDeadline.TotalMilliseconds <= uint.MaxValue - 1,
                "Leave deadline must be a positive supported timer duration.")
            .ValidateOnStart();

        services.AddHttpClient("PlaybackRecovery", client =>
        {
            client.Timeout = Timeout.InfiniteTimeSpan;
        });
        
        return services
            .AddSingleton<ITrackSerializer, LavalinkTrackSerializer>()
            .AddSingleton<PlaybackRemoteProbe>()
            .AddSingleton<PlaybackRecoveryService>()
            .AddSingleton<PlaybackCleanupService>()
            .AddSingleton<GuildPlaybackSessions>()
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
            .AddSingleton<DiscordPlaybackRecoveryService>()
            .AddSingleton<IPlaylistService, PlaylistService>()
            .AddSingleton<ITrackSearchResolverService, TrackSearchResolverService>();
    }
}