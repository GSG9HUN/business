using DC_bot.BotControl;
using DC_bot.Db;
using DC_bot.Entities.Playback;
using DC_bot.Interface.Service.Persistence.Models.Queue;
using DC_bot.Repositories.Playback;
using DC_bot.Repositories.Shared;
using Microsoft.EntityFrameworkCore;

namespace DC_bot.Repositories.Queue;

/// <summary>
/// Applies operation-fenced queue start and completion transitions inside serializable database transactions.
/// </summary>
/// <param name = "factory">The injected factory used to create a fresh context for each operation or transaction attempt.</param>
internal sealed class QueueClaimService(IDbContextFactory<BotDbContext> factory)
{

    /// <summary>
    /// Atomically claims the requested or next eligible queue item; reuses the same pending operation and rejects a competing pending claim.
    /// </summary>
    /// <remarks>Operation identity fences stale or duplicate updates. A durable claim is not proof that remote playback started; the caller must reconcile uncertain transport before releasing or repeating it.</remarks>
    /// <param name = "guildId">The Discord guild whose persisted state is addressed.</param>
    /// <param name = "operationId">The nonempty playback operation identifier used to fence stale or duplicate state transitions.</param>
    /// <param name = "queueItemId">The durable queue-item identifier, when a specific item is required.</param>
    /// <param name = "cancellationToken">Cancels supported database work and retry backoff; owned rollback uses its own deadline.</param>
    /// <returns>The claimed item and operation, a busy result for a competing attempt, or an empty result when no candidate exists.</returns>
    public Task<QueueClaimResult> ClaimForStartAsync(
        ulong guildId,
        Guid operationId,
        long? queueItemId = null,
        CancellationToken cancellationToken = default)
    {
        if (operationId == Guid.Empty)
        {
            throw new ArgumentException("Operation ID is required.", nameof(operationId));
        }

        return PostgreSqlConcurrencyHelper.ExecuteInSerializableTransactionWithRetryAsync(factory, async (db, ct) =>
        {
            var pending = await db.GuildQueueItems.SingleOrDefaultAsync(q => q.GuildId == guildId && q.State == QueueItemState.Starting, ct);

            if (pending is not null)
            {
                return pending.PlaybackOperationId == operationId ? new QueueClaimResult(QueueClaimStatus.Claimed, new QueueStartClaim(operationId, QueueItemMapper.ToRecord(pending))) : new QueueClaimResult(QueueClaimStatus.Busy);
            }

            var batch = await QueueBatchStore.GetActiveAsync(db, guildId, create: false, ct);

            if (batch is null)
            {
                return new QueueClaimResult(QueueClaimStatus.Empty);
            }

            var query = db.GuildQueueItems.Where(q => q.GuildId == guildId && q.BatchId == batch.Id);
            var item = queueItemId.HasValue ? await query.SingleOrDefaultAsync(q => q.Id == queueItemId.Value, ct) : await query.Where(q => q.State == QueueItemState.Queued)
                .OrderBy(q => q.Position)
                .ThenBy(q => q.Id)
                .FirstOrDefaultAsync(ct);

            if (item is null)
            {
                return new QueueClaimResult(QueueClaimStatus.Empty);
            }

            item.StartPreviousState = item.State;
            item.StartPreviousOperationId = item.PlaybackOperationId;
            item.PlaybackOperationId = operationId;
            item.State = QueueItemState.Starting;
            item.PlaybackNodeAddress = null;
            item.PlaybackNodeSessionId = null;
            item.PlaybackTransportConfirmed = false;
            await db.SaveChangesAsync(ct);
            await QueueRealtimeNotifier.NotifyQueueUpdatedAsync(db, guildId, MobileRealtimeEventNames.QueueSnapshotChanged, ct);
            return new QueueClaimResult(QueueClaimStatus.Claimed, new QueueStartClaim(operationId, QueueItemMapper.ToRecord(item)));
        }, cancellationToken);
    }

