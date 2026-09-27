using DC_bot.Db;
using DC_bot.Entities.Queue;
using DC_bot.Interface.Service.Persistence.Models.Queue;
using DC_bot.Interface.Service.Persistence.Queue;
using DC_bot.Repositories.Guilds;
using DC_bot.Repositories.Shared;
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
            .Where(item => item.GuildId == guildId && item.State == QueueItemState.Queued)
            .OrderBy(item => item.Position)
            .ToListAsync(cancellationToken);

        return entities.Select(QueueItemMapper.ToRecord).ToList();
    }

    public async Task<bool> AnyQueuedItemsAsync(ulong guildId, CancellationToken cancellationToken = default)
    {
        await using var dbContext = await dbContextFactory.CreateDbContextAsync(cancellationToken);
        return await dbContext.GuildQueueItems
            .AsNoTracking()
            .AnyAsync(item => item.GuildId == guildId && item.State == QueueItemState.Queued, cancellationToken);
    }

    public async Task<QueueItemRecord?> GetNextQueuedItemAsync(ulong guildId, CancellationToken cancellationToken = default)
    {
        await using var dbContext = await dbContextFactory.CreateDbContextAsync(cancellationToken);

        var entity = await dbContext.GuildQueueItems
            .AsNoTracking()
            .Where(item => item.GuildId == guildId && item.State == QueueItemState.Queued)
            .OrderBy(item => item.Position)
            .FirstOrDefaultAsync(cancellationToken);

        return entity is null ? null : QueueItemMapper.ToRecord(entity);
    }

    public async Task<QueueItemRecord?> GetPreviousItemAsync(ulong guildId, CancellationToken cancellationToken = default)
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

    public async Task<QueueItemRecord?> GetPlayingItemByTrackIdentifierAsync(
        ulong guildId,
        string trackIdentifier,
        CancellationToken cancellationToken = default)
    {
        await using var dbContext = await dbContextFactory.CreateDbContextAsync(cancellationToken);

        var entity = await dbContext.GuildQueueItems
            .AsNoTracking()
            .Where(item => item.GuildId == guildId &&
                           item.State == QueueItemState.Playing &&
                           item.TrackIdentifier == trackIdentifier)
            .OrderByDescending(item => item.Position)
            .FirstOrDefaultAsync(cancellationToken);

        return entity is null ? null : QueueItemMapper.ToRecord(entity);
    }

    public async Task<QueueItemRecord> EnqueueAsync(
        ulong guildId,
        string trackIdentifier,
        CancellationToken cancellationToken = default)
    {
        return await EnqueueAsync(guildId, trackIdentifier, requestedBy: null, cancellationToken);
    }

    public async Task<QueueItemRecord> EnqueueAsync(
        ulong guildId,
        string trackIdentifier,
        string? requestedBy,
        CancellationToken cancellationToken = default)
    {
        return await EnqueueAsync(guildId, trackIdentifier, sourceQuery: null, sourceSearchMode: null, requestedBy,
            cancellationToken);
    }

    public async Task<QueueItemRecord> EnqueueAsync(
        ulong guildId,
        string trackIdentifier,
        string? sourceQuery,
        string? sourceSearchMode,
        string? requestedBy,
        CancellationToken cancellationToken = default)
    {
        return await PostgreSqlConcurrencyHelper.ExecuteInSerializableTransactionWithRetryAsync(
            dbContextFactory,
            (dbContext, ct) => EnqueueAsync(dbContext, guildId, trackIdentifier, sourceQuery, sourceSearchMode,
                requestedBy, ct),
            cancellationToken);
    }

    public async Task EnqueueManyAsync(
        ulong guildId,
        IReadOnlyList<string> trackIdentifiers,
        CancellationToken cancellationToken = default)
    {
        await EnqueueManyAsync(guildId, trackIdentifiers, requestedBy: null, cancellationToken);
    }

    public async Task EnqueueManyAsync(
        ulong guildId,
        IReadOnlyList<string> trackIdentifiers,
        string? requestedBy,
        CancellationToken cancellationToken = default)
    {
        await EnqueueManyAsync(guildId, trackIdentifiers, sourceQuery: null, sourceSearchMode: null, requestedBy,
            cancellationToken);
    }

    public async Task EnqueueManyAsync(
        ulong guildId,
        IReadOnlyList<string> trackIdentifiers,
        string? sourceQuery,
        string? sourceSearchMode,
        string? requestedBy,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(trackIdentifiers);

        if (trackIdentifiers.Count == 0)
        {
            return;
        }

        var queueItems = trackIdentifiers
            .Select(trackIdentifier => new QueueItemToEnqueue(
                trackIdentifier,
                sourceQuery,
                sourceSearchMode,
                requestedBy))
            .ToList();

        await EnqueueManyAsync(guildId, queueItems, cancellationToken);
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
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(trackIdentifiers);

        if (trackIdentifiers.Count > MaxQueuedItemsPerGuild)
        {
            throw new InvalidOperationException($"Queue cannot contain more than {MaxQueuedItemsPerGuild} queued tracks.");
        }

        await PostgreSqlConcurrencyHelper.ExecuteInSerializableTransactionWithRetryAsync(
            dbContextFactory,
            (dbContext, ct) => ReorderQueuedItemsAsync(dbContext, guildId, trackIdentifiers, ct),
            cancellationToken);
    }

    public Task<QueueItemRemovalRecord> RemoveQueuedItemAtAsync(
        ulong guildId,
        int trackNumber,
        CancellationToken cancellationToken = default)
    {
        return PostgreSqlConcurrencyHelper.ExecuteInSerializableTransactionWithRetryAsync(
            dbContextFactory,
            (dbContext, ct) => RemoveQueuedItemAtAsync(dbContext, guildId, trackNumber, ct),
            cancellationToken);
    }

    public Task MarkPlayingAsync(long queueItemId, CancellationToken cancellationToken = default)
    {
        return UpdateStateAsync(queueItemId, QueueItemState.Playing, setPlayedAt: false, setSkippedAt: false,
            cancellationToken);
    }

    public async Task UpdateTrackIdentifierAsync(
        long queueItemId,
        string trackIdentifier,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(trackIdentifier);

        await using var dbContext = await dbContextFactory.CreateDbContextAsync(cancellationToken);
        var entity = await dbContext.GuildQueueItems
            .FirstOrDefaultAsync(item => item.Id == queueItemId, cancellationToken);
        if (entity is null)
        {
            return;
        }

        entity.TrackIdentifier = trackIdentifier;
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task ClearSourceMetadataAsync(long queueItemId, CancellationToken cancellationToken = default)
    {
        await using var dbContext = await dbContextFactory.CreateDbContextAsync(cancellationToken);
        var entity = await dbContext.GuildQueueItems
            .FirstOrDefaultAsync(item => item.Id == queueItemId, cancellationToken);
        if (entity is null)
        {
            return;
        }

        entity.SourceQuery = null;
        entity.SourceSearchMode = null;
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public Task MarkPlayedAsync(long queueItemId, CancellationToken cancellationToken = default)
    {
        return UpdateStateAsync(queueItemId, QueueItemState.Played, setPlayedAt: true, setSkippedAt: false,
            cancellationToken);
    }

    public Task MarkSkippedAsync(long queueItemId, CancellationToken cancellationToken = default)
    {
        return UpdateStateAsync(queueItemId, QueueItemState.Skipped, setPlayedAt: false, setSkippedAt: true,
            cancellationToken);
    }

    public async Task MarkAllQueuedAsSkippedAsync(ulong guildId, CancellationToken cancellationToken = default)
    {
        await using var dbContext = await dbContextFactory.CreateDbContextAsync(cancellationToken);
        var now = DateTimeOffset.UtcNow;

        await dbContext.GuildQueueItems
            .Where(x => x.GuildId == guildId && x.State == QueueItemState.Queued)
            .ExecuteUpdateAsync(s => s
                .SetProperty(b => b.State, QueueItemState.Skipped)
                .SetProperty(b => b.SkippedAtUtc, now),
                cancellationToken);
    }
    public Task<QueueItemRecord?> ClaimNextQueuedItemAsync(ulong guildId, CancellationToken cancellationToken = default)
    {
        return _queueClaimService.ClaimNextQueuedItemAsync(guildId, cancellationToken);
    }

    private static async Task<QueueItemRecord> EnqueueAsync(
        BotDbContext dbContext,
        ulong guildId,
        string trackIdentifier,
        string? sourceQuery,
        string? sourceSearchMode,
        string? requestedBy,
        CancellationToken cancellationToken)
    {
        await GuildDataBootstrapper.EnsureExistsAsync(dbContext, guildId, cancellationToken);

        var queuedItemCount = await dbContext.GuildQueueItems
            .CountAsync(item => item.GuildId == guildId && item.State == QueueItemState.Queued, cancellationToken);

        if (queuedItemCount >= MaxQueuedItemsPerGuild)
        {
            throw new InvalidOperationException($"Queue cannot contain more than {MaxQueuedItemsPerGuild} queued tracks.");
        }

        var maxPosition = await dbContext.GuildQueueItems
            .Where(item => item.GuildId == guildId)
            .Select(item => (int?)item.Position)
            .MaxAsync(cancellationToken) ?? -1;

        var entity = new GuildQueueItemEntity
        {
            GuildId = guildId,
            Position = maxPosition + 1,
            TrackIdentifier = trackIdentifier,
            SourceQuery = NormalizeSourceQuery(sourceQuery),
            SourceSearchMode = NormalizeSourceSearchMode(sourceSearchMode),
            RequestedBy = NormalizeRequestedBy(requestedBy),
            State = QueueItemState.Queued,
            AddedAtUtc = DateTimeOffset.UtcNow
        };

        dbContext.GuildQueueItems.Add(entity);
        await dbContext.SaveChangesAsync(cancellationToken);

        return QueueItemMapper.ToRecord(entity);
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
            throw new InvalidOperationException($"Queue cannot contain more than {MaxQueuedItemsPerGuild} queued tracks.");
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
    }

    private static async Task ReorderQueuedItemsAsync(
        BotDbContext dbContext,
        ulong guildId,
        IReadOnlyList<string> trackIdentifiers,
        CancellationToken cancellationToken)
    {
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
                throw new InvalidOperationException("Reordered queue contains a track that does not match the persisted queue.");
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
    }

    private static async Task<QueueItemRemovalRecord> RemoveQueuedItemAtAsync(
        BotDbContext dbContext,
        ulong guildId,
        int trackNumber,
        CancellationToken cancellationToken)
    {
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

        return new QueueItemRemovalRecord(true, queuedEntities.Count, removedRecord);
    }

    private static async Task UpdateQueueItemPositionAsync(
        BotDbContext dbContext,
        long queueItemId,
        int newPosition,
        CancellationToken cancellationToken)
    {
        var item = await dbContext.GuildQueueItems.FirstOrDefaultAsync(q => q.Id == queueItemId, cancellationToken);
        if (item is null)
        {
            return;
        }

        var positionOccupied = await dbContext.GuildQueueItems.AnyAsync(
            q => q.GuildId == item.GuildId && q.Position == newPosition && q.Id != queueItemId,
            cancellationToken);
        if (positionOccupied)
        {
            throw new InvalidOperationException("Queue position is already occupied.");
        }

        item.Position = newPosition;
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    private async Task UpdateStateAsync(
        long queueItemId,
        QueueItemState state,
        bool setPlayedAt,
        bool setSkippedAt,
        CancellationToken cancellationToken)
    {
        await using var dbContext = await dbContextFactory.CreateDbContextAsync(cancellationToken);

        var entity = await dbContext.GuildQueueItems.FirstOrDefaultAsync(item => item.Id == queueItemId, cancellationToken);
        if (entity is null)
        {
            return;
        }

        entity.State = state;
        if (state == QueueItemState.Playing)
        {
            entity.PlayedAtUtc = null;
            entity.SkippedAtUtc = null;
        }

        if (setPlayedAt)
        {
            entity.PlayedAtUtc = DateTimeOffset.UtcNow;
        }

        if (setSkippedAt)
        {
            entity.SkippedAtUtc = DateTimeOffset.UtcNow;
        }

        await dbContext.SaveChangesAsync(cancellationToken);
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

    public async Task UpdateQueueItemPositionAsync(long queueItemId, int newPosition, CancellationToken cancellationToken = default)
    {
        await PostgreSqlConcurrencyHelper.ExecuteInSerializableTransactionWithRetryAsync(
            dbContextFactory,
            (dbContext, ct) => UpdateQueueItemPositionAsync(dbContext, queueItemId, newPosition, ct),
            cancellationToken);
    }
}
