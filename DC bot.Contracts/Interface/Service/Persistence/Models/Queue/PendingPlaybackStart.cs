namespace DC_bot.Interface.Service.Persistence.Models.Queue;

public sealed record PendingPlaybackStart(
    ulong GuildId,
    long QueueItemId,
    Guid OperationId,
    string? NodeAddress,
    string? NodeSessionId,
    bool TransportConfirmed);