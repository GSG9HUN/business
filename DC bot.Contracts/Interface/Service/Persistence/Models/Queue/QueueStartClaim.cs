namespace DC_bot.Interface.Service.Persistence.Models.Queue;

/// <summary>
/// Carries immutable queue start claim values across the shared storage or mobile-command boundary.
/// </summary>
/// <param name = "OperationId">The nonempty playback operation identifier used to fence stale or duplicate state transitions.</param>
/// <param name = "Item">The item value used by this storage contract.</param>
public sealed record QueueStartClaim(Guid OperationId, QueueItemRecord Item);
