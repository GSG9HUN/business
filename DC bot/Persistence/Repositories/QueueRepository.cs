using DC_bot.Interface.Service.Persistence;
using DC_bot.Interface.Service.Persistence.Models;
using DC_bot.Persistence.Db;
using DC_bot.Persistence.Entities;
using Microsoft.EntityFrameworkCore;


namespace DC_bot.Persistence.Repositories;

public class QueueRepository(IDbContextFactory<BotDbContext> dbContextFactory) : IQueueRepository
{
    private const int MaxQueuedItemsPerGuild = 50;

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

        return entities.Select(MapToRecord).ToList();
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

        return entity is null ? null : MapToRecord(entity);
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

        return entity is null ? null : MapToRecord(entity);
    }

    public async Task<QueueItemRecord> EnqueueAsync(
        ulong guildId,
        string trackIdentifier,
        CancellationToken cancellationToken = default)
    {
        await using var dbContext = await dbContextFactory.CreateDbContextAsync(cancellationToken);

        await EnsureGuildDataExistsAsync(dbContext, guildId, cancellationToken);

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
            State = QueueItemState.Queued,
            AddedAtUtc = DateTimeOffset.UtcNow
        };

        dbContext.GuildQueueItems.Add(entity);
        await dbContext.SaveChangesAsync(cancellationToken);

        return MapToRecord(entity);
    }

    public async Task EnqueueManyAsync(
        ulong guildId,
        IReadOnlyList<string> trackIdentifiers,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(trackIdentifiers);

        if (trackIdentifiers.Count == 0)
        {
            return;
        }

        await using var dbContext = await dbContextFactory.CreateDbContextAsync(cancellationToken);

        await EnsureGuildDataExistsAsync(dbContext, guildId, cancellationToken);

        var queuedItemCount = await dbContext.GuildQueueItems
            .CountAsync(item => item.GuildId == guildId && item.State == QueueItemState.Queued, cancellationToken);

        if (queuedItemCount + trackIdentifiers.Count > MaxQueuedItemsPerGuild)
        {
            throw new InvalidOperationException($"Queue cannot contain more than {MaxQueuedItemsPerGuild} queued tracks.");
        }

        var maxPosition = await dbContext.GuildQueueItems
            .Where(item => item.GuildId == guildId)
            .Select(item => (int?)item.Position)
            .MaxAsync(cancellationToken) ?? -1;

        var addedAtUtc = DateTimeOffset.UtcNow;
        var entities = new List<GuildQueueItemEntity>(trackIdentifiers.Count);
        for (var index = 0; index < trackIdentifiers.Count; index++)
        {
            entities.Add(new GuildQueueItemEntity
            {
                GuildId = guildId,
                Position = maxPosition + index + 1,
                TrackIdentifier = trackIdentifiers[index],
                State = QueueItemState.Queued,
                AddedAtUtc = addedAtUtc
            });
        }

        dbContext.GuildQueueItems.AddRange(entities);
        await dbContext.SaveChangesAsync(cancellationToken);
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

        await using var dbContext = await dbContextFactory.CreateDbContextAsync(cancellationToken);

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

        await using var transaction = await dbContext.Database.BeginTransactionAsync(cancellationToken);

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
        await transaction.CommitAsync(cancellationToken);
    }

    public Task MarkPlayingAsync(long queueItemId, CancellationToken cancellationToken = default)
    {
        return UpdateStateAsync(queueItemId, QueueItemState.Playing, setPlayedAt: false, setSkippedAt: false,
            cancellationToken);
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
    public async Task<QueueItemRecord?> ClaimNextQueuedItemAsync(ulong guildId, CancellationToken cancellationToken = default)
    {
        await using var dbContext = await dbContextFactory.CreateDbContextAsync(cancellationToken);

        await using var transaction = await dbContext.Database.BeginTransactionAsync(cancellationToken);
        try
        {
            var entity = await dbContext.GuildQueueItems
                .FromSqlInterpolated(PostgreSqlQueueClaimSql.ClaimNextQueuedItem(guildId))
                .FirstOrDefaultAsync(cancellationToken);

            if (entity == null) return null;

            entity.State = QueueItemState.Playing;
            await dbContext.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);

            return MapToRecord(entity);
        }
        catch
        {
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }
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

    private static QueueItemRecord MapToRecord(GuildQueueItemEntity entity)
    {
        return new QueueItemRecord(
            entity.Id,
            entity.GuildId,
            entity.Position,
            entity.TrackIdentifier,
            entity.State,
            entity.AddedAtUtc,
            entity.PlayedAtUtc,
            entity.SkippedAtUtc);
    }

    private static async Task EnsureGuildDataExistsAsync(
        BotDbContext dbContext,
        ulong guildId,
        CancellationToken cancellationToken)
    {
        var exists = await dbContext.GuildData.AnyAsync(g => g.GuildId == guildId, cancellationToken);
        if (exists)
        {
            return;
        }

        dbContext.GuildData.Add(new GuildDataEntity
        {
            GuildId = guildId,
            IsPremium = false,
            UpdatedAtUtc = DateTimeOffset.UtcNow
        });

        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateQueueItemPositionAsync(long queueItemId, int newPosition, CancellationToken cancellationToken = default)
    {
        await using var dbContext = await dbContextFactory.CreateDbContextAsync(cancellationToken);
        var item = await dbContext.GuildQueueItems.FirstOrDefaultAsync(q => q.Id == queueItemId, cancellationToken);
        if (item is null)
        {
            return;
        }
        item.Position = newPosition;
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
