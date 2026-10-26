using DC_bot.Interface.Service.Persistence.Models.Playlists;

namespace DC_bot.Interface.Service.Persistence.Playlists;

/// <summary>
/// Defines shared storage operations for playlist track repository, including caller cancellation and immutable result contracts.
/// </summary>
public interface IPlaylistTrackRepository
{

    /// <summary>
    /// Reads one playlist&apos;s tracks in stored order without exposing tracked EF entities.
    /// </summary>
    /// <param name = "playlistId">The durable playlist identifier.</param>
    /// <param name = "cancellationToken">Cancels supported database work and retry backoff; owned rollback uses its own deadline.</param>
    /// <returns>The matching records in the documented order, or an empty collection when none match.</returns>
    Task<IReadOnlyList<PlaylistTrackRecord>> GetByPlaylistIdOrderedAsync(
        long playlistId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Reads tracks for the supplied playlists while preserving each playlist&apos;s stored order.
    /// </summary>
    /// <param name = "playlistIds">The playlist ids value used by this storage contract.</param>
    /// <param name = "cancellationToken">Cancels supported database work and retry backoff; owned rollback uses its own deadline.</param>
    /// <returns>The matching records in the documented order, or an empty collection when none match.</returns>
    Task<IReadOnlyList<PlaylistTrackRecord>> GetByPlaylistIdsOrderedAsync(
        IReadOnlyCollection<long> playlistIds,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Appends an ordered track collection under serializable retry so concurrent appends receive distinct positions.
    /// </summary>
    /// <param name = "playlistId">The durable playlist identifier.</param>
    /// <param name = "tracks">The tracks value used by this storage contract.</param>
    /// <param name = "cancellationToken">Cancels supported database work and retry backoff; owned rollback uses its own deadline.</param>
    /// <returns>A task representing completion of the documented operation.</returns>
    Task AddRangeAsync(
        long playlistId,
        IReadOnlyCollection<PlaylistTrackCreateRecord> tracks,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Appends one track with its source identity under serializable retry.
    /// </summary>
    /// <param name = "playlistId">The durable playlist identifier.</param>
    /// <param name = "track">The track value used by this storage contract.</param>
    /// <param name = "cancellationToken">Cancels supported database work and retry backoff; owned rollback uses its own deadline.</param>
    /// <returns>A task representing completion of the documented operation.</returns>
    Task AddTrackAsync(long playlistId, PlaylistTrackCreateRecord track, CancellationToken cancellationToken = default);

    /// <summary>
    /// Removes the numbered playlist track and compacts remaining positions under serializable retry.
    /// </summary>
    /// <param name = "playlistId">The durable playlist identifier.</param>
    /// <param name = "orderNumber">The order number value used by this storage contract.</param>
    /// <param name = "cancellationToken">Cancels supported database work and retry backoff; owned rollback uses its own deadline.</param>
    /// <returns>A task representing completion of the documented operation.</returns>
    Task RemoveTrackAsync(long playlistId, int orderNumber, CancellationToken cancellationToken = default);
}
