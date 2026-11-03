using DC_bot.Interface.Service.Music.Tracks;

namespace DC_bot.Interface.Service.Music.Playback;

/// <summary>
/// Defines the current track service boundary so callers are independent of its concrete implementation.
/// </summary>
public interface ICurrentTrackService
{

    /// <summary>
    /// Restores the persisted current track, returning null when no current track can be resolved.
    /// </summary>
    /// <param name="guildId">The Discord guild whose state or operation is addressed.</param>
    /// <param name="cancellationToken">Cancels waiting and supported downstream I/O; owned cleanup completes independently.</param>
    /// <returns>The asynchronous operation result, including its status or value when completed.</returns>
    Task<ILavaLinkTrack?> GetCurrentTrackAsync(ulong guildId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Persists current-track identity and publishes the corresponding playback update.
    /// </summary>
    /// <param name="guildId">The Discord guild whose state or operation is addressed.</param>
    /// <param name="track">The lava link track? dependency used by this boundary.</param>
    /// <param name="realtimeEventName">The realtime event published with persistence, or the operation default when omitted.</param>
    /// <param name="cancellationToken">Cancels waiting and supported downstream I/O; owned cleanup completes independently.</param>
    /// <returns>A task representing completion of this operation.</returns>
    Task SetCurrentTrackAsync(
        ulong guildId,
        ILavaLinkTrack? track,
        string? realtimeEventName = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Formats the persisted current track for a command response.
    /// </summary>
    /// <param name="guildId">The Discord guild whose state or operation is addressed.</param>
    /// <param name="cancellationToken">Cancels waiting and supported downstream I/O; owned cleanup completes independently.</param>
    /// <returns>The asynchronous operation result, including its status or value when completed.</returns>
    Task<string> GetCurrentTrackFormattedAsync(ulong guildId, CancellationToken cancellationToken = default);
}
