namespace DC_bot.Interface.Service.Persistence.Models.Queue;

/// <summary>
/// Carries immutable queue item removal values across the shared storage or mobile-command boundary.
/// </summary>
/// <param name = "Success">The success value used by this storage contract.</param>
/// <param name = "QueueSize">The queue size value used by this storage contract.</param>
/// <param name = "RemovedItem">The removed item value used by this storage contract.</param>
public sealed record QueueItemRemovalRecord(bool Success, int QueueSize, QueueItemRecord? RemovedItem);
