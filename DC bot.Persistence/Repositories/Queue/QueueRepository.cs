using DC_bot.Db;
using DC_bot.Interface.Service.Persistence.Exceptions;
using DC_bot.Entities.Queue;
using DC_bot.Interface.Service.Persistence.Models.Queue;
using DC_bot.Interface.Service.Persistence.Queue;
using DC_bot.Repositories.Guilds;
using DC_bot.Repositories.Shared;
using DC_bot.BotControl;
using Microsoft.EntityFrameworkCore;

namespace DC_bot.Repositories.Queue;

/// <summary>
/// Exposes durable queue edits, canonical batch history, and fenced playback-start operations through injected persistence helpers.
/// </summary>
public class QueueRepository : IQueueRepository
{
    private readonly IDbContextFactory<BotDbContext> _dbContextFactory;

    /// <summary>
    /// Initializes QueueRepository using the supplied persistence dependencies without constructing fallback services.
    /// </summary>
    /// <param name = "dbContextFactory">The injected factory used to create a fresh context for each operation or transaction attempt.</param>
    /// <param name = "queueClaimService">The queue claim service value used by this storage contract.</param>
    /// <param name = "batches">The batches value used by this storage contract.</param>
    internal QueueRepository(
        IDbContextFactory<BotDbContext> dbContextFactory,
        QueueClaimService queueClaimService,
        QueueBatchRepository batches)
    {
        _dbContextFactory = dbContextFactory ?? throw new ArgumentNullException(nameof(dbContextFactory));
        _queueClaimService = queueClaimService ?? throw new ArgumentNullException(nameof(queueClaimService));
        _batches = batches ?? throw new ArgumentNullException(nameof(batches));
    }

    private const int MaxQueuedItemsPerGuild = 50;
    private readonly QueueClaimService _queueClaimService;
    private readonly QueueBatchRepository _batches;

