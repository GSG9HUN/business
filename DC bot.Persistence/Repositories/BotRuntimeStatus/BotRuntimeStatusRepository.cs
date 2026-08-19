using DC_bot.Db;
using DC_bot.Entities.BotRuntimeStatus;
using DC_bot.Interface.Service.Persistence.BotRuntimeStatus;
using DC_bot.Interface.Service.Persistence.Models.BotRuntimeStatus;
using Microsoft.EntityFrameworkCore;

namespace DC_bot.Repositories.BotRuntimeStatus;

public class BotRuntimeStatusRepository(IDbContextFactory<BotDbContext> dbContextFactory) : IBotRuntimeStatusRepository
{
    private const int Id = 1;
    public async Task UpsertHeartbeatAsync(DateTimeOffset heartbeatAtUtc, CancellationToken cancellationToken = default)
    {
        await using var db = await dbContextFactory.CreateDbContextAsync(cancellationToken);
        var transaction = await db.Database.BeginTransactionAsync(cancellationToken);
        try
        {
            
            var entity = await db.BotRuntimeStatus.FirstOrDefaultAsync(x => x.Id == Id, cancellationToken);
            
            if (entity is not null)
            {
                entity.LastHeartbeatAtUtc = heartbeatAtUtc;
                await db.SaveChangesAsync(cancellationToken);
                await transaction.CommitAsync(cancellationToken);
                return;
            }
            
            db.BotRuntimeStatus.Add(new BotRuntimeStatusEntity
            {
                Id = Id,
                LastHeartbeatAtUtc = heartbeatAtUtc,
                StartedAtUtc = DateTimeOffset.UtcNow
            });
            
            await db.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
        }
        catch (Exception)
        {
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }
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