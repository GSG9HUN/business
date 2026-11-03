namespace DC_bot.Interface.Service.Persistence.BotControl;

/// <summary>
/// Defines shared storage operations for bot control command notifier, including caller cancellation and immutable result contracts.
/// </summary>
public interface IBotControlCommandNotifier
{

    /// <summary>
    /// Opens the dedicated PostgreSQL connection and installs LISTEN when needed.
    /// </summary>
    /// <param name = "cancellationToken">Cancels supported database work and retry backoff; owned rollback uses its own deadline.</param>
    /// <returns>A task representing completion of the documented operation.</returns>
    Task EnsureListeningAsync(CancellationToken cancellationToken);

    /// <summary>
    /// Waits for a command-channel notification on the listening connection until caller cancellation.
    /// </summary>
    /// <param name = "cancellationToken">Cancels supported database work and retry backoff; owned rollback uses its own deadline.</param>
    /// <returns>A task representing completion of the documented operation.</returns>
    Task WaitForCommandAsync(CancellationToken cancellationToken);
}
