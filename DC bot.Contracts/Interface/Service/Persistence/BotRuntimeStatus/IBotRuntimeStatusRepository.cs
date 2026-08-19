using DC_bot.Interface.Service.Persistence.Models.BotRuntimeStatus;

namespace DC_bot.Interface.Service.Persistence.BotRuntimeStatus;

public interface IBotRuntimeStatusRepository
{
    Task UpsertHeartbeatAsync(DateTimeOffset heartbeatAtUtc, CancellationToken ct = default);
    Task<BotRuntimeStatusRecord?> GetCurrentAsync(CancellationToken ct = default);
}