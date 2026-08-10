using DC_bot.Interface.Service.Persistence.Models.BotControlCommand;

namespace DC_bot.Interface.Service.Persistence.BotControl;

public interface IBotControlCommandsRepository
{
    Task<BotControlCommandRecord> EnqueueAsync(
        ulong guildId,
        ulong userId,
        string type,
        CancellationToken cancellationToken);
    
    Task<BotControlCommandRecord?> ClaimNextPendingAsync(CancellationToken ct);
    Task MarkDoneAsync(string commandId, CancellationToken ct);
    Task MarkFailedAsync(string commandId, string errorMessage, CancellationToken ct);
}