    /// <inheritdoc />
    public async Task<IReadOnlyList<QueueItemRecord>> GetSnapshotItemsAsync(
        ulong guildId,
        IReadOnlyCollection<long> queueItemIds,
        CancellationToken cancellationToken)
    {
        await using var dbContext = await _dbContextFactory.CreateDbContextAsync(cancellationToken);
        var entities = await dbContext.GuildQueueItems.AsNoTracking()
            .Where(item => item.GuildId == guildId && queueItemIds.Contains(item.Id))
            .ToListAsync(cancellationToken);
        return entities.Select(QueueItemMapper.ToRecord)
            .ToList();
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<QueueItemRecord>> GetDisplayItemsAsync(
        ulong guildId,
        CancellationToken cancellationToken = default)
    {
        await using var dbContext = await _dbContextFactory.CreateDbContextAsync(cancellationToken);
        var currentId = await dbContext.GuildPlaybackStates.Where(state => state.GuildId == guildId
                && state.CurrentTrackIdentifier != null)
            .Select(state => state.QueueItemId)
            .FirstOrDefaultAsync(cancellationToken);
        var activeBatches = dbContext.GuildQueueItems.Where(item => item.GuildId == guildId
                && (item.State == QueueItemState.Queued
                    || item.State == QueueItemState.Starting || item.State == QueueItemState.Playing
                    || item.Id == currentId))
            .Select(item => item.EnqueueGroupId);
        var latestBatch = dbContext.GuildQueueItems.Where(item => item.GuildId == guildId)
            .OrderByDescending(item => item.Id)
            .Select(item => item.EnqueueGroupId)
            .Take(1);
        var entities = await dbContext.GuildQueueItems.AsNoTracking()
            .Where(item => item.GuildId == guildId
                && (activeBatches.Contains(item.EnqueueGroupId)
            || latestBatch.Contains(item.EnqueueGroupId)))
            .OrderBy(item => item.AddedAtUtc)
            .ThenBy(item => item.BatchPosition)
            .ThenBy(item => item.Id)
            .ToListAsync(cancellationToken);
        return entities.Select(entity => entity.Id == currentId
                ? QueueItemMapper.ToRecord(entity) with { State = QueueItemState.Playing }
                : QueueItemMapper.ToRecord(entity))
            .ToList();
    }

    /// <inheritdoc/>
    public Task<bool> TryEnableBatchRepeatAsync(ulong guildId, CancellationToken cancellationToken = default) => _batches.TryEnableRepeatAsync(guildId, cancellationToken);

    /// <inheritdoc/>
    public Task<IReadOnlyList<QueueItemToEnqueue>> GetActiveBatchItemsAsync(
        ulong guildId,
        CancellationToken cancellationToken = default) => _batches.GetItemsAsync(guildId, cancellationToken);

    /// <inheritdoc/>
    public Task<bool> RestartActiveBatchAsync(ulong guildId, CancellationToken cancellationToken = default) => _batches.RestartAsync(guildId, cancellationToken);

    /// <inheritdoc/>
    public Task<bool> CloseBatchIfIdleAsync(ulong guildId, CancellationToken cancellationToken = default) => _batches.CloseAsync(guildId, explicitLeave: false, cancellationToken);

    /// <inheritdoc/>
    public async Task CloseActiveBatchAsync(ulong guildId, CancellationToken cancellationToken = default) => _ = await _batches.CloseAsync(guildId, explicitLeave: true, cancellationToken);

    /// <inheritdoc/>
    public Task<IReadOnlyList<PlaybackBatchRecord>> GetRecentBatchesAsync(
        ulong guildId,
        int limit = 10,
        CancellationToken cancellationToken = default) => _batches.GetRecentAsync(guildId, limit, cancellationToken);

    /// <inheritdoc/>
    public async Task<IReadOnlyList<QueueItemRecord>> GetQueuedItemsAsync(
        ulong guildId,
        CancellationToken cancellationToken = default)
    {
        await using var dbContext = await _dbContextFactory.CreateDbContextAsync(cancellationToken);
        var entities = await dbContext.GuildQueueItems.AsNoTracking()
            .Where(item => item.GuildId == guildId
            && (item.State == QueueItemState.Queued
            || item.State == QueueItemState.Starting))
            .OrderBy(item => item.Position)
            .ToListAsync(cancellationToken);
        return entities.Select(QueueItemMapper.ToRecord)
            .ToList();
    }

    /// <summary>
    /// Rejects queue edits while a playback-start attempt still requires reconciliation.
    /// </summary>
    /// <param name = "dbContext">The caller&apos;s EF context, whose current transaction remains owned by the caller.</param>
    /// <param name = "guildId">The Discord guild whose persisted state is addressed.</param>
    /// <param name = "cancellationToken">Cancels supported database work and retry backoff; owned rollback uses its own deadline.</param>
    /// <returns>A task representing completion of the documented operation.</returns>
    private static async Task EnsureNoPendingStartAsync(BotDbContext dbContext, ulong guildId, CancellationToken cancellationToken)
    {
        if (await dbContext.GuildQueueItems.AnyAsync(item => item.GuildId == guildId
            && item.State == QueueItemState.Starting, cancellationToken))
        {
            throw new InvalidOperationException("Queue editing is blocked while playback start reconciliation is pending.");
        }
    }

    /// <inheritdoc/>
    public async Task<bool> AnyQueuedItemsAsync(ulong guildId, CancellationToken cancellationToken = default)
    {
        await using var dbContext = await _dbContextFactory.CreateDbContextAsync(cancellationToken);
        return await dbContext.GuildQueueItems.AsNoTracking()
            .AnyAsync(item => item.GuildId == guildId && item.State == QueueItemState.Queued, cancellationToken);
    }

    /// <inheritdoc/>
    public async Task<QueueItemRecord?> GetPreviousItemAsync(ulong guildId, CancellationToken cancellationToken = default)
    {
        await using var dbContext = await _dbContextFactory.CreateDbContextAsync(cancellationToken);
        var entity = await (
            from playback in dbContext.GuildPlaybackStates.AsNoTracking()join current in dbContext.GuildQueueItems.AsNoTracking()on playback.QueueItemId equals current.Id
            join currentItem in dbContext.PlaybackBatchItems.AsNoTracking()on current.BatchItemId equals currentItem.Id
            from candidateItem in dbContext.PlaybackBatchItems.AsNoTracking()join candidate in dbContext.GuildQueueItems.AsNoTracking()on candidateItem.Id equals candidate.BatchItemId
            where playback.GuildId == guildId
            && current.GuildId == guildId
            && current.State == QueueItemState.Playing
            && current.BatchId == playback.ActiveBatchId
            && currentItem.BatchId == playback.ActiveBatchId
            && candidateItem.BatchId == currentItem.BatchId
            && !candidateItem.IsRemoved
            && candidateItem.Position < currentItem.Position
            && candidate.GuildId == guildId
            && candidate.BatchId == current.BatchId
            && (candidate.State == QueueItemState.Played
            || candidate.State == QueueItemState.Skipped)orderby candidateItem.Position descending, candidate.Id descending
            select candidate).FirstOrDefaultAsync(cancellationToken);
        return entity is null ? null : QueueItemMapper.ToRecord(entity);
    }

    /// <inheritdoc/>
    public async Task<QueueItemRecord?> GetByIdAsync(long queueItemId, CancellationToken cancellationToken = default)
    {
        await using var dbContext = await _dbContextFactory.CreateDbContextAsync(cancellationToken);
        var entity = await dbContext.GuildQueueItems.AsNoTracking()
            .FirstOrDefaultAsync(item => item.Id == queueItemId, cancellationToken);
        return entity is null ? null : QueueItemMapper.ToRecord(entity);
    }

    /// <inheritdoc/>
    public async Task EnqueueManyAsync(
        ulong guildId,
        IReadOnlyList<QueueItemToEnqueue> queueItems,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(queueItems);

        if (queueItems.Count == 0)
        {
            return;
        }

        await PostgreSqlConcurrencyHelper.ExecuteInSerializableTransactionWithRetryAsync(_dbContextFactory, (dbContext, ct) => EnqueueManyAsync(dbContext, guildId, queueItems, ct), cancellationToken);
    }

    /// <inheritdoc/>
    public async Task ReorderQueuedItemsAsync(
        ulong guildId,
        IReadOnlyList<string> trackIdentifiers,
        string? realtimeEventName,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(trackIdentifiers);
        await PostgreSqlConcurrencyHelper.ExecuteInSerializableTransactionWithRetryAsync(_dbContextFactory, (dbContext, ct) => ReorderQueuedItemsAsync(dbContext, guildId, trackIdentifiers, realtimeEventName, ct), cancellationToken);
    }

    /// <inheritdoc/>
    public async Task<QueueItemRemovalRecord> RemoveQueuedItemAtAsync(
        ulong guildId,
        int trackNumber,
        CancellationToken cancellationToken)
    {
        return await PostgreSqlConcurrencyHelper.ExecuteInSerializableTransactionWithRetryAsync(_dbContextFactory, (dbContext, ct) => RemoveQueuedItemAtAsync(dbContext, guildId, trackNumber, ct), cancellationToken);
    }

    /// <inheritdoc/>
    public async Task MarkAllQueuedAsSkippedAsync(ulong guildId, CancellationToken cancellationToken = default)
    {
        await MarkAllQueuedAsSkippedWithCountAsync(guildId, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<int> MarkAllQueuedAsSkippedWithCountAsync(ulong guildId, CancellationToken cancellationToken = default)
    {
        return await PostgreSqlConcurrencyHelper.ExecuteInSerializableTransactionWithRetryAsync(
            _dbContextFactory,
            async (dbContext, ct) =>
            {
                await EnsureNoPendingStartAsync(dbContext, guildId, ct);
                await QueueRepeatListRepository.ClearAsync(dbContext, guildId, ct);
                var now = DateTimeOffset.UtcNow;
                var count = await dbContext.GuildQueueItems.Where(item => item.GuildId == guildId && item.State == QueueItemState.Queued)
                    .ExecuteUpdateAsync(setters => setters.SetProperty(item => item.State, QueueItemState.Skipped)
                        .SetProperty(item => item.SkippedAtUtc, now), ct);
                await QueueRealtimeNotifier.NotifyQueueUpdatedAsync(
                    dbContext, guildId, MobileRealtimeEventNames.QueueCleared, ct);
                return count;
            },
            cancellationToken);
    }

    /// <inheritdoc/>
    public Task<QueueClaimResult> ClaimForStartAsync(
        ulong guildId,
        Guid operationId,
        long? queueItemId = null,
        CancellationToken cancellationToken = default) => _queueClaimService.ClaimForStartAsync(guildId, operationId, queueItemId, cancellationToken);

    /// <inheritdoc/>
    /// <remarks>Forwards the recovery position and pause state to the same transactional, operation-fenced commit.</remarks>
    public Task<bool> CompleteStartAsync(
        ulong guildId,
        long queueItemId,
        Guid operationId,
        string trackIdentifier,
        string realtimeEventName,
        bool clearSourceMetadata = false,
        TimeSpan? initialPosition = null,
        bool initiallyPaused = false,
        CancellationToken cancellationToken = default) => _queueClaimService.CompleteStartAsync(guildId, queueItemId, operationId, trackIdentifier, realtimeEventName, clearSourceMetadata, initialPosition, initiallyPaused, cancellationToken);

    /// <inheritdoc/>
    public Task<bool> ReleaseStartAsync(
        ulong guildId,
        long queueItemId,
        Guid operationId,
        bool skip,
        CancellationToken cancellationToken = default) => _queueClaimService.ReleaseStartAsync(guildId, queueItemId, operationId, skip, cancellationToken);

    /// <inheritdoc/>
    public Task<bool> CompleteAttemptAsync(
        ulong guildId,
        long queueItemId,
        Guid operationId,
        bool played,
        CancellationToken cancellationToken = default) => _queueClaimService.CompleteAttemptAsync(guildId, queueItemId, operationId, played, cancellationToken);

    /// <inheritdoc/>
    public async Task<bool> TryMarkQueuedAsSkippedAsync(long queueItemId, CancellationToken cancellationToken = default)
    {
        return await PostgreSqlConcurrencyHelper.ExecuteInSerializableTransactionWithRetryAsync(_dbContextFactory, async (dbContext, ct) =>
        {
            var queuedItem = await dbContext.GuildQueueItems.AsNoTracking()
                .Where(item => item.Id == queueItemId && item.State == QueueItemState.Queued)
                .Select(item => new { item.GuildId })
                .SingleOrDefaultAsync(ct);

            if (queuedItem is null)
            {
                return false;
            }

            var now = DateTimeOffset.UtcNow;
            var affected = await dbContext.GuildQueueItems.Where(item => item.Id == queueItemId && item.State == QueueItemState.Queued)
                .ExecuteUpdateAsync(update => update.SetProperty(item => item.State, QueueItemState.Skipped)
                    .SetProperty(item => item.SkippedAtUtc, now), ct);

            if (affected == 0)
            {
                return false;
            }

            await QueueRealtimeNotifier.NotifyQueueUpdatedAsync(dbContext, queuedItem.GuildId, MobileRealtimeEventNames.QueueItemRemoved, ct);
            return true;
        }, cancellationToken);
    }

    /// <summary>
    /// Appends source-tagged queue items to the current canonical batch in order, creating a new batch only when required.
    /// </summary>
    /// <param name = "dbContext">The caller&apos;s EF context, whose current transaction remains owned by the caller.</param>
    /// <param name = "guildId">The Discord guild whose persisted state is addressed.</param>
    /// <param name = "queueItems">The ordered queue entries including source query, search mode, and requester metadata.</param>
    /// <param name = "cancellationToken">Cancels supported database work and retry backoff; owned rollback uses its own deadline.</param>
    /// <returns>A task representing completion of the documented operation.</returns>
    private static async Task EnqueueManyAsync(
        BotDbContext dbContext,
        ulong guildId,
        IReadOnlyList<QueueItemToEnqueue> queueItems,
        CancellationToken cancellationToken)
    {
        await GuildDataBootstrapper.EnsureExistsAsync(dbContext, guildId, cancellationToken);
        var queuedItemCount = await dbContext.GuildQueueItems.CountAsync(item => item.GuildId == guildId && item.State == QueueItemState.Queued, cancellationToken);

        if ((long)queuedItemCount + queueItems.Count > MaxQueuedItemsPerGuild
            && !await GuildPremiumQuery.IsActiveAsync(dbContext, guildId, cancellationToken))
        {
            throw new QueueCapacityExceededException(MaxQueuedItemsPerGuild, queuedItemCount, queueItems.Count);
        }

        var batch = await QueueBatchStore.GetActiveAsync(dbContext, guildId, create: true, cancellationToken) ?? throw new InvalidOperationException("Could not create a playback batch.");
        var batchPosition = await dbContext.PlaybackBatchItems.Where(x => x.BatchId == batch.Id)
            .Select(x => (int? )x.Position)
            .MaxAsync(cancellationToken) ?? -1;
        var maxPosition = await dbContext.GuildQueueItems.Where(item => item.GuildId == guildId)
            .Select(item => (int? )item.Position)
            .MaxAsync(cancellationToken) ?? -1;
        var addedAtUtc = DateTimeOffset.UtcNow;
        var enqueueGroupId = Guid.NewGuid();
        var entities = new List<GuildQueueItemEntity>(queueItems.Count);
        var canonical = new List<DC_bot.Entities.Playback.PlaybackBatchItemEntity>(queueItems.Count);

        for (var index = 0; index < queueItems.Count; index++)
        {
            var queueItem = queueItems[index];
            ArgumentException.ThrowIfNullOrWhiteSpace(queueItem.TrackIdentifier);
            var batchItem = new DC_bot.Entities.Playback.PlaybackBatchItemEntity
            {
                BatchId = batch.Id,
                EnqueueGroupId = enqueueGroupId,
                Position = checked(batchPosition + index + 1),
                TrackIdentifier = queueItem.TrackIdentifier,
                SourceQuery = NormalizeSourceQuery(queueItem.SourceQuery),
                SourceSearchMode = NormalizeSourceSearchMode(queueItem.SourceSearchMode),
                RequestedBy = NormalizeRequestedBy(queueItem.RequestedBy),
                RequestedByDisplayName = NormalizeRequestedBy(queueItem.RequestedByDisplayName),
                AddedAtUtc = addedAtUtc
            };
            canonical.Add(batchItem);
            entities.Add(new GuildQueueItemEntity
            {
                GuildId = guildId,
                BatchId = batch.Id,
                Position = checked(maxPosition + index + 1),
                TrackIdentifier = queueItem.TrackIdentifier,
                SourceQuery = NormalizeSourceQuery(queueItem.SourceQuery),
                SourceSearchMode = NormalizeSourceSearchMode(queueItem.SourceSearchMode),
                RequestedBy = NormalizeRequestedBy(queueItem.RequestedBy),
                RequestedByDisplayName = NormalizeRequestedBy(queueItem.RequestedByDisplayName),
                State = QueueItemState.Queued,
                EnqueueGroupId = enqueueGroupId,
                BatchPosition = checked(batchPosition + index + 1),
                AddedAtUtc = addedAtUtc
            });
        }

        dbContext.PlaybackBatchItems.AddRange(canonical);
        await dbContext.SaveChangesAsync(cancellationToken);

        for (var index = 0; index < entities.Count; index++)
        {
            entities[index].BatchItemId = canonical[index].Id;
        }

        dbContext.GuildQueueItems.AddRange(entities);
        await dbContext.SaveChangesAsync(cancellationToken);
        await QueueRealtimeNotifier.NotifyQueueUpdatedAsync(dbContext, guildId, MobileRealtimeEventNames.QueueItemsAdded, cancellationToken);
    }

    /// <summary>
    /// Changes waiting positions without changing canonical batch membership, then emits the selected queue event.
    /// </summary>
    /// <param name = "dbContext">The caller&apos;s EF context, whose current transaction remains owned by the caller.</param>
    /// <param name = "guildId">The Discord guild whose persisted state is addressed.</param>
    /// <param name = "trackIdentifiers">The track identities in the requested durable order, preserving duplicate occurrences.</param>
    /// <param name = "realtimeEventName">The shared realtime event name, or the documented default when omitted.</param>
    /// <param name = "cancellationToken">Cancels supported database work and retry backoff; owned rollback uses its own deadline.</param>
    /// <returns>A task representing completion of the documented operation.</returns>
    private static async Task ReorderQueuedItemsAsync(
        BotDbContext dbContext,
        ulong guildId,
        IReadOnlyList<string> trackIdentifiers,
        string? realtimeEventName,
        CancellationToken cancellationToken)
    {
        await EnsureNoPendingStartAsync(dbContext, guildId, cancellationToken);
        var queuedEntities = await dbContext.GuildQueueItems.Where(item => item.GuildId == guildId && item.State == QueueItemState.Queued)
            .OrderBy(item => item.Position)
            .ToListAsync(cancellationToken);

        if (queuedEntities.Count != trackIdentifiers.Count)
        {
            throw new InvalidOperationException("Queued track count does not match the reordered queue count.");
        }

        var entityLookup = queuedEntities.GroupBy(item => item.TrackIdentifier)
            .ToDictionary(group => group.Key, group => new Queue<GuildQueueItemEntity>(group.OrderBy(item => item.Position)));
        var reorderedEntities = new List<GuildQueueItemEntity>(trackIdentifiers.Count);

        foreach (var trackIdentifier in trackIdentifiers)
        {
            if (!entityLookup.TryGetValue(trackIdentifier, out var matchingItems)
                || matchingItems.Count == 0)
            {
                throw new InvalidOperationException("Reordered queue contains a track that does not match the persisted queue.");
            }

            reorderedEntities.Add(matchingItems.Dequeue());
        }

        var originalPositions = queuedEntities.Select(item => item.Position)
            .OrderBy(position => position)
            .ToArray();
        var maxPosition = await dbContext.GuildQueueItems.Where(item => item.GuildId == guildId)
            .Select(item => (int? )item.Position)
            .MaxAsync(cancellationToken) ?? -1;
        var temporaryPositionBase = checked(maxPosition + queuedEntities.Count + 1);

        for (var index = 0; index < queuedEntities.Count; index++)
        {
            queuedEntities[index].Position = temporaryPositionBase + index;
        }

        await dbContext.SaveChangesAsync(cancellationToken);

        for (var index = 0; index < reorderedEntities.Count; index++)
        {
            reorderedEntities[index].Position = originalPositions[index];
        }

        await dbContext.SaveChangesAsync(cancellationToken);
        await QueueRepeatListRepository.SynchronizeAsync(dbContext, guildId, queuedEntities, reorderedEntities, cancellationToken);
        await QueueRealtimeNotifier.NotifyQueueUpdatedAsync(dbContext, guildId, realtimeEventName ?? MobileRealtimeEventNames.QueueItemMoved, cancellationToken);
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<PendingPlaybackStart>> GetPendingStartsAsync(
        ulong? guildId = null,
        CancellationToken cancellationToken = default)
    {
        await using var db = await _dbContextFactory.CreateDbContextAsync(cancellationToken);
        var query = db.GuildQueueItems.AsNoTracking()
            .Where(item => item.State == QueueItemState.Starting);

        if (guildId.HasValue)
        {
            query = query.Where(item => item.GuildId == guildId.Value);
        }

        var rows = await query.OrderBy(item => item.GuildId)
            .ThenBy(item => item.Id)
            .Select(item => new { item.GuildId, item.Id, item.PlaybackOperationId, item.PlaybackNodeAddress, item.PlaybackNodeSessionId, item.PlaybackTransportConfirmed })
            .ToListAsync(cancellationToken);
        return rows.Select(item => new PendingPlaybackStart(item.GuildId, item.Id, item.PlaybackOperationId.GetValueOrDefault(), item.PlaybackNodeAddress, item.PlaybackNodeSessionId, item.PlaybackTransportConfirmed))
            .ToList();
    }

    /// <inheritdoc/>
    public Task<bool> BindStartNodeAsync(
        ulong guildId,
        long queueItemId,
        Guid operationId,
        string nodeAddress,
        string nodeSessionId,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(nodeAddress);
        ArgumentException.ThrowIfNullOrWhiteSpace(nodeSessionId);
        return PostgreSqlConcurrencyHelper.ExecuteInSerializableTransactionWithRetryAsync(_dbContextFactory, async (db, ct) =>
        {
            var item = await db.GuildQueueItems.SingleOrDefaultAsync(item => item.GuildId == guildId
                && item.Id == queueItemId
                && item.State == QueueItemState.Starting
                && item.PlaybackOperationId == operationId, ct);

            if (item is null)
            {
                return false;
            }

            // An operation's remote scope is immutable once written.

            if (item.PlaybackNodeSessionId is not null)
            {
                return item.PlaybackNodeSessionId == nodeSessionId
                    && item.PlaybackNodeAddress == nodeAddress;
            }

            item.PlaybackNodeAddress = nodeAddress;
            item.PlaybackNodeSessionId = nodeSessionId;
            await db.SaveChangesAsync(ct);
            return true;
        }, cancellationToken);
    }

    /// <inheritdoc/>
    public Task<bool> ResolveDisconnectedStartAsync(
        ulong guildId,
        long queueItemId,
        Guid operationId,
        CancellationToken cancellationToken = default)
    {
        return PostgreSqlConcurrencyHelper.ExecuteInSerializableTransactionWithRetryAsync(_dbContextFactory, async (db, ct) =>
        {
            var item = await db.GuildQueueItems.SingleOrDefaultAsync(item => item.GuildId == guildId
                && item.Id == queueItemId
                && item.PlaybackOperationId == operationId, ct);

            if (item is null)
            {
                return false;
            }

            if (item.State == QueueItemState.Skipped)
            {
                return true;
            }

            if (item.State != QueueItemState.Starting)
            {
                return false;
            }

            var crossedItems = await db.GuildQueueItems.Where(q => q.GuildId == guildId
                    && q.Id != queueItemId
                    && q.PlaybackOperationId == operationId
                    && q.State == QueueItemState.Skipped
                    && q.StartPreviousState == QueueItemState.Queued)
                .ToListAsync(ct);

            foreach (var crossed in crossedItems)
            {
                if (!item.PlaybackTransportConfirmed)
                {
                    crossed.State = QueueItemState.Queued;
                    crossed.SkippedAtUtc = null;
                    crossed.PlaybackOperationId = crossed.StartPreviousOperationId;
                }

                crossed.StartPreviousState = null;
                crossed.StartPreviousOperationId = null;
            }

            item.State = QueueItemState.Skipped;
            item.SkippedAtUtc = DateTimeOffset.UtcNow;
            item.StartPreviousState = null;
            item.StartPreviousOperationId = null;
            // Keep operation + node binding as diagnostic ownership evidence.
            var playback = await db.GuildPlaybackStates.SingleOrDefaultAsync(state => state.GuildId == guildId && state.QueueItemId == queueItemId, ct);

            if (playback is not null)
            {
                playback.CurrentTrackIdentifier = null;
                playback.QueueItemId = null;
                playback.IsPaused = false;
                playback.PositionSeconds = 0;
                playback.PositionUpdatedAtUtc = DateTimeOffset.UtcNow;
                playback.UpdatedAtUtc = DateTimeOffset.UtcNow;
            }

            await db.SaveChangesAsync(ct);
            await QueueRealtimeNotifier.NotifyQueueUpdatedAsync(db, guildId, MobileRealtimeEventNames.QueueSnapshotChanged, ct);
            await Playback.PlaybackRealtimeNotifier.NotifyPlaybackUpdatedAsync(db, guildId, MobileRealtimeEventNames.PlaybackStopped, ct);
            return true;
        }, cancellationToken);
    }

    /// <inheritdoc/>
    public Task<bool> ConfirmStartTransportAsync(
        ulong guildId,
        long queueItemId,
        Guid operationId,
        CancellationToken cancellationToken = default)
    {
        return PostgreSqlConcurrencyHelper.ExecuteInSerializableTransactionWithRetryAsync(_dbContextFactory, async (db, ct) =>
        {
            var item = await db.GuildQueueItems.SingleOrDefaultAsync(item => item.GuildId == guildId
                && item.Id == queueItemId
                && item.PlaybackOperationId == operationId
                && (item.State == QueueItemState.Starting
                || item.State == QueueItemState.Playing), ct);

            if (item is null
                || item.PlaybackNodeAddress is null
                || item.PlaybackNodeSessionId is null)
            {
                return false;
            }

            item.PlaybackTransportConfirmed = true;
            await db.SaveChangesAsync(ct);
            return true;
        }, cancellationToken);
    }

    /// <summary>
    /// Removes the numbered waiting item and returns its identity and the resulting queue size.
    /// </summary>
    /// <param name = "dbContext">The caller&apos;s EF context, whose current transaction remains owned by the caller.</param>
    /// <param name = "guildId">The Discord guild whose persisted state is addressed.</param>
    /// <param name = "trackNumber">The one-based track number used by the public queue or playlist operation.</param>
    /// <param name = "cancellationToken">Cancels supported database work and retry backoff; owned rollback uses its own deadline.</param>
    /// <returns>The removal outcome, resulting queue size, and removed item when applicable.</returns>
    private static async Task<QueueItemRemovalRecord> RemoveQueuedItemAtAsync(
        BotDbContext dbContext,
        ulong guildId,
        int trackNumber,
        CancellationToken cancellationToken)
    {
        await EnsureNoPendingStartAsync(dbContext, guildId, cancellationToken);
        var queuedEntities = await dbContext.GuildQueueItems.Where(item => item.GuildId == guildId && item.State == QueueItemState.Queued)
            .OrderBy(item => item.Position)
            .ToListAsync(cancellationToken);
        var removeIndex = trackNumber - 1;

        if (trackNumber < 1 || removeIndex >= queuedEntities.Count)
        {
            return new QueueItemRemovalRecord(false, queuedEntities.Count, null);
        }

        var removedEntity = queuedEntities[removeIndex];
        var removedRecord = QueueItemMapper.ToRecord(removedEntity);
        var survivors = queuedEntities.Where((_, index) => index != removeIndex)
            .ToList();
        var originalPositions = queuedEntities.Select(item => item.Position)
            .OrderBy(position => position)
            .ToArray();
        var maxPosition = await dbContext.GuildQueueItems.Where(item => item.GuildId == guildId)
            .Select(item => (int? )item.Position)
            .MaxAsync(cancellationToken) ?? -1;
        var temporaryPositionBase = checked(maxPosition + queuedEntities.Count + 1);

        for (var index = 0; index < survivors.Count; index++)
        {
            survivors[index].Position = temporaryPositionBase + index;
        }

        removedEntity.State = QueueItemState.Skipped;
        removedEntity.SkippedAtUtc = DateTimeOffset.UtcNow;
        await dbContext.SaveChangesAsync(cancellationToken);

        for (var index = 0; index < survivors.Count; index++)
        {
            survivors[index].Position = originalPositions[index];
        }

        await dbContext.SaveChangesAsync(cancellationToken);
        await QueueRepeatListRepository.SynchronizeAsync(dbContext, guildId, queuedEntities, survivors, cancellationToken);
        await QueueRealtimeNotifier.NotifyQueueUpdatedAsync(dbContext, guildId, MobileRealtimeEventNames.QueueItemRemoved, cancellationToken);
        return new QueueItemRemovalRecord(true, queuedEntities.Count, removedRecord);
    }

    /// <summary>
    /// Normalizes requester metadata before persistence using the storage length limit.
    /// </summary>
    /// <param name = "requestedBy">The optional requester display value, subject to the storage length limit.</param>
    /// <returns>The matching value, or null when no value satisfies the documented lookup.</returns>
    private static string? NormalizeRequestedBy(string? requestedBy)
    {
        if (string.IsNullOrWhiteSpace(requestedBy))
        {
            return null;
        }

        var trimmed = requestedBy.Trim();
        return trimmed.Length <= 128 ? trimmed : trimmed[..128];
    }

    /// <summary>
    /// Normalizes source query metadata before persistence using the storage length limit.
    /// </summary>
    /// <param name = "sourceQuery">The optional original reloadable URL or query, subject to the storage length limit.</param>
    /// <returns>The matching value, or null when no value satisfies the documented lookup.</returns>
    private static string? NormalizeSourceQuery(string? sourceQuery)
    {
        if (string.IsNullOrWhiteSpace(sourceQuery))
        {
            return null;
        }

        return sourceQuery.Trim();
    }

    /// <summary>
    /// Normalizes the persisted search-mode name using the storage length limit.
    /// </summary>
    /// <param name = "sourceSearchMode">The optional persisted provider/search-mode name.</param>
    /// <returns>The matching value, or null when no value satisfies the documented lookup.</returns>
    private static string? NormalizeSourceSearchMode(string? sourceSearchMode)
    {
        if (string.IsNullOrWhiteSpace(sourceSearchMode))
        {
            return null;
        }

        var trimmed = sourceSearchMode.Trim();
        return trimmed.Length <= 64 ? trimmed : trimmed[..64];
    }

    /// <inheritdoc />
    public Task<QueueClaimResult> ClaimForSkipToAsync(
        ulong guildId,
        Guid operationId,
        long targetId,
        IReadOnlyList<long> expectedIds,
        CancellationToken cancellationToken = default)
    {
        return _queueClaimService.ClaimForSkipToAsync(guildId, operationId, targetId, expectedIds, cancellationToken);
    }

    /// <inheritdoc />
    public async Task EnqueueNextAsync(
        ulong guildId,
        IReadOnlyList<QueueItemToEnqueue> items,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(items);

        if (items.Count == 0)
        {
            return;
        }

        await PostgreSqlConcurrencyHelper.ExecuteInSerializableTransactionWithRetryAsync(
            _dbContextFactory, async (db, ct) =>
            {
                await EnsureNoPendingStartAsync(db, guildId, ct);
                var before = await db.GuildQueueItems.Where(item => item.GuildId == guildId && item.State == QueueItemState.Queued)
                    .OrderBy(item => item.Position)
                    .ToListAsync(ct);
                var oldIds = before.Select(item => item.Id)
                    .ToHashSet();
                await EnqueueManyAsync(db, guildId, items, ct);
                var all = await db.GuildQueueItems.Where(item => item.GuildId == guildId && item.State == QueueItemState.Queued)
                    .OrderBy(item => item.Position)
                    .ToListAsync(ct);
                var ordered = all.Where(item => !oldIds.Contains(item.Id))
                    .Concat(before)
                    .ToList();
                await SaveOccurrenceOrderAsync(db, guildId, all, ordered, false, ct);
            }, cancellationToken);
    }

    /// <inheritdoc />
    public Task<bool> MoveQueuedItemAsync(
        ulong guildId,
        IReadOnlyList<long> expectedIds,
        int fromIndex,
        int toIndex,
        CancellationToken cancellationToken = default)
    {
        return PostgreSqlConcurrencyHelper.ExecuteInSerializableTransactionWithRetryAsync(
            _dbContextFactory, async (db, ct) =>
            {
                await EnsureNoPendingStartAsync(db, guildId, ct);
                var items = await db.GuildQueueItems.Where(item => item.GuildId == guildId && item.State == QueueItemState.Queued)
                    .OrderBy(item => item.Position)
                    .ThenBy(item => item.Id)
                    .ToListAsync(ct);

                if (!items.Select(item => item.Id)
                    .SequenceEqual(expectedIds)
                    || fromIndex < 0
                    || toIndex < 0
                    || fromIndex >= items.Count
                    || toIndex >= items.Count)
                {
                    return false;
                }

                var ordered = items.ToList();
                var selected = ordered[fromIndex];
                ordered.RemoveAt(fromIndex);
                ordered.Insert(toIndex, selected);
                await SaveOccurrenceOrderAsync(db, guildId, items, ordered, true, ct);
                return true;
            }, cancellationToken);
    }

    /// <summary>Persists a permutation by occurrence identity using collision-free temporary positions.</summary>
    private static async Task SaveOccurrenceOrderAsync(
        BotDbContext db,
        ulong guildId,
        IReadOnlyList<GuildQueueItemEntity> before,
        IReadOnlyList<GuildQueueItemEntity> ordered,
        bool synchronizeRepeat,
        CancellationToken token)
    {
        var positions = before.Select(item => item.Position)
            .OrderBy(position => position)
            .ToArray();
        var maximum = await db.GuildQueueItems.Where(item => item.GuildId == guildId)
            .Select(item => (int?)item.Position)
            .MaxAsync(token) ?? -1;

        for (var index = 0; index < before.Count; index++)
        {
            before[index].Position = checked(maximum + index + 1);
        }

        await db.SaveChangesAsync(token);

        for (var index = 0; index < ordered.Count; index++)
        {
            ordered[index].Position = positions[index];
        }

        await db.SaveChangesAsync(token);

        if (synchronizeRepeat)
        {
            await QueueRepeatListRepository.SynchronizeAsync(db, guildId, before, ordered, token);
        }

        await QueueRealtimeNotifier.NotifyQueueUpdatedAsync(db, guildId, MobileRealtimeEventNames.QueueItemMoved, token);
    }


    /// <inheritdoc />
    public Task<bool> EnqueueAutoplayIfEmptyAsync(
        ulong guildId,
        QueueItemToEnqueue item,
        CancellationToken cancellationToken = default)
    {
        return PostgreSqlConcurrencyHelper.ExecuteInSerializableTransactionWithRetryAsync(
            _dbContextFactory, async (db, token) =>
            {
                if (await db.GuildQueueItems.AnyAsync(value => value.GuildId == guildId
                    && (value.State == QueueItemState.Queued || value.State == QueueItemState.Starting), token))
                {
                    return false;
                }

                await EnqueueManyAsync(db, guildId, [item], token);
                return true;
            }, cancellationToken);
    }

}
