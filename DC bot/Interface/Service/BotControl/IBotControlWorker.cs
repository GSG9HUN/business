namespace DC_bot.Interface.Service.BotControl;

/// <summary>
/// Defines the bot control worker boundary so callers are independent of its concrete implementation.
/// </summary>
public interface IBotControlWorker
{

    /// <summary>
    /// Processes durable mobile commands and their notification lifecycle until shutdown is requested.
    /// </summary>
    /// <param name="cancellationToken">Cancels waiting and supported downstream I/O; owned cleanup completes independently.</param>
    /// <returns>A task representing completion of this operation.</returns>
    Task RunAsync(CancellationToken cancellationToken = default);
}
