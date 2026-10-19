namespace DC_bot.Interface.Service.Persistence.Models.Queue;

public sealed record QueueClaimResult(
    QueueClaimStatus Status,
    QueueStartClaim? Claim = null);
