using DC_bot.Db;
using DC_bot.Entities.BotRuntimeStatus;
using DC_bot.Interface.Service.Persistence.BotRuntimeStatus;
using DC_bot.Interface.Service.Persistence.Models.BotRuntimeStatus;
using DC_bot.Repositories.Shared;
using Microsoft.EntityFrameworkCore;

namespace DC_bot.Repositories.BotRuntimeStatus;

public class BotRuntimeStatusRepository(IDbContextFactory<BotDbContext> dbContextFactory) : IBotRuntimeStatusRepository
{
    private const int Id = 1;
    public async Task UpsertHeartbeatAsync(DateTimeOffset heartbeatAtUtc, CancellationToken cancellationToken = default)
    {
        await using var db = await dbContextFactory.CreateDbContextAsync(cancellationToken);

        var entity = await db.BotRuntimeStatus.FirstOrDefaultAsync(x => x.Id == Id, cancellationToken);
        if (entity is null)
        {
            entity = new BotRuntimeStatusEntity
            {
                Id = Id,
                LastHeartbeatAtUtc = heartbeatAtUtc,
                StartedAtUtc = DateTimeOffset.UtcNow
            };
            db.BotRuntimeStatus.Add(entity);

            var inserted = await PostgreSqlConcurrencyHelper.SaveChangesIgnoringUniqueViolationAsync(
                db,
                cancellationToken);
            if (inserted)
            {
                return;
            }

            entity = await db.BotRuntimeStatus.FirstAsync(x => x.Id == Id, cancellationToken);
        }

        entity.LastHeartbeatAtUtc = heartbeatAtUtc;
        await db.SaveChangesAsync(cancellationToken);
    }

    public async Task<BotRuntimeStatusRecord?> GetCurrentAsync(CancellationToken cancellationToken = default)
    {
        await using var db = await dbContextFactory.CreateDbContextAsync(cancellationToken);
        
        var record = await db.BotRuntimeStatus
            .OrderByDescending(x => x.Id)
            .Select(x => new BotRuntimeStatusRecord(x.Id, x.LastHeartbeatAtUtc, x.StartedAtUtc))
            .FirstOrDefaultAsync(cancellationToken);
        return record;
    }
}
