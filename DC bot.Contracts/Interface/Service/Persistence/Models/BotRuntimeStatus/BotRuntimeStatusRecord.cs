namespace DC_bot.Interface.Service.Persistence.Models.BotRuntimeStatus;

/// <summary>
/// Carries immutable bot runtime status values across the shared storage or mobile-command boundary.
/// </summary>
/// <param name = "Id">The id value used by this storage contract.</param>
/// <param name = "LastHeartbeatAtUtc">The last heartbeat at utc value used by this storage contract.</param>
/// <param name = "StartedAtUtc">The started at utc value used by this storage contract.</param>
public record BotRuntimeStatusRecord(int Id, DateTimeOffset LastHeartbeatAtUtc, DateTimeOffset StartedAtUtc);
