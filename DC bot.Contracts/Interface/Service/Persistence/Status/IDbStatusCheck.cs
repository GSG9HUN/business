namespace DC_bot.Interface.Service.Persistence.Status;

/// <summary>
/// Defines shared storage operations for db status check, including caller cancellation and immutable result contracts.
/// </summary>
public interface IDbStatusCheck
{

    /// <summary>
    /// Checks database connectivity; requested cancellation propagates while ordinary connectivity failures return false.
    /// </summary>
    /// <param name = "cancellationToken">Cancels supported database work and retry backoff; owned rollback uses its own deadline.</param>
    /// <returns>True when the documented check or conditional mutation succeeds; otherwise false.</returns>
    Task<bool> CanConnectAsync(CancellationToken cancellationToken = default);
}
