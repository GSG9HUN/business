namespace DC_bot.Interface.Service.Music.Lavalink;

/// <summary>
/// Defines the lavalink node connection service boundary so callers are independent of its concrete implementation.
/// </summary>
public interface ILavalinkNodeConnectionService
{

    /// <summary>
    /// Connects the configured Discord or Lavalink runtime boundary and propagates connection failures.
    /// </summary>
    /// <param name="cancellationToken">Cancels waiting and supported downstream I/O; owned cleanup completes independently.</param>
    /// <returns>A task representing completion of this operation.</returns>
    Task ConnectAsync(CancellationToken cancellationToken = default);
}
