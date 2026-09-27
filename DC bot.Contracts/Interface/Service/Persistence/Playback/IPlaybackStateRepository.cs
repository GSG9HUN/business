using DC_bot.Interface.Service.Persistence.Models.Playback;

namespace DC_bot.Interface.Service.Persistence.Playback;

public interface IPlaybackStateRepository
{
    Task<PlaybackStateRecord> GetOrCreateAsync(ulong guildId, CancellationToken cancellationToken = default);

    Task SetRepeatStateAsync(
        ulong guildId,
        bool isRepeating,
        bool isRepeatingList,
        CancellationToken cancellationToken = default) =>
        SetRepeatStateAsync(guildId, isRepeating, isRepeatingList, realtimeEventName: null, cancellationToken);

    Task SetRepeatStateAsync(
        ulong guildId,
        bool isRepeating,
        bool isRepeatingList,
        string? realtimeEventName = null,
        CancellationToken cancellationToken = default);

    Task SetCurrentTrackAsync(
        ulong guildId,
        string? trackIdentifier,
        long? queueItemId,
        CancellationToken cancellationToken = default) =>
        SetCurrentTrackAsync(guildId, trackIdentifier, queueItemId, realtimeEventName: null, cancellationToken);

    Task SetCurrentTrackAsync(
        ulong guildId, 
        string? trackIdentifier, 
        long? queueItemId, 
        string? realtimeEventName = null,
        CancellationToken cancellationToken = default);

    Task SetPlaybackPositionAsync(
        ulong guildId,
        TimeSpan position,
        bool isPaused,
        CancellationToken cancellationToken = default) =>
        SetPlaybackPositionAsync(guildId, position, isPaused, realtimeEventName: null, cancellationToken);

    Task SetPlaybackPositionAsync(
        ulong guildId,
        TimeSpan position,
        bool isPaused,
        string? realtimeEventName = null,
        CancellationToken cancellationToken = default);
}
