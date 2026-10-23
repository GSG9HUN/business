using DC_bot.Interface.Service.Persistence.Models.Queue;

namespace DC_bot.Interface.Service.Persistence.Queue;

/// <summary>
/// Defines shared storage operations for queue repository, including caller cancellation and immutable result contracts.
/// </summary>
public interface IQueueRepository
{

    /// <summary>
    /// Atomically enables repeat for a live batch instead of snapshotting only its remaining tracks.
    /// </summary>
    /// <param name = "guildId">The Discord guild whose persisted state is addressed.</param>
    /// <param name = "cancellationToken">Cancels supported database work and retry backoff; owned rollback uses its own deadline.</param>
    /// <returns>True when the documented check or conditional mutation succeeds; otherwise false.</returns>
    Task<bool> TryEnableBatchRepeatAsync(ulong guildId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Reads the active batch&apos;s canonical items, including already played items required for full-list repetition.
    /// </summary>
    /// <param name = "guildId">The Discord guild whose persisted state is addressed.</param>
    /// <param name = "cancellationToken">Cancels supported database work and retry backoff; owned rollback uses its own deadline.</param>
    /// <returns>The matching records in the documented order, or an empty collection when none match.</returns>
    Task<IReadOnlyList<QueueItemToEnqueue>> GetActiveBatchItemsAsync(ulong guildId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Re-enqueues the active batch&apos;s canonical list only when repetition is enabled and playback is idle.
    /// </summary>
    /// <param name = "guildId">The Discord guild whose persisted state is addressed.</param>
    /// <param name = "cancellationToken">Cancels supported database work and retry backoff; owned rollback uses its own deadline.</param>
    /// <returns>True when the documented check or conditional mutation succeeds; otherwise false.</returns>
    Task<bool> RestartActiveBatchAsync(ulong guildId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Attempts to close the current batch only when no active queue items remain; false requires the caller to reconsider newly added work.
    /// </summary>
    /// <param name = "guildId">The Discord guild whose persisted state is addressed.</param>
    /// <param name = "cancellationToken">Cancels supported database work and retry backoff; owned rollback uses its own deadline.</param>
    /// <returns>True when the documented check or conditional mutation succeeds; otherwise false.</returns>
    Task<bool> CloseBatchIfIdleAsync(ulong guildId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Closes the current batch for an explicit leave and settles its active queue and repeat state.
    /// </summary>
    /// <param name = "guildId">The Discord guild whose persisted state is addressed.</param>
    /// <param name = "cancellationToken">Cancels supported database work and retry backoff; owned rollback uses its own deadline.</param>
    /// <returns>A task representing completion of the documented operation.</returns>
    Task CloseActiveBatchAsync(ulong guildId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Reads bounded, recent batch history without changing playback state or the previous-track cursor.
    /// </summary>
    /// <param name = "guildId">The Discord guild whose persisted state is addressed.</param>
    /// <param name = "limit">The requested history bound; the implementation clamps it to the supported range.</param>
    /// <param name = "cancellationToken">Cancels supported database work and retry backoff; owned rollback uses its own deadline.</param>
    /// <returns>The matching records in the documented order, or an empty collection when none match.</returns>
    Task<IReadOnlyList<PlaybackBatchRecord>> GetRecentBatchesAsync(
        ulong guildId,
        int limit = 10,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Reads waiting and pending-start queue entries in durable queue position order.
    /// </summary>
    /// <param name = "guildId">The Discord guild whose persisted state is addressed.</param>
    /// <param name = "cancellationToken">Cancels supported database work and retry backoff; owned rollback uses its own deadline.</param>
    /// <returns>The matching records in the documented order, or an empty collection when none match.</returns>
    Task<IReadOnlyList<QueueItemRecord>> GetQueuedItemsAsync(ulong guildId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Reports whether the guild has at least one waiting item eligible for playback.
    /// </summary>
    /// <param name = "guildId">The Discord guild whose persisted state is addressed.</param>
    /// <param name = "cancellationToken">Cancels supported database work and retry backoff; owned rollback uses its own deadline.</param>
    /// <returns>True when the documented check or conditional mutation succeeds; otherwise false.</returns>
    Task<bool> AnyQueuedItemsAsync(ulong guildId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Finds the preceding completed canonical item in the current active batch; earlier batches are excluded.
    /// </summary>
    /// <param name = "guildId">The Discord guild whose persisted state is addressed.</param>
    /// <param name = "cancellationToken">Cancels supported database work and retry backoff; owned rollback uses its own deadline.</param>
    /// <returns>The matching value, or null when no value satisfies the documented lookup.</returns>
    Task<QueueItemRecord?> GetPreviousItemAsync(ulong guildId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Reads a queue item or playlist by its durable database identifier.
    /// </summary>
    /// <param name = "queueItemId">The durable queue-item identifier, when a specific item is required.</param>
    /// <param name = "cancellationToken">Cancels supported database work and retry backoff; owned rollback uses its own deadline.</param>
    /// <returns>The matching value, or null when no value satisfies the documented lookup.</returns>
    Task<QueueItemRecord?> GetByIdAsync(long queueItemId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Appends source-tagged queue items to the current canonical batch in order, creating a new batch only when required.
    /// </summary>
    /// <param name = "guildId">The Discord guild whose persisted state is addressed.</param>
    /// <param name = "queueItems">The ordered queue entries including source query, search mode, and requester metadata.</param>
    /// <param name = "cancellationToken">Cancels supported database work and retry backoff; owned rollback uses its own deadline.</param>
    /// <returns>A task representing completion of the documented operation.</returns>
    Task EnqueueManyAsync(ulong guildId, IReadOnlyList<QueueItemToEnqueue> queueItems, CancellationToken cancellationToken = default);

    /// <summary>
    /// Changes waiting positions without changing canonical batch membership, then emits the selected queue event.
    /// </summary>
    /// <param name = "guildId">The Discord guild whose persisted state is addressed.</param>
    /// <param name = "trackIdentifiers">The track identities in the requested durable order, preserving duplicate occurrences.</param>
    /// <param name = "realtimeEventName">The shared realtime event name, or the documented default when omitted.</param>
    /// <param name = "cancellationToken">Cancels supported database work and retry backoff; owned rollback uses its own deadline.</param>
    /// <returns>A task representing completion of the documented operation.</returns>
    Task ReorderQueuedItemsAsync(
        ulong guildId,
        IReadOnlyList<string> trackIdentifiers,
        string? realtimeEventName = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Removes the numbered waiting item and returns its identity and the resulting queue size.
    /// </summary>
    /// <param name = "guildId">The Discord guild whose persisted state is addressed.</param>
    /// <param name = "trackNumber">The one-based track number used by the public queue or playlist operation.</param>
    /// <param name = "cancellationToken">Cancels supported database work and retry backoff; owned rollback uses its own deadline.</param>
    /// <returns>The removal outcome, resulting queue size, and removed item when applicable.</returns>
    Task<QueueItemRemovalRecord> RemoveQueuedItemAtAsync(
        ulong guildId,
        int trackNumber,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Marks waiting guild items skipped and publishes the cleared-queue event in the same transaction.
    /// </summary>
    /// <param name = "guildId">The Discord guild whose persisted state is addressed.</param>
    /// <param name = "cancellationToken">Cancels supported database work and retry backoff; owned rollback uses its own deadline.</param>
    /// <returns>A task representing completion of the documented operation.</returns>
    Task MarkAllQueuedAsSkippedAsync(ulong guildId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Atomically claims the requested or next eligible queue item; reuses the same pending operation and rejects a competing pending claim.
    /// </summary>
    /// <remarks>Operation identity fences stale or duplicate updates. A durable claim is not proof that remote playback started; the caller must reconcile uncertain transport before releasing or repeating it.</remarks>
    /// <param name = "guildId">The Discord guild whose persisted state is addressed.</param>
    /// <param name = "operationId">The nonempty playback operation identifier used to fence stale or duplicate state transitions.</param>
    /// <param name = "queueItemId">The durable queue-item identifier, when a specific item is required.</param>
    /// <param name = "cancellationToken">Cancels supported database work and retry backoff; owned rollback uses its own deadline.</param>
    /// <returns>The claimed item and operation, a busy result for a competing attempt, or an empty result when no candidate exists.</returns>
    Task<QueueClaimResult> ClaimForStartAsync(
        ulong guildId,
        Guid operationId,
        long? queueItemId = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Idempotently commits a matching start claim as playing, updates current-track state, and emits queue and playback events.
    /// </summary>
    /// <remarks>Operation identity fences stale or duplicate updates. A durable claim is not proof that remote playback started; the caller must reconcile uncertain transport before releasing or repeating it.</remarks>
    /// <param name = "guildId">The Discord guild whose persisted state is addressed.</param>
    /// <param name = "queueItemId">The durable queue-item identifier, when a specific item is required.</param>
    /// <param name = "operationId">The nonempty playback operation identifier used to fence stale or duplicate state transitions.</param>
    /// <param name = "trackIdentifier">The encoded source-track identity stored independently of any runtime player.</param>
    /// <param name = "realtimeEventName">The shared realtime event name, or the documented default when omitted.</param>
    /// <param name = "clearSourceMetadata">Whether confirmed replacement playback clears stale reload-source metadata.</param>
    /// <param name="initialPosition">The source position committed with a recovery start, or null to start at zero.</param>
    /// <param name="initiallyPaused">Whether the confirmed start is paused rather than actively advancing.</param>
    /// <param name = "cancellationToken">Cancels supported database work and retry backoff; owned rollback uses its own deadline.</param>
    /// <returns>True when the documented check or conditional mutation succeeds; otherwise false.</returns>
    Task<bool> CompleteStartAsync(
        ulong guildId,
        long queueItemId,
        Guid operationId,
        string trackIdentifier,
        string realtimeEventName,
        bool clearSourceMetadata = false,
        TimeSpan? initialPosition = null,
        bool initiallyPaused = false,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Conditionally releases a matching pending start by restoring its previous state or marking it skipped.
    /// </summary>
    /// <remarks>Operation identity fences stale or duplicate updates. A durable claim is not proof that remote playback started; the caller must reconcile uncertain transport before releasing or repeating it.</remarks>
    /// <param name = "guildId">The Discord guild whose persisted state is addressed.</param>
    /// <param name = "queueItemId">The durable queue-item identifier, when a specific item is required.</param>
    /// <param name = "operationId">The nonempty playback operation identifier used to fence stale or duplicate state transitions.</param>
    /// <param name = "skip">Whether the released attempt is skipped instead of restoring its earlier queue state.</param>
    /// <param name = "cancellationToken">Cancels supported database work and retry backoff; owned rollback uses its own deadline.</param>
    /// <returns>True when the documented check or conditional mutation succeeds; otherwise false.</returns>
    Task<bool> ReleaseStartAsync(
        ulong guildId,
        long queueItemId,
        Guid operationId,
        bool skip,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Completes only the matching playing attempt as played or skipped so late or duplicate events cannot consume a newer attempt.
    /// </summary>
    /// <remarks>Operation identity fences stale or duplicate updates. A durable claim is not proof that remote playback started; the caller must reconcile uncertain transport before releasing or repeating it.</remarks>
    /// <param name = "guildId">The Discord guild whose persisted state is addressed.</param>
    /// <param name = "queueItemId">The durable queue-item identifier, when a specific item is required.</param>
    /// <param name = "operationId">The nonempty playback operation identifier used to fence stale or duplicate state transitions.</param>
    /// <param name = "played">Whether the completed attempt finished normally rather than being skipped.</param>
    /// <param name = "cancellationToken">Cancels supported database work and retry backoff; owned rollback uses its own deadline.</param>
    /// <returns>True when the documented check or conditional mutation succeeds; otherwise false.</returns>
    Task<bool> CompleteAttemptAsync(
        ulong guildId,
        long queueItemId,
        Guid operationId,
        bool played,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Marks the item skipped only while it is still queued, preserving any concurrently changed playback state.
    /// </summary>
    /// <param name = "queueItemId">The durable queue-item identifier, when a specific item is required.</param>
    /// <param name = "cancellationToken">Cancels supported database work and retry backoff; owned rollback uses its own deadline.</param>
    /// <returns>True when the documented check or conditional mutation succeeds; otherwise false.</returns>
    Task<bool> TryMarkQueuedAsSkippedAsync(long queueItemId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Reads unresolved durable start claims, optionally restricted to one guild, for remote-state reconciliation.
    /// </summary>
    /// <param name = "guildId">The Discord guild whose persisted state is addressed.</param>
    /// <param name = "cancellationToken">Cancels supported database work and retry backoff; owned rollback uses its own deadline.</param>
    /// <returns>The matching records in the documented order, or an empty collection when none match.</returns>
    Task<IReadOnlyList<PendingPlaybackStart>> GetPendingStartsAsync(
        ulong? guildId = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Binds the matching pending start to its Lavalink node address and session before remote dispatch.
    /// </summary>
    /// <param name = "guildId">The Discord guild whose persisted state is addressed.</param>
    /// <param name = "queueItemId">The durable queue-item identifier, when a specific item is required.</param>
    /// <param name = "operationId">The nonempty playback operation identifier used to fence stale or duplicate state transitions.</param>
    /// <param name = "nodeAddress">The Lavalink node address bound to the pending operation before dispatch.</param>
    /// <param name = "nodeSessionId">The Lavalink node-session identity bound to the pending operation.</param>
    /// <param name = "cancellationToken">Cancels supported database work and retry backoff; owned rollback uses its own deadline.</param>
    /// <returns>True when the documented check or conditional mutation succeeds; otherwise false.</returns>
    Task<bool> BindStartNodeAsync(
        ulong guildId,
        long queueItemId,
        Guid operationId,
        string nodeAddress,
        string nodeSessionId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Conditionally settles a pending attempt after its owner has established that the bound remote session is gone.
    /// </summary>
    /// <remarks>The caller supplies remote-disconnect evidence; this repository does not contact Lavalink. Do not call this solely because a local wait was cancelled.</remarks>
    /// <param name = "guildId">The Discord guild whose persisted state is addressed.</param>
    /// <param name = "queueItemId">The durable queue-item identifier, when a specific item is required.</param>
    /// <param name = "operationId">The nonempty playback operation identifier used to fence stale or duplicate state transitions.</param>
    /// <param name = "cancellationToken">Cancels supported database work and retry backoff; owned rollback uses its own deadline.</param>
    /// <returns>True when the documented check or conditional mutation succeeds; otherwise false.</returns>
    Task<bool> ResolveDisconnectedStartAsync(
        ulong guildId,
        long queueItemId,
        Guid operationId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Records confirmed transport for the matching operation without dispatching playback again.
    /// </summary>
    /// <param name = "guildId">The Discord guild whose persisted state is addressed.</param>
    /// <param name = "queueItemId">The durable queue-item identifier, when a specific item is required.</param>
    /// <param name = "operationId">The nonempty playback operation identifier used to fence stale or duplicate state transitions.</param>
    /// <param name = "cancellationToken">Cancels supported database work and retry backoff; owned rollback uses its own deadline.</param>
    /// <returns>True when the documented check or conditional mutation succeeds; otherwise false.</returns>
    Task<bool> ConfirmStartTransportAsync(
        ulong guildId,
        long queueItemId,
        Guid operationId,
        CancellationToken cancellationToken = default);
}
