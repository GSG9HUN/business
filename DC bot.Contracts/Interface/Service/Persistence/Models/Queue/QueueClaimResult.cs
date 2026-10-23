namespace DC_bot.Interface.Service.Persistence.Models.Queue;

/// <summary>
/// Carries immutable queue claim result values across the shared storage or mobile-command boundary.
/// </summary>
/// <param name = "Status">The status value used by this storage contract.</param>
/// <param name = "Claim">The claim value used by this storage contract.</param>
public sealed record QueueClaimResult(QueueClaimStatus Status, QueueStartClaim? Claim = null);
