using DC_bot.Interface.Service.Music.Tracks;

namespace DC_bot.Interface.Service.Music.Repeat;

/// <summary>
/// Defines the repeat service boundary so callers are independent of its concrete implementation.
/// </summary>
public interface IRepeatService
{

    /// <summary>
    /// Ensures persisted playback and repeat state exists for the guild.
    /// </summary>
    /// <param name="guildId">The Discord guild whose state or operation is addressed.</param>
    /// <param name="cancellationToken">Cancels waiting and supported downstream I/O; owned cleanup completes independently.</param>
    /// <returns>A task representing completion of this operation.</returns>
    Task InitAsync(ulong guildId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Reads the guild&apos;s persisted single-track repeat mode.
    /// </summary>
    /// <param name="guildId">The Discord guild whose state or operation is addressed.</param>
    /// <param name="cancellationToken">Cancels waiting and supported downstream I/O; owned cleanup completes independently.</param>
    /// <returns>A task yielding whether the requested check or operation succeeded.</returns>
    Task<bool> IsRepeatingAsync(ulong guildId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Persists single-track repeat while retaining list-repeat state and publishing the mode change.
    /// </summary>
    /// <param name="guildId">The Discord guild whose state or operation is addressed.</param>
    /// <param name="value">The value supplied for this operation.</param>
    /// <param name="cancellationToken">Cancels waiting and supported downstream I/O; owned cleanup completes independently.</param>
    /// <returns>A task representing completion of this operation.</returns>
    Task SetRepeatingAsync(ulong guildId, bool value, CancellationToken cancellationToken = default);

    /// <summary>
    /// Reads the guild&apos;s persisted list-repeat mode.
    /// </summary>
    /// <param name="guildId">The Discord guild whose state or operation is addressed.</param>
    /// <param name="cancellationToken">Cancels waiting and supported downstream I/O; owned cleanup completes independently.</param>
    /// <returns>A task yielding whether the requested check or operation succeeded.</returns>
    Task<bool> IsRepeatingListAsync(ulong guildId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Persists list-repeat mode and clears the legacy repeat snapshot when disabled.
    /// </summary>
    /// <param name="guildId">The Discord guild whose state or operation is addressed.</param>
    /// <param name="value">The value supplied for this operation.</param>
    /// <param name="cancellationToken">Cancels waiting and supported downstream I/O; owned cleanup completes independently.</param>
    /// <returns>A task representing completion of this operation.</returns>
    Task SetRepeatingListAsync(ulong guildId, bool value, CancellationToken cancellationToken = default);

    /// <summary>
    /// Serializes the supplied current and waiting tracks into the legacy repeat-list snapshot.
    /// </summary>
    /// <param name="guildId">The Discord guild whose state or operation is addressed.</param>
    /// <param name="currentTrack">The lava link track? dependency used by this boundary.</param>
    /// <param name="queuedTracks">The queued tracks supplied for this operation.</param>
    /// <param name="cancellationToken">Cancels waiting and supported downstream I/O; owned cleanup completes independently.</param>
    /// <returns>A task representing completion of this operation.</returns>
    Task SaveRepeatListSnapshotAsync(
        ulong guildId,
        ILavaLinkTrack? currentTrack,
        IReadOnlyCollection<ILavaLinkTrack> queuedTracks,
        CancellationToken cancellationToken = default);
}
