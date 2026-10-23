namespace DC_bot.Interface.Service.Persistence.Models.Queue;

/// <summary>
/// Carries immutable pending playback start values across the shared storage or mobile-command boundary.
/// </summary>
/// <param name = "GuildId">The Discord guild whose persisted state is addressed.</param>
/// <param name = "QueueItemId">The durable queue-item identifier, when a specific item is required.</param>
/// <param name = "OperationId">The nonempty playback operation identifier used to fence stale or duplicate state transitions.</param>
/// <param name = "NodeAddress">The Lavalink node address bound to the pending operation before dispatch.</param>
/// <param name = "NodeSessionId">The Lavalink node-session identity bound to the pending operation.</param>
/// <param name = "TransportConfirmed">The transport confirmed value used by this storage contract.</param>
public sealed record PendingPlaybackStart(
    ulong GuildId,
    long QueueItemId,
    Guid OperationId,
    string? NodeAddress,
    string? NodeSessionId,
    bool TransportConfirmed);
