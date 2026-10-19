using DC_bot.Interface.Discord;
using DC_bot.Interface.Service.Music.Playback.Models;
using DC_bot.Interface.Service.Music.Tracks;
using DC_bot.Service.Music.Lifecycle;
using Lavalink4NET.Rest.Entities.Tracks;

namespace DC_bot.Interface.Service.Music.Playback;

public interface ITrackPlaybackService
{
    Task<PlaybackStartResult> PlayTheFoundMusicAsync(
        TrackLoadResult searchQuery, 
        PlaybackSession session,
        IDiscordChannel textChannel, 
        string? requestedBy,
        string? sourceQuery, 
        TrackSearchMode? sourceSearchMode);
    
    Task<PlaybackStartResult> TryPlayNextTrackAsync(
        PlaybackSession session, 
        IDiscordChannel textChannel,
        PlaybackStartReason reason = PlaybackStartReason.QueueNext,
        ControlReservation? reservation = null);
    
    Task<PlaybackStartResult> StartExistingAsync(
        PlaybackSession session,
        IDiscordChannel textChannel,
        long queueItemId,
        PlaybackStartReason reason,
        ILavaLinkTrack? replacement = null,
        ControlReservation? reservation = null);
}