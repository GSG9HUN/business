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
using DC_bot.Service.Music.Lifecycle.Cleanup;
using DC_bot.Service.Music.Lifecycle.Connections;
using DC_bot.Service.Music.Lifecycle.PersistentVoice;
using DC_bot.Service.Music.Lifecycle.Recovery;
using DC_bot.Service.Music.Lifecycle.Sessions;
using DC_bot.Service.Music.Playback;
using DC_bot.Service.Music.PlaylistService;
using DC_bot.Service.Music.Presentation;
using DC_bot.Service.Music.Queue;
using DC_bot.Service.Music.Repeat;
using DC_bot.Service.Music.Tracks;
using Microsoft.Extensions.DependencyInjection;

namespace DC_bot.Startup.DependencyInjection;

/// <summary>
/// Registers the music runtime dependencies at the composition root.
/// </summary>
public static class MusicServiceCollectionExtensions
{

    /// <summary>
    /// Registers the music services dependencies with the shared service collection.
    /// </summary>
    /// <param name="services">The services supplied for this operation.</param>
    /// <returns>The service collection with these registrations, allowing further composition.</returns>
    public static IServiceCollection AddMusicServices(this IServiceCollection services)
    {
        services.AddOptions<PlaylistOptions>();
        services.AddOptions<PlaybackLifecycleOptions>()
            .Configure(options =>
        {
            options.JoinDeadline = TimeSpan.Parse(Environment.GetEnvironmentVariable("MUSIC_JOIN_DEADLINE") ?? throw new InvalidOperationException("MUSIC_JOIN_DEADLINE is required."), System.Globalization.CultureInfo.InvariantCulture);
            options.LeaveDeadline = TimeSpan.Parse(Environment.GetEnvironmentVariable("MUSIC_LEAVE_DEADLINE") ?? throw new InvalidOperationException("MUSIC_LEAVE_DEADLINE is required."), System.Globalization.CultureInfo.InvariantCulture);
        })
            .Validate(options => options.JoinDeadline > TimeSpan.Zero && options.JoinDeadline.TotalMilliseconds <= uint.MaxValue - 1, "Join deadline must be a positive supported timer duration.")
            .Validate(options => options.LeaveDeadline > TimeSpan.Zero && options.LeaveDeadline.TotalMilliseconds <= uint.MaxValue - 1, "Leave deadline must be a positive supported timer duration.")
            .ValidateOnStart();
        services.AddHttpClient("PlaybackRecovery", client =>
        {
            client.Timeout = Timeout.InfiniteTimeSpan;
        });
        return services.AddSingleton<ITrackSerializer, LavalinkTrackSerializer>()
            .AddSingleton<PlaybackRemoteProbe>()
            .AddSingleton<PlaybackRecoveryService>()
            .AddSingleton<PlaybackCleanupService>()
            .AddSingleton<GuildPlaybackSessions>()
            .AddSingleton<IRepeatService, RepeatService>()
            .AddSingleton<ICurrentTrackService, CurrentTrackService>()
            .AddSingleton<ITrackNotificationService, TrackNotificationService>()
            .AddSingleton<ITrackFormatterService, TrackFormatterService>()
            .AddSingleton<PlayerConnectionRetryPolicy>()
            .AddSingleton<IPlayerConnectionService, PlayerConnectionService>()
            .AddSingleton<ILavalinkNodeConnectionService, LavalinkNodeConnectionService>()
            .AddSingleton<IPlaybackEventHandlerService, PlaybackEventHandlerService>()
            .AddSingleton<IPlaybackControlService, PlaybackControlService>()
            .AddSingleton<IPlaybackRequestService, PlaybackRequestService>()
            .AddSingleton<ITrackPlaybackService, TrackPlaybackService>()
            .AddSingleton<ITrackEndedHandlerService, TrackEndedHandlerService>()
            .AddSingleton<ILavaLinkService, LavaLinkService>()
            .AddSingleton<IMusicQueueService, MusicQueueService>()
            .AddSingleton<DisconnectedPlayerCleanupService>()
            .AddSingleton<TwentyFourSevenService>()
            .AddSingleton<PersistentVoiceGate>()
            .AddSingleton<PersistentVoiceLeaveService>()
            .AddSingleton<PersistentVoiceRecoveryService>()
            .AddSingleton<DiscordPlaybackRecoveryService>()
            .AddSingleton<PlaylistTrackLoader>()
            .AddSingleton<PlaylistTrackDisplayMapper>()
            .AddSingleton(provider => new PlaylistQueryService(provider.GetRequiredService<DC_bot.Interface.Service.Persistence.Playlists.IPlaylistRepository>(), provider.GetRequiredService<DC_bot.Interface.Service.Persistence.Playlists.IPlaylistTrackRepository>(), provider.GetRequiredService<PlaylistTrackDisplayMapper>(), provider.GetRequiredService<Microsoft.Extensions.Logging.ILogger<PlaylistService>>()))
            .AddSingleton(provider => new PlaylistMutationService(provider.GetRequiredService<DC_bot.Interface.Service.Persistence.Playlists.IPlaylistRepository>(), provider.GetRequiredService<Microsoft.Extensions.Options.IOptions<PlaylistOptions>>().Value, provider.GetRequiredService<Microsoft.Extensions.Logging.ILogger<PlaylistService>>()))
            .AddSingleton(provider => new PlaylistTrackMutationService(provider.GetRequiredService<DC_bot.Interface.Service.Persistence.Playlists.IPlaylistRepository>(), provider.GetRequiredService<DC_bot.Interface.Service.Persistence.Playlists.IPlaylistTrackRepository>(), provider.GetRequiredService<ITrackSearchResolverService>(), provider.GetRequiredService<ITrackSerializer>(), provider.GetRequiredService<PlaylistTrackLoader>(), provider.GetRequiredService<Microsoft.Extensions.Options.IOptions<PlaylistOptions>>().Value, provider.GetRequiredService<Microsoft.Extensions.Logging.ILogger<PlaylistService>>()))
            .AddSingleton<IPlaylistService>(provider => new PlaylistService(provider.GetRequiredService<PlaylistQueryService>(), provider.GetRequiredService<PlaylistMutationService>(), provider.GetRequiredService<PlaylistTrackMutationService>()))
            .AddSingleton<ITrackSearchResolverService, TrackSearchResolverService>();
    }
}
