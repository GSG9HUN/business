using DC_bot.Interface.Service.Persistence.Models.BotControlCommand;

namespace DC_bot.Interface.Service.Persistence.BotControl;

/// <summary>
/// Defines shared storage operations for bot control commands repository, including caller cancellation and immutable result contracts.
/// </summary>
public interface IBotControlCommandsRepository
{

    /// <summary>
    /// Reads the durable command and its outcome by command identifier, returning null when it does not exist.
    /// </summary>
    /// <param name = "commandId">The command id value used by this storage contract.</param>
    /// <param name = "cancellationToken">Cancels supported database work and retry backoff; owned rollback uses its own deadline.</param>
    /// <returns>The matching value, or null when no value satisfies the documented lookup.</returns>
    Task<BotControlCommandRecord?> GetByCommandIdAsync(string commandId, CancellationToken cancellationToken);

    /// <summary>
    /// Persists a pending mobile command and publishes its worker and realtime notifications within the same transaction.
    /// </summary>
    /// <param name = "guildId">The Discord guild whose persisted state is addressed.</param>
    /// <param name = "userId">The user id value used by this storage contract.</param>
    /// <param name = "type">The type value used by this storage contract.</param>
    /// <param name = "cancellationToken">Cancels supported database work and retry backoff; owned rollback uses its own deadline.</param>
    /// <returns>A task yielding the persisted or mapped result described above.</returns>
    Task<BotControlCommandRecord> EnqueueAsync(ulong guildId, ulong userId, string type, CancellationToken cancellationToken);

    /// <summary>
    /// Persists a pending mobile command and publishes its worker and realtime notifications within the same transaction.
    /// </summary>
    /// <param name = "guildId">The Discord guild whose persisted state is addressed.</param>
    /// <param name = "userId">The user id value used by this storage contract.</param>
    /// <param name = "type">The type value used by this storage contract.</param>
    /// <param name = "payloadJson">The payload json value used by this storage contract.</param>
    /// <param name = "cancellationToken">Cancels supported database work and retry backoff; owned rollback uses its own deadline.</param>
    /// <returns>A task yielding the persisted or mapped result described above.</returns>
    Task<BotControlCommandRecord> EnqueueAsync(
        ulong guildId,
        ulong userId,
        string type,
        string? payloadJson,
        CancellationToken cancellationToken);

    /// <summary>
    /// Atomically claims the oldest eligible pending command so competing workers cannot execute the same claim.
    /// </summary>
    /// <param name = "ct">Cancels supported database work and retry backoff; owned rollback uses its own deadline.</param>
    /// <returns>The matching value, or null when no value satisfies the documented lookup.</returns>
    Task<BotControlCommandRecord?> ClaimNextPendingAsync(CancellationToken ct);

    /// <summary>
    /// Lists commands left in the started state for uncertain-outcome recovery rather than automatic replay.
    /// </summary>
    /// <param name = "ct">Cancels supported database work and retry backoff; owned rollback uses its own deadline.</param>
    /// <returns>The matching records in the documented order, or an empty collection when none match.</returns>
    Task<IReadOnlyList<BotControlCommandRecord>> GetStartedAsync(CancellationToken ct);

    /// <summary>
    /// Persists a successful command result and emits its command-state notification.
    /// </summary>
    /// <param name = "commandId">The command id value used by this storage contract.</param>
    /// <param name = "resultJson">The result json value used by this storage contract.</param>
    /// <param name = "ct">Cancels supported database work and retry backoff; owned rollback uses its own deadline.</param>
    /// <returns>A task representing completion of the documented operation.</returns>
    Task MarkDoneAsync(string commandId, string? resultJson, CancellationToken ct);

    /// <summary>
    /// Persists the command failure message and optional structured result, then emits its state notification.
    /// </summary>
    /// <param name = "commandId">The command id value used by this storage contract.</param>
    /// <param name = "errorMessage">The error message value used by this storage contract.</param>
    /// <param name = "resultJson">The result json value used by this storage contract.</param>
    /// <param name = "ct">Cancels supported database work and retry backoff; owned rollback uses its own deadline.</param>
    /// <returns>A task representing completion of the documented operation.</returns>
    Task MarkFailedAsync(string commandId, string errorMessage, string? resultJson, CancellationToken ct);
}
