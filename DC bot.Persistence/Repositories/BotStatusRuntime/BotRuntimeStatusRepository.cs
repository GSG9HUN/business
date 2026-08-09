using DC_bot.Db;
using DC_bot.Interface.Service.Persistence.BotRuntimeStatus;
using DC_bot.Interface.Service.Persistence.Models.BotRuntimeStatus;
using Microsoft.EntityFrameworkCore;

namespace DC_bot.Repositories.BotStatusRuntime;

public class BotStatusRuntimeRepository(IDbContextFactory<BotDbContext> dbContextFactory): IBotRuntimeStatusRepository
{
    public Task UpsertHeartbeatAsync(DateTimeOffset heartbeatAtUtc, CancellationToken ct = default)
    {
        throw new NotImplementedException();
    }

    public Task<BotRuntimeStatusRecord?> GetCurrentAsync(CancellationToken ct = default)
    {
        using var db = dbContextFactory.CreateDbContext();
        try
        {
           /* var record = db.BotStatusRuntime
                .OrderByDescending(x => x.Id)
                .Select(x => new BotRuntimeStatusRecord(x.Id, x.LastHeartbeatAtUtc, x.StartedAtUtc))
                .FirstOrDefault();
            return Task.FromResult(record);*/
        }
        catch (Exception)
        {
            throw;
        }
    }
}