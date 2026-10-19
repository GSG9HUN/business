using DC_bot.Db;
using DC_bot.Entities.Queue;
using DC_bot.Interface.Service.Persistence.Models.Queue;
using DC_bot.Interface.Service.Persistence.Queue;
using DC_bot.Repositories.Guilds;
using DC_bot.Repositories.Shared;
using DC_bot.BotControl;
using Microsoft.EntityFrameworkCore;

namespace DC_bot.Repositories.Queue;

public class QueueRepository(IDbContextFactory<BotDbContext> dbContextFactory) : IQueueRepository
{
    private const int MaxQueuedItemsPerGuild = 50;
    private readonly QueueClaimService _queueClaimService = new(dbContextFactory);

    public async Task<IReadOnlyList<QueueItemRecord>> GetQueuedItemsAsync(
        ulong guildId,
        CancellationToken cancellationToken = default)
    {
        await using var dbContext = await dbContextFactory.CreateDbContextAsync(cancellationToken);

        var entities = await dbContext.GuildQueueItems
            .AsNoTracking()
            .Where(item => item.GuildId == guildId &&
                           (item.State == QueueItemState.Queued || item.State == QueueItemState.Starting))
            .OrderBy(item => item.Position)
            .ToListAsync(cancellationToken);

        return entities.Select(QueueItemMapper.ToRecord).ToList();
    }

    private static async Task EnsureNoPendingStartAsync(
        BotDbContext dbContext, ulong guildId, CancellationToken cancellationToken)
    {
        if (await dbContext.GuildQueueItems.AnyAsync(
                item => item.GuildId == guildId && item.State == QueueItemState.Starting,
                cancellationToken))
            throw new InvalidOperationException(
                "Queue editing is blocked while playback start reconciliation is pending.");
    }

    public async Task<bool> AnyQueuedItemsAsync(ulong guildId, CancellationToken cancellationToken = default)
    {
        await using var dbContext = await dbContextFactory.CreateDbContextAsync(cancellationToken);
        return await dbContext.GuildQueueItems
            .AsNoTracking()
            .AnyAsync(item => item.GuildId == guildId && item.State == QueueItemState.Queued, cancellationToken);
    }

    public async Task<QueueItemRecord?> GetPreviousItemAsync(ulong guildId,
        CancellationToken cancellationToken = default)
    {
        await using var dbContext = await dbContextFactory.CreateDbContextAsync(cancellationToken);

        var entity = await dbContext.GuildQueueItems
            .AsNoTracking()
            .Where(item => item.GuildId == guildId &&
                           (item.State == QueueItemState.Played || item.State == QueueItemState.Skipped))
            .OrderByDescending(item => item.Position)
            .FirstOrDefaultAsync(cancellationToken);

        return entity is null ? null : QueueItemMapper.ToRecord(entity);
    }

    public async Task<QueueItemRecord?> GetByIdAsync(long queueItemId, CancellationToken cancellationToken = default)
    {
        await using var dbContext = await dbContextFactory.CreateDbContextAsync(cancellationToken);

        var entity = await dbContext.GuildQueueItems
            .AsNoTracking()
            .FirstOrDefaultAsync(item => item.Id == queueItemId, cancellationToken);

        return entity is null ? null : QueueItemMapper.ToRecord(entity);
    }

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

