using DC_bot.Interface.Service.Persistence.Models.BotRuntimeStatus;

namespace DC_bot.Interface.Service.Persistence.BotRuntimeStatus;

/// <summary>
/// Defines shared storage operations for bot runtime status repository, including caller cancellation and immutable result contracts.
/// </summary>
public interface IBotRuntimeStatusRepository
{

    /// <summary>
    /// Persists the bot instance&apos;s runtime heartbeat and liveness metadata.
    /// </summary>
    /// <param name = "heartbeatAtUtc">The heartbeat at utc value used by this storage contract.</param>
    /// <param name = "ct">Cancels supported database work and retry backoff; owned rollback uses its own deadline.</param>
    /// <returns>A task representing completion of the documented operation.</returns>
    Task UpsertHeartbeatAsync(DateTimeOffset heartbeatAtUtc, CancellationToken ct = default);

    /// <summary>
    /// Reads the current persisted bot-runtime status when available.
    /// </summary>
    /// <param name = "ct">Cancels supported database work and retry backoff; owned rollback uses its own deadline.</param>
    /// <returns>The matching value, or null when no value satisfies the documented lookup.</returns>
    Task<BotRuntimeStatusRecord?> GetCurrentAsync(CancellationToken ct = default);
}
