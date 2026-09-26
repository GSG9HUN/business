using DC_bot.Interface.Service.Persistence.Models.BotControlCommand;

namespace DC_bot.Interface.Service.Persistence.BotControl;

public interface IBotControlCommandsRepository
{
    Task<BotControlCommandRecord?> GetByCommandIdAsync(
        string commandId,
        CancellationToken cancellationToken);

    Task<BotControlCommandRecord> EnqueueAsync(
        ulong guildId,
        ulong userId,
        string type,
        CancellationToken cancellationToken);

    Task<BotControlCommandRecord> EnqueueAsync(
        ulong guildId,
        ulong userId,
        string type,
        string? payloadJson,
        CancellationToken cancellationToken);
    
    Task<BotControlCommandRecord?> ClaimNextPendingAsync(CancellationToken ct);
    Task<IReadOnlyList<BotControlCommandRecord>> GetStartedAsync(CancellationToken ct);
    Task MarkDoneAsync(string commandId, string? resultJson, CancellationToken ct);
    Task MarkFailedAsync(string commandId, string errorMessage, string? resultJson, CancellationToken ct);
}
