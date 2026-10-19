namespace DC_bot.Interface.Service.Persistence.Models.Queue;

public sealed record QueueItemRemovalRecord(
    bool Success,
    int QueueSize,
    QueueItemRecord? RemovedItem);
