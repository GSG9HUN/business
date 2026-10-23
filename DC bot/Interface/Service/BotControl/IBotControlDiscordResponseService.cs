using DC_bot.Interface.Service.BotControl.Models;

namespace DC_bot.Interface.Service.BotControl;

/// <summary>
/// Defines the bot control discord response service boundary so callers are independent of its concrete implementation.
/// </summary>
public interface IBotControlDiscordResponseService
{

    /// <summary>
    /// Attempts Discord response delivery and reports whether delivery succeeded.
    /// </summary>
    /// <param name="result">The result supplied for this operation.</param>
    /// <param name="cancellationToken">Cancels waiting and supported downstream I/O; owned cleanup completes independently.</param>
    /// <returns>A task yielding whether the requested check or operation succeeded.</returns>
    Task<bool> TrySendAsync(BotControlCommandResult result, CancellationToken cancellationToken = default);
}
