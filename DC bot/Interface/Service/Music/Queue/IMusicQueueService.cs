using DC_bot.Interface.Service.Music.Queue.Models;
using DC_bot.Interface.Service.Music.Tracks;

namespace DC_bot.Interface.Service.Music.Queue;

/// <summary>
/// Defines the music queue service boundary so callers are independent of its concrete implementation.
/// </summary>
public interface IMusicQueueService
{

    /// <summary>
    /// Atomically enables repetition of the live canonical batch when it contains active playback work.
    /// </summary>
    /// <param name="guildId">The Discord guild whose state or operation is addressed.</param>
    /// <param name="cancellationToken">Cancels waiting and supported downstream I/O; owned cleanup completes independently.</param>
    /// <returns>A task yielding whether the requested check or operation succeeded.</returns>
    Task<bool> TryEnableBatchRepeatAsync(ulong guildId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Reports whether the guild has durable waiting queue entries.
    /// </summary>
    /// <param name="guildId">The Discord guild whose state or operation is addressed.</param>
    /// <returns>A task yielding whether the requested check or operation succeeded.</returns>
    /// <param name="cancellationToken">Cancels waiting and supported downstream I/O; owned cleanup completes independently.</param>
    public Task<bool> HasTracks(ulong guildId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Serializes incoming tracks and appends their source metadata to the active batch and waiting queue.
    /// </summary>
    /// <param name="guildId">The Discord guild whose state or operation is addressed.</param>
    /// <param name="tracks">The tracks supplied for this operation.</param>
    /// <param name="cancellationToken">Cancels waiting and supported downstream I/O; owned cleanup completes independently.</param>
    /// <returns>A task representing completion of this operation.</returns>
    public Task EnqueueMany(
        ulong guildId,
        IReadOnlyCollection<QueueTrackToEnqueue> tracks,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns a read-only view of parseable waiting tracks in durable queue order.
    /// </summary>
    /// <param name="guildId">The Discord guild whose state or operation is addressed.</param>
    /// <returns>The asynchronous operation result, including its status or value when completed.</returns>
    /// <param name="cancellationToken">Cancels waiting and supported downstream I/O; owned cleanup completes independently.</param>
    public Task<IReadOnlyCollection<ILavaLinkTrack>> ViewQueue(ulong guildId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Restores waiting tracks and marks unparseable persisted queue entries skipped.
    /// </summary>
    /// <param name="guildId">The Discord guild whose state or operation is addressed.</param>
    /// <param name="cancellationToken">Cancels waiting and supported downstream I/O; owned cleanup completes independently.</param>
    /// <returns>The asynchronous operation result, including its status or value when completed.</returns>
    public Task<Queue<ILavaLinkTrack>> GetQueue(ulong guildId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Restores all canonical items of the active batch, including played tracks, logging unparseable entries.
    /// </summary>
    /// <param name="guildId">The Discord guild whose state or operation is addressed.</param>
    /// <returns>The asynchronous operation result, including its status or value when completed.</returns>
    /// <param name="cancellationToken">Cancels waiting and supported downstream I/O; owned cleanup completes independently.</param>
    Task<IReadOnlyCollection<ILavaLinkTrack>> GetRepeatableQueue(ulong guildId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Persists the requested order of waiting tracks without rebuilding canonical batch membership.
    /// </summary>
    /// <param name="guildId">The Discord guild whose state or operation is addressed.</param>
    /// <param name="shuffledQueue">The shuffled queue supplied for this operation.</param>
    /// <param name="cancellationToken">Cancels waiting and supported downstream I/O; owned cleanup completes independently.</param>
    /// <returns>A task representing completion of this operation.</returns>
    public Task SetQueue(ulong guildId, Queue<ILavaLinkTrack> shuffledQueue, CancellationToken cancellationToken = default);

    /// <summary>
    /// Marks waiting tracks skipped while preserving the playing track and canonical batch history.
    /// </summary>
    /// <param name="guildId">The Discord guild whose state or operation is addressed.</param>
    /// <param name="cancellationToken">Cancels waiting and supported downstream I/O; owned cleanup completes independently.</param>
    /// <returns>A task representing completion of this operation.</returns>
    Task ClearQueue(ulong guildId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Shuffles waiting tracks and persists their order while retaining queue-item identities.
    /// </summary>
    /// <param name="guildId">The Discord guild whose state or operation is addressed.</param>
    /// <param name="cancellationToken">Cancels waiting and supported downstream I/O; owned cleanup completes independently.</param>
    /// <returns>The asynchronous operation result, including its status or value when completed.</returns>
    Task<QueueShuffleResult> ShuffleQueue(ulong guildId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Removes a numbered waiting track and reports its title and the remaining queue size.
    /// </summary>
    /// <param name="guildId">The Discord guild whose state or operation is addressed.</param>
    /// <param name="trackNumber">The track number supplied for this operation.</param>
    /// <param name="cancellationToken">Cancels waiting and supported downstream I/O; owned cleanup completes independently.</param>
    /// <returns>The asynchronous operation result, including its status or value when completed.</returns>
    Task<QueueRemoveResult> RemoveAt(ulong guildId, int trackNumber, CancellationToken cancellationToken = default);

    /// <summary>
    /// Moves a waiting track to the requested position and reports the resulting queue indices.
    /// </summary>
    /// <param name="guildId">The Discord guild whose state or operation is addressed.</param>
    /// <param name="trackIndex">The track index supplied for this operation.</param>
    /// <param name="targetIndex">The target index supplied for this operation.</param>
    /// <param name="cancellationToken">Cancels waiting and supported downstream I/O; owned cleanup completes independently.</param>
    /// <returns>The asynchronous operation result, including its status or value when completed.</returns>
    Task<QueueMoveResult> MoveToIndex(ulong guildId, int trackIndex, int targetIndex, CancellationToken cancellationToken = default);
}