    /// <summary>
    /// Idempotently commits a matching start claim, initial position, and pause state, then emits queue and playback events.
    /// </summary>
    /// <remarks>Operation identity fences stale or duplicate updates. A durable claim is not proof that remote playback started; the caller must reconcile uncertain transport before releasing or repeating it.</remarks>
    /// <param name = "guildId">The Discord guild whose persisted state is addressed.</param>
    /// <param name = "queueItemId">The durable queue-item identifier, when a specific item is required.</param>
    /// <param name = "operationId">The nonempty playback operation identifier used to fence stale or duplicate state transitions.</param>
    /// <param name = "trackIdentifier">The encoded source-track identity stored independently of any runtime player.</param>
    /// <param name = "realtimeEventName">The shared realtime event name, or the documented default when omitted.</param>
    /// <param name = "clearSourceMetadata">Whether confirmed replacement playback clears stale reload-source metadata.</param>
    /// <param name="initialPosition">The recovery position committed in the same transaction as queue ownership.</param>
    /// <param name="initiallyPaused">Whether the confirmed remote start remains paused.</param>
    /// <param name = "cancellationToken">Cancels supported database work and retry backoff; owned rollback uses its own deadline.</param>
    /// <returns>True when the documented check or conditional mutation succeeds; otherwise false.</returns>
    public Task<bool> CompleteStartAsync(
        ulong guildId,
        long queueItemId,
        Guid operationId,
        string trackIdentifier,
        string realtimeEventName,
        bool clearSourceMetadata = false,
        TimeSpan? initialPosition = null,
        bool initiallyPaused = false,
        CancellationToken cancellationToken = default)
    {
        return PostgreSqlConcurrencyHelper.ExecuteInSerializableTransactionWithRetryAsync(factory, async (db, ct) =>
        {
            var item = await db.GuildQueueItems.SingleOrDefaultAsync(q => q.Id == queueItemId && q.GuildId == guildId && q.PlaybackOperationId == operationId, ct);

            if (item is null)
            {
                return false;
            }

            if (item.State == QueueItemState.Playing)
            {
                return true;
            }

            if (item.State != QueueItemState.Starting)
            {
                return false;
            }

            var now = DateTimeOffset.UtcNow;
            var previous = await db.GuildQueueItems.Where(q => q.GuildId == guildId && q.Id != queueItemId && q.State == QueueItemState.Playing)
                .ToListAsync(ct);

            foreach (var old in previous)
            {
                old.State = QueueItemState.Skipped;
                old.SkippedAtUtc = now;
            }

            item.State = QueueItemState.Playing;
            item.StartPreviousState = null;
            item.StartPreviousOperationId = null;
            item.TrackIdentifier = trackIdentifier;

            if (item.BatchItemId is { } batchItemId)
            {
                var canonical = await db.PlaybackBatchItems.SingleAsync(x => x.Id == batchItemId, ct);
                canonical.TrackIdentifier = trackIdentifier;

                if (clearSourceMetadata)
                {
                    canonical.SourceQuery = null;
                    canonical.SourceSearchMode = null;
                }
            }

            item.PlayedAtUtc = null;
            item.SkippedAtUtc = null;

            if (clearSourceMetadata)
            {
                item.SourceQuery = null;
                item.SourceSearchMode = null;
            }

            var state = await db.GuildPlaybackStates.SingleOrDefaultAsync(p => p.GuildId == guildId, ct);

            if (state is null)
            {
                state = new GuildPlaybackStateEntity
                {
                    GuildId = guildId
                };
                db.GuildPlaybackStates.Add(state);
            }

            state.CurrentTrackIdentifier = trackIdentifier;
            state.QueueItemId = queueItemId;
            state.IsPaused = initiallyPaused;
            state.PositionSeconds = initialPosition.HasValue
                ? checked((int)Math.Max(0, initialPosition.Value.TotalSeconds))
                : 0;
            state.PositionUpdatedAtUtc = now;
            state.UpdatedAtUtc = now;
            await db.SaveChangesAsync(ct);
            await QueueRealtimeNotifier.NotifyQueueUpdatedAsync(db, guildId, MobileRealtimeEventNames.QueueItemClaimed, ct);
            await PlaybackRealtimeNotifier.NotifyPlaybackUpdatedAsync(db, guildId, realtimeEventName, ct);
            return true;
        }, cancellationToken);
    }

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
    public Task<bool> ReleaseStartAsync(
        ulong guildId,
        long queueItemId,
        Guid operationId,
        bool skip,
        CancellationToken cancellationToken = default)
    {
        return PostgreSqlConcurrencyHelper.ExecuteInSerializableTransactionWithRetryAsync(factory, async (db, ct) =>
        {
            var item = await db.GuildQueueItems.SingleOrDefaultAsync(q => q.GuildId == guildId && q.Id == queueItemId && q.State == QueueItemState.Starting && q.PlaybackOperationId == operationId, ct);

            if (item is null)
            {
                return false;
            }

            item.State = skip ? QueueItemState.Skipped : item.StartPreviousState ?? QueueItemState.Queued;
            item.StartPreviousState = null;
            item.PlaybackOperationId = skip ? null : item.StartPreviousOperationId;
            item.StartPreviousOperationId = null;

            if (skip)
            {
                item.SkippedAtUtc = DateTimeOffset.UtcNow;
            }

            await db.SaveChangesAsync(ct);
            await QueueRealtimeNotifier.NotifyQueueUpdatedAsync(db, guildId, MobileRealtimeEventNames.QueueSnapshotChanged, ct);
            return true;
        }, cancellationToken);
    }

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
    public Task<bool> CompleteAttemptAsync(
        ulong guildId,
        long queueItemId,
        Guid operationId,
        bool played,
        CancellationToken cancellationToken = default)
    {
        return PostgreSqlConcurrencyHelper.ExecuteInSerializableTransactionWithRetryAsync(factory, async (db, ct) =>
        {
            var item = await db.GuildQueueItems.SingleOrDefaultAsync(q => q.GuildId == guildId && q.Id == queueItemId && q.PlaybackOperationId == operationId && (q.State == QueueItemState.Playing || q.State == QueueItemState.Starting), ct);

            if (item is null)
            {
                return false;
            }

            item.State = played ? QueueItemState.Played : QueueItemState.Skipped;
            item.StartPreviousState = null;
            item.StartPreviousOperationId = null;

            if (played)
            {
                item.PlayedAtUtc = DateTimeOffset.UtcNow;
            }
            else
            {
                item.SkippedAtUtc = DateTimeOffset.UtcNow;
            }

            await db.SaveChangesAsync(ct);
            await QueueRealtimeNotifier.NotifyQueueUpdatedAsync(db, guildId, MobileRealtimeEventNames.QueueSnapshotChanged, ct);
            return true;
        }, cancellationToken);
    }
}
