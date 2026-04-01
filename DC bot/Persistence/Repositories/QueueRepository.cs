using DC_bot.Interface.Service.Persistence;
using DC_bot.Interface.Service.Persistence.Models;
using DC_bot.Persistence.Db;
using DC_bot.Persistence.Entities;
using Microsoft.EntityFrameworkCore;

namespace DC_bot.Persistence.Repositories;

public class QueueRepository(IDbContextFactory<BotDbContext> dbContextFactory) : IQueueRepository
{
    private const short Queued = 0;
    private const short Playing = 1;
    private const short Played = 2;
    private const short Skipped = 3;

    public async Task<IReadOnlyList<QueueItemRecord>> GetQueuedItemsAsync(
        ulong guildId,
        CancellationToken cancellationToken = default)
    {
        await using var dbContext = await dbContextFactory.CreateDbContextAsync(cancellationToken);
        var id = ToDbGuildId(guildId);

        var entities = await dbContext.GuildQueueItems
            .AsNoTracking()
            .Where(item => item.GuildId == id && item.State == Queued)
            .OrderBy(item => item.Position)
            .ToListAsync(cancellationToken);

        return entities.Select(MapToRecord).ToList();
    }

    public async Task<QueueItemRecord?> GetNextQueuedItemAsync(ulong guildId, CancellationToken cancellationToken = default)
    {
        await using var dbContext = await dbContextFactory.CreateDbContextAsync(cancellationToken);
        var id = ToDbGuildId(guildId);

        var entity = await dbContext.GuildQueueItems
            .AsNoTracking()
            .Where(item => item.GuildId == id && item.State == Queued)
            .OrderBy(item => item.Position)
            .FirstOrDefaultAsync(cancellationToken);

        return entity is null ? null : MapToRecord(entity);
    }

    public async Task<QueueItemRecord?> GetPreviousItemAsync(ulong guildId, CancellationToken cancellationToken = default)
    {
        await using var dbContext = await dbContextFactory.CreateDbContextAsync(cancellationToken);
        var id = ToDbGuildId(guildId);

        var entity = await dbContext.GuildQueueItems
            .AsNoTracking()
            .Where(item => item.GuildId == id && (item.State == Played || item.State == Skipped))
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
        var id = ToDbGuildId(guildId);

        await EnsureGuildDataExistsAsync(dbContext, id, cancellationToken);

        var maxPosition = await dbContext.GuildQueueItems
            .Where(item => item.GuildId == id)
            .Select(item => (int?)item.Position)
            .MaxAsync(cancellationToken) ?? -1;

        var entity = new GuildQueueItemEntity
        {
            GuildId = id,
            Position = maxPosition + 1,
            TrackIdentifier = trackIdentifier,
            State = Queued,
            AddedAtUtc = DateTimeOffset.UtcNow
        };

        dbContext.GuildQueueItems.Add(entity);
        await dbContext.SaveChangesAsync(cancellationToken);

        return MapToRecord(entity);
    }

    public Task MarkPlayingAsync(long queueItemId, CancellationToken cancellationToken = default)
    {
        return UpdateStateAsync(queueItemId, Playing, setPlayedAt: false, setSkippedAt: false, cancellationToken);
    }

    public Task MarkPlayedAsync(long queueItemId, CancellationToken cancellationToken = default)
    {
        return UpdateStateAsync(queueItemId, Played, setPlayedAt: true, setSkippedAt: false, cancellationToken);
    }

    public Task MarkSkippedAsync(long queueItemId, CancellationToken cancellationToken = default)
    {
        return UpdateStateAsync(queueItemId, Skipped, setPlayedAt: false, setSkippedAt: true, cancellationToken);
    }

    private async Task UpdateStateAsync(
        long queueItemId,
        short state,
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
            ToDomainGuildId(entity.GuildId),
            entity.Position,
            entity.TrackIdentifier,
            entity.State,
            entity.AddedAtUtc,
            entity.PlayedAtUtc,
            entity.SkippedAtUtc);
    }

    private static async Task EnsureGuildDataExistsAsync(
        BotDbContext dbContext,
        long guildId,
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

    private static long ToDbGuildId(ulong guildId)
    {
        return checked((long)guildId);
    }

    private static ulong ToDomainGuildId(long guildId)
    {
        return checked((ulong)guildId);
    }
}
