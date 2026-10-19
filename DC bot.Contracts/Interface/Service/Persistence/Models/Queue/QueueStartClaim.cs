namespace DC_bot.Interface.Service.Persistence.Models.Queue;

public sealed record QueueStartClaim(Guid OperationId, QueueItemRecord Item);