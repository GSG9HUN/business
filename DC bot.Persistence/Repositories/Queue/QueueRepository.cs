using DC_bot.Db;
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
    internal QueueRepository(IDbContextFactory<BotDbContext> dbContextFactory, QueueClaimService queueClaimService, QueueBatchRepository batches)
    {
        _dbContextFactory = dbContextFactory ?? throw new ArgumentNullException(nameof(dbContextFactory));
        _queueClaimService = queueClaimService ?? throw new ArgumentNullException(nameof(queueClaimService));
        _batches = batches ?? throw new ArgumentNullException(nameof(batches));
    }

    private const int MaxQueuedItemsPerGuild = 50;
    private readonly QueueClaimService _queueClaimService;
    private readonly QueueBatchRepository _batches;

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
            .Where(item => item.GuildId == guildId && (item.State == QueueItemState.Queued || item.State == QueueItemState.Starting))
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
            where playback.GuildId == guildId && current.GuildId == guildId && current.State == QueueItemState.Playing && current.BatchId == playback.ActiveBatchId && currentItem.BatchId == playback.ActiveBatchId && candidateItem.BatchId == currentItem.BatchId && candidateItem.Position < currentItem.Position && candidate.GuildId == guildId && candidate.BatchId == current.BatchId && (candidate.State == QueueItemState.Played || candidate.State == QueueItemState.Skipped)orderby candidateItem.Position descending, candidate.Id descending
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
        await using var dbContext = await _dbContextFactory.CreateDbContextAsync(cancellationToken);
        await using var transaction = await dbContext.Database.BeginTransactionAsync(cancellationToken);
        try
        {
            var now = DateTimeOffset.UtcNow;
            await dbContext.GuildQueueItems.Where(x => x.GuildId == guildId && x.State == QueueItemState.Queued)
                .ExecuteUpdateAsync(s => s.SetProperty(b => b.State, QueueItemState.Skipped)
                    .SetProperty(b => b.SkippedAtUtc, now), cancellationToken);
            await QueueRealtimeNotifier.NotifyQueueUpdatedAsync(dbContext, guildId, MobileRealtimeEventNames.QueueCleared, cancellationToken);
            await transaction.CommitAsync(cancellationToken);
        }
        catch (Exception operationException)
        {
            await TransactionCleanup.TryRollbackAsync(transaction, operationException);
            throw;
        }
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

        if (queuedItemCount + queueItems.Count > MaxQueuedItemsPerGuild)
        {
            throw new InvalidOperationException($"Queue cannot contain more than {MaxQueuedItemsPerGuild} queued tracks.");
        }

        var batch = await QueueBatchStore.GetActiveAsync(dbContext, guildId, create: true, cancellationToken) ?? throw new InvalidOperationException("Could not create a playback batch.");
        var batchPosition = await dbContext.PlaybackBatchItems.Where(x => x.BatchId == batch.Id)
            .Select(x => (int? )x.Position)
            .MaxAsync(cancellationToken) ?? -1;
        var maxPosition = await dbContext.GuildQueueItems.Where(item => item.GuildId == guildId)
            .Select(item => (int? )item.Position)
            .MaxAsync(cancellationToken) ?? -1;
        var addedAtUtc = DateTimeOffset.UtcNow;
        var entities = new List<GuildQueueItemEntity>(queueItems.Count);
        var canonical = new List<DC_bot.Entities.Playback.PlaybackBatchItemEntity>(queueItems.Count);

        for (var index = 0; index < queueItems.Count; index++)
        {
            var queueItem = queueItems[index];
            ArgumentException.ThrowIfNullOrWhiteSpace(queueItem.TrackIdentifier);
            var batchItem = new DC_bot.Entities.Playback.PlaybackBatchItemEntity
            {
                BatchId = batch.Id,
                Position = checked(batchPosition + index + 1),
                TrackIdentifier = queueItem.TrackIdentifier,
                SourceQuery = NormalizeSourceQuery(queueItem.SourceQuery),
                SourceSearchMode = NormalizeSourceSearchMode(queueItem.SourceSearchMode),
                RequestedBy = NormalizeRequestedBy(queueItem.RequestedBy),
                AddedAtUtc = addedAtUtc
            };
            canonical.Add(batchItem);
            entities.Add(new GuildQueueItemEntity { GuildId = guildId, BatchId = batch.Id, Position = checked(maxPosition + index + 1), TrackIdentifier = queueItem.TrackIdentifier, SourceQuery = NormalizeSourceQuery(queueItem.SourceQuery), SourceSearchMode = NormalizeSourceSearchMode(queueItem.SourceSearchMode), RequestedBy = NormalizeRequestedBy(queueItem.RequestedBy), State = QueueItemState.Queued, AddedAtUtc = addedAtUtc });
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
            var item = await db.GuildQueueItems.SingleOrDefaultAsync(item => item.GuildId == guildId && item.Id == queueItemId && item.State == QueueItemState.Starting && item.PlaybackOperationId == operationId, ct);

            if (item is null)
            {
                return false;
            }

            // An operation's remote scope is immutable once written.
            if (item.PlaybackNodeSessionId is not null)
            {
                return item.PlaybackNodeSessionId == nodeSessionId && item.PlaybackNodeAddress == nodeAddress;
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
            var item = await db.GuildQueueItems.SingleOrDefaultAsync(item => item.GuildId == guildId && item.Id == queueItemId && item.PlaybackOperationId == operationId, ct);

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
            var item = await db.GuildQueueItems.SingleOrDefaultAsync(item => item.GuildId == guildId && item.Id == queueItemId && item.PlaybackOperationId == operationId && (item.State == QueueItemState.Starting || item.State == QueueItemState.Playing), ct);

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
}