        await PostgreSqlConcurrencyHelper.ExecuteInSerializableTransactionWithRetryAsync(
            dbContextFactory,
            (dbContext, ct) => EnqueueManyAsync(dbContext, guildId, queueItems, ct),
            cancellationToken);
    }

    public async Task ReorderQueuedItemsAsync(
        ulong guildId,
        IReadOnlyList<string> trackIdentifiers,
        string? realtimeEventName,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(trackIdentifiers);

        await PostgreSqlConcurrencyHelper.ExecuteInSerializableTransactionWithRetryAsync(
            dbContextFactory,
            (dbContext, ct) => ReorderQueuedItemsAsync(dbContext, guildId, trackIdentifiers, realtimeEventName, ct),
            cancellationToken);
    }

    public async Task<QueueItemRemovalRecord> RemoveQueuedItemAtAsync(
        ulong guildId,
        int trackNumber,
        CancellationToken cancellationToken)
    {
        return await PostgreSqlConcurrencyHelper.ExecuteInSerializableTransactionWithRetryAsync(
            dbContextFactory,
            (dbContext, ct) => RemoveQueuedItemAtAsync(dbContext, guildId, trackNumber, ct),
            cancellationToken);
    }

    public async Task MarkAllQueuedAsSkippedAsync(ulong guildId, CancellationToken cancellationToken = default)
    {
        await using var dbContext = await dbContextFactory.CreateDbContextAsync(cancellationToken);
        await using var transaction = await dbContext.Database.BeginTransactionAsync(cancellationToken);

        try
        {
            var now = DateTimeOffset.UtcNow;

            await dbContext.GuildQueueItems
                .Where(x => x.GuildId == guildId && x.State == QueueItemState.Queued)
                .ExecuteUpdateAsync(s => s
                        .SetProperty(b => b.State, QueueItemState.Skipped)
                        .SetProperty(b => b.SkippedAtUtc, now),
                    cancellationToken);

            await QueueRealtimeNotifier.NotifyQueueUpdatedAsync(dbContext, guildId,
                MobileRealtimeEventNames.QueueCleared,
                cancellationToken);

            await transaction.CommitAsync(cancellationToken);
        }
        catch
        {
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }
    }

    public Task<QueueClaimResult> ClaimForStartAsync(
        ulong guildId, Guid operationId, long? queueItemId = null,
        CancellationToken cancellationToken = default) =>
        _queueClaimService.ClaimForStartAsync(guildId, operationId, queueItemId, cancellationToken);

    public Task<bool> CompleteStartAsync(
        ulong guildId, long queueItemId, Guid operationId,
        string trackIdentifier, string realtimeEventName,
        bool clearSourceMetadata = false,
        CancellationToken cancellationToken = default) =>
        _queueClaimService.CompleteStartAsync(guildId, queueItemId, operationId,
            trackIdentifier, realtimeEventName, clearSourceMetadata, cancellationToken);

    public Task<bool> ReleaseStartAsync(
        ulong guildId, long queueItemId, Guid operationId, bool skip,
        CancellationToken cancellationToken = default) =>
        _queueClaimService.ReleaseStartAsync(guildId, queueItemId, operationId, skip, cancellationToken);

    public Task<bool> CompleteAttemptAsync(
        ulong guildId, long queueItemId, Guid operationId, bool played,
        CancellationToken cancellationToken = default) =>
        _queueClaimService.CompleteAttemptAsync(guildId, queueItemId, operationId, played, cancellationToken);

    public async Task<bool> TryMarkQueuedAsSkippedAsync(long queueItemId, CancellationToken cancellationToken = default)
    {
        return await PostgreSqlConcurrencyHelper.ExecuteInSerializableTransactionWithRetryAsync(
            dbContextFactory,
            async (dbContext, ct) =>
            {
                var queuedItem = await dbContext.GuildQueueItems
                    .AsNoTracking()
                    .Where(item => item.Id == queueItemId && item.State == QueueItemState.Queued)
                    .Select(item => new { item.GuildId })
                    .SingleOrDefaultAsync(ct);

                if (queuedItem is null)
                {
                    return false;
                }

                var now = DateTimeOffset.UtcNow;
                var affected = await dbContext.GuildQueueItems
                    .Where(item => item.Id == queueItemId && item.State == QueueItemState.Queued)
                    .ExecuteUpdateAsync(update => update
                            .SetProperty(item => item.State, QueueItemState.Skipped)
                            .SetProperty(item => item.SkippedAtUtc, now),
                        ct);

                if (affected == 0)
                {
                    return false;
                }

                await QueueRealtimeNotifier.NotifyQueueUpdatedAsync(
                    dbContext,
                    queuedItem.GuildId,
                    MobileRealtimeEventNames.QueueItemRemoved,
                    ct);

                return true;
            },
            cancellationToken);
    }

    private static async Task EnqueueManyAsync(
        BotDbContext dbContext,
        ulong guildId,
        IReadOnlyList<QueueItemToEnqueue> queueItems,
        CancellationToken cancellationToken)
    {
        await GuildDataBootstrapper.EnsureExistsAsync(dbContext, guildId, cancellationToken);

        var queuedItemCount = await dbContext.GuildQueueItems
            .CountAsync(item => item.GuildId == guildId && item.State == QueueItemState.Queued, cancellationToken);

        if (queuedItemCount + queueItems.Count > MaxQueuedItemsPerGuild)
        {
            throw new InvalidOperationException(
                $"Queue cannot contain more than {MaxQueuedItemsPerGuild} queued tracks.");
        }

        var maxPosition = await dbContext.GuildQueueItems
            .Where(item => item.GuildId == guildId)
            .Select(item => (int?)item.Position)
            .MaxAsync(cancellationToken) ?? -1;

        var addedAtUtc = DateTimeOffset.UtcNow;
        var entities = new List<GuildQueueItemEntity>(queueItems.Count);
        for (var index = 0; index < queueItems.Count; index++)
        {
            var queueItem = queueItems[index];
            ArgumentException.ThrowIfNullOrWhiteSpace(queueItem.TrackIdentifier);

            entities.Add(new GuildQueueItemEntity
            {
                GuildId = guildId,
                Position = maxPosition + index + 1,
                TrackIdentifier = queueItem.TrackIdentifier,
                SourceQuery = NormalizeSourceQuery(queueItem.SourceQuery),
                SourceSearchMode = NormalizeSourceSearchMode(queueItem.SourceSearchMode),
                RequestedBy = NormalizeRequestedBy(queueItem.RequestedBy),
                State = QueueItemState.Queued,
                AddedAtUtc = addedAtUtc
            });
        }

        dbContext.GuildQueueItems.AddRange(entities);
        await dbContext.SaveChangesAsync(cancellationToken);
        await QueueRealtimeNotifier.NotifyQueueUpdatedAsync(dbContext, guildId,
            MobileRealtimeEventNames.QueueItemsAdded, cancellationToken);
    }

    private static async Task ReorderQueuedItemsAsync(
        BotDbContext dbContext,
        ulong guildId,
        IReadOnlyList<string> trackIdentifiers,
        string? realtimeEventName,
        CancellationToken cancellationToken)
    {
        await EnsureNoPendingStartAsync(dbContext, guildId, cancellationToken);

        var queuedEntities = await dbContext.GuildQueueItems
            .Where(item => item.GuildId == guildId && item.State == QueueItemState.Queued)
            .OrderBy(item => item.Position)
            .ToListAsync(cancellationToken);

        if (queuedEntities.Count != trackIdentifiers.Count)
        {
            throw new InvalidOperationException("Queued track count does not match the reordered queue count.");
        }

        var entityLookup = queuedEntities
            .GroupBy(item => item.TrackIdentifier)
            .ToDictionary(
                group => group.Key,
                group => new Queue<GuildQueueItemEntity>(group.OrderBy(item => item.Position)));

        var reorderedEntities = new List<GuildQueueItemEntity>(trackIdentifiers.Count);
        foreach (var trackIdentifier in trackIdentifiers)
        {
            if (!entityLookup.TryGetValue(trackIdentifier, out var matchingItems) || matchingItems.Count == 0)
            {
                throw new InvalidOperationException(
                    "Reordered queue contains a track that does not match the persisted queue.");
            }

            reorderedEntities.Add(matchingItems.Dequeue());
        }

        var originalPositions = queuedEntities
            .Select(item => item.Position)
            .OrderBy(position => position)
            .ToArray();

        var maxPosition = await dbContext.GuildQueueItems
            .Where(item => item.GuildId == guildId)
            .Select(item => (int?)item.Position)
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

        await QueueRealtimeNotifier.NotifyQueueUpdatedAsync(dbContext, guildId,
            realtimeEventName ?? MobileRealtimeEventNames.QueueItemMoved,
            cancellationToken);
    }

    public async Task<IReadOnlyList<PendingPlaybackStart>> GetPendingStartsAsync(
        ulong? guildId = null,
        CancellationToken cancellationToken = default)
    {
        await using var db = await dbContextFactory.CreateDbContextAsync(cancellationToken);
        var query = db.GuildQueueItems.AsNoTracking()
            .Where(item => item.State == QueueItemState.Starting);
        if (guildId.HasValue)
            query = query.Where(item => item.GuildId == guildId.Value);

        var rows = await query.OrderBy(item => item.GuildId).ThenBy(item => item.Id)
            .Select(item => new
            {
                item.GuildId,
                item.Id,
                item.PlaybackOperationId,
                item.PlaybackNodeAddress,
                item.PlaybackNodeSessionId,
                item.PlaybackTransportConfirmed
            }).ToListAsync(cancellationToken);

        return rows.Select(item => new PendingPlaybackStart(
            item.GuildId, item.Id, item.PlaybackOperationId.GetValueOrDefault(),
            item.PlaybackNodeAddress, item.PlaybackNodeSessionId,
            item.PlaybackTransportConfirmed)).ToList();
    }

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

        return PostgreSqlConcurrencyHelper.ExecuteInSerializableTransactionWithRetryAsync(
            dbContextFactory, async (db, ct) =>
            {
                var item = await db.GuildQueueItems.SingleOrDefaultAsync(item =>
                    item.GuildId == guildId && item.Id == queueItemId &&
                    item.State == QueueItemState.Starting &&
                    item.PlaybackOperationId == operationId, ct);
                if (item is null) return false;

                // An operation's remote scope is immutable once written.
                if (item.PlaybackNodeSessionId is not null)
                    return item.PlaybackNodeSessionId == nodeSessionId &&
                           item.PlaybackNodeAddress == nodeAddress;

                item.PlaybackNodeAddress = nodeAddress;
                item.PlaybackNodeSessionId = nodeSessionId;
                await db.SaveChangesAsync(ct);
                return true;
            }, cancellationToken);
    }

    public Task<bool> ResolveDisconnectedStartAsync(
        ulong guildId,
        long queueItemId,
        Guid operationId,
        CancellationToken cancellationToken = default)
    {
        return PostgreSqlConcurrencyHelper.ExecuteInSerializableTransactionWithRetryAsync(
            dbContextFactory, async (db, ct) =>
            {
                var item = await db.GuildQueueItems.SingleOrDefaultAsync(item =>
                    item.GuildId == guildId && item.Id == queueItemId &&
                    item.PlaybackOperationId == operationId, ct);
                if (item is null) return false;
                if (item.State == QueueItemState.Skipped) return true;
                if (item.State != QueueItemState.Starting) return false;

                item.State = QueueItemState.Skipped;
                item.SkippedAtUtc = DateTimeOffset.UtcNow;
                item.StartPreviousState = null;
                item.StartPreviousOperationId = null;
                // Keep operation + node binding as diagnostic ownership evidence.
                var playback = await db.GuildPlaybackStates.SingleOrDefaultAsync(
                    state => state.GuildId == guildId && state.QueueItemId == queueItemId, ct);
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
                await QueueRealtimeNotifier.NotifyQueueUpdatedAsync(
                    db, guildId, MobileRealtimeEventNames.QueueSnapshotChanged, ct);
                await DC_bot.Repositories.Playback.PlaybackRealtimeNotifier.NotifyPlaybackUpdatedAsync(
                    db, guildId, MobileRealtimeEventNames.PlaybackStopped, ct);
                return true;
            }, cancellationToken);
    }

    public Task<bool> ConfirmStartTransportAsync(
        ulong guildId, long queueItemId, Guid operationId,
        CancellationToken cancellationToken = default)
    {
        return PostgreSqlConcurrencyHelper.ExecuteInSerializableTransactionWithRetryAsync(
            dbContextFactory, async (db, ct) =>
            {
                var item = await db.GuildQueueItems.SingleOrDefaultAsync(item =>
                    item.GuildId == guildId && item.Id == queueItemId &&
                    item.PlaybackOperationId == operationId &&
                    (item.State == QueueItemState.Starting || item.State == QueueItemState.Playing), ct);
                if (item is null || item.PlaybackNodeAddress is null ||
                    item.PlaybackNodeSessionId is null)
                    return false;
                item.PlaybackTransportConfirmed = true;
                await db.SaveChangesAsync(ct);
                return true;
            }, cancellationToken);
    }


    private static async Task<QueueItemRemovalRecord> RemoveQueuedItemAtAsync(BotDbContext dbContext, ulong guildId,
        int trackNumber, CancellationToken cancellationToken)
    {
        await EnsureNoPendingStartAsync(dbContext, guildId, cancellationToken);

        var queuedEntities = await dbContext.GuildQueueItems
            .Where(item => item.GuildId == guildId && item.State == QueueItemState.Queued)
            .OrderBy(item => item.Position)
            .ToListAsync(cancellationToken);

        var removeIndex = trackNumber - 1;
        if (trackNumber < 1 || removeIndex >= queuedEntities.Count)
        {
            return new QueueItemRemovalRecord(false, queuedEntities.Count, null);
        }

        var removedEntity = queuedEntities[removeIndex];
        var removedRecord = QueueItemMapper.ToRecord(removedEntity);
        var survivors = queuedEntities
            .Where((_, index) => index != removeIndex)
            .ToList();

        var originalPositions = queuedEntities
            .Select(item => item.Position)
            .OrderBy(position => position)
            .ToArray();

        var maxPosition = await dbContext.GuildQueueItems
            .Where(item => item.GuildId == guildId)
            .Select(item => (int?)item.Position)
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

        await QueueRealtimeNotifier.NotifyQueueUpdatedAsync(dbContext, guildId,
            MobileRealtimeEventNames.QueueItemRemoved, cancellationToken);

        return new QueueItemRemovalRecord(true, queuedEntities.Count, removedRecord);
    }

    private static string? NormalizeRequestedBy(string? requestedBy)
    {
        if (string.IsNullOrWhiteSpace(requestedBy))
        {
            return null;
        }

        var trimmed = requestedBy.Trim();
        return trimmed.Length <= 128 ? trimmed : trimmed[..128];
    }

    private static string? NormalizeSourceQuery(string? sourceQuery)
    {
        if (string.IsNullOrWhiteSpace(sourceQuery))
        {
            return null;
        }

        return sourceQuery.Trim();
    }

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