using DC_bot.Interface.Service.Persistence.Models.Playback;

namespace DC_bot.Interface.Service.Persistence.Playback;

/// <summary>
/// Defines shared storage operations for playback state repository, including caller cancellation and immutable result contracts.
/// </summary>
public interface IPlaybackStateRepository
{

    /// <summary>
    /// Reads the guild playback state or mobile-user settings, creating documented defaults when missing.
    /// </summary>
    /// <param name = "guildId">The Discord guild whose persisted state is addressed.</param>
    /// <param name = "cancellationToken">Cancels supported database work and retry backoff; owned rollback uses its own deadline.</param>
    /// <returns>A task yielding the persisted or mapped result described above.</returns>
    Task<PlaybackStateRecord> GetOrCreateAsync(ulong guildId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Persists repeat flags while retaining unrelated playback state and publishes the selected realtime event.
    /// </summary>
    /// <param name = "guildId">The Discord guild whose persisted state is addressed.</param>
    /// <param name = "isRepeating">Whether single-track repetition is enabled.</param>
    /// <param name = "isRepeatingList">Whether canonical-list repetition is enabled.</param>
    /// <param name = "cancellationToken">Cancels supported database work and retry backoff; owned rollback uses its own deadline.</param>
    /// <returns>A task representing completion of the documented operation.</returns>
    Task SetRepeatStateAsync(ulong guildId, bool isRepeating, bool isRepeatingList, CancellationToken cancellationToken = default) => SetRepeatStateAsync(guildId, isRepeating, isRepeatingList, realtimeEventName: null, cancellationToken);

    /// <summary>
    /// Persists repeat flags while retaining unrelated playback state and publishes the selected realtime event.
    /// </summary>
    /// <param name = "guildId">The Discord guild whose persisted state is addressed.</param>
    /// <param name = "isRepeating">Whether single-track repetition is enabled.</param>
    /// <param name = "isRepeatingList">Whether canonical-list repetition is enabled.</param>
    /// <param name = "realtimeEventName">The shared realtime event name, or the documented default when omitted.</param>
    /// <param name = "cancellationToken">Cancels supported database work and retry backoff; owned rollback uses its own deadline.</param>
    /// <returns>A task representing completion of the documented operation.</returns>
    Task SetRepeatStateAsync(
        ulong guildId,
        bool isRepeating,
        bool isRepeatingList,
        string? realtimeEventName = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Persists or clears current-track identity and its queue-item relationship, publishing the selected track event.
    /// </summary>
    /// <param name = "guildId">The Discord guild whose persisted state is addressed.</param>
    /// <param name = "trackIdentifier">The encoded source-track identity stored independently of any runtime player.</param>
    /// <param name = "queueItemId">The durable queue-item identifier, when a specific item is required.</param>
    /// <param name = "cancellationToken">Cancels supported database work and retry backoff; owned rollback uses its own deadline.</param>
    /// <returns>A task representing completion of the documented operation.</returns>
    Task SetCurrentTrackAsync(
        ulong guildId,
        string? trackIdentifier,
        long? queueItemId,
        CancellationToken cancellationToken = default) => SetCurrentTrackAsync(guildId, trackIdentifier, queueItemId, realtimeEventName: null, cancellationToken);

    /// <summary>
    /// Persists or clears current-track identity and its queue-item relationship, publishing the selected track event.
    /// </summary>
    /// <param name = "guildId">The Discord guild whose persisted state is addressed.</param>
    /// <param name = "trackIdentifier">The encoded source-track identity stored independently of any runtime player.</param>
    /// <param name = "queueItemId">The durable queue-item identifier, when a specific item is required.</param>
    /// <param name = "realtimeEventName">The shared realtime event name, or the documented default when omitted.</param>
    /// <param name = "cancellationToken">Cancels supported database work and retry backoff; owned rollback uses its own deadline.</param>
    /// <returns>A task representing completion of the documented operation.</returns>
    Task SetCurrentTrackAsync(
        ulong guildId,
        string? trackIdentifier,
        long? queueItemId,
        string? realtimeEventName = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Persists playback position and pause state and publishes the selected playback event.
    /// </summary>
    /// <param name = "guildId">The Discord guild whose persisted state is addressed.</param>
    /// <param name = "position">The position value used by this storage contract.</param>
    /// <param name = "isPaused">The is paused value used by this storage contract.</param>
    /// <param name = "cancellationToken">Cancels supported database work and retry backoff; owned rollback uses its own deadline.</param>
    /// <returns>A task representing completion of the documented operation.</returns>
    Task SetPlaybackPositionAsync(ulong guildId, TimeSpan position, bool isPaused, CancellationToken cancellationToken = default) => SetPlaybackPositionAsync(guildId, position, isPaused, realtimeEventName: null, cancellationToken);

    /// <summary>
    /// Persists playback position and pause state and publishes the selected playback event.
    /// </summary>
    /// <param name = "guildId">The Discord guild whose persisted state is addressed.</param>
    /// <param name = "position">The position value used by this storage contract.</param>
    /// <param name = "isPaused">The is paused value used by this storage contract.</param>
    /// <param name = "realtimeEventName">The shared realtime event name, or the documented default when omitted.</param>
    /// <param name = "cancellationToken">Cancels supported database work and retry backoff; owned rollback uses its own deadline.</param>
    /// <returns>A task representing completion of the documented operation.</returns>
    Task SetPlaybackPositionAsync(
        ulong guildId,
        TimeSpan position,
        bool isPaused,
        string? realtimeEventName = null,
        CancellationToken cancellationToken = default);
}
