using DC_bot.Interface.Service.BotControl.Models;
using DC_bot.Interface.Service.Persistence.Models.BotControlCommand;

namespace DC_bot.Interface.Service.BotControl;

/// <summary>
/// Defines the bot control command dispatcher boundary so callers are independent of its concrete implementation.
/// </summary>
public interface IBotControlCommandDispatcher
{

    /// <summary>
    /// Dispatches the  operation to its resolved service handler.
    /// </summary>
    /// <param name="command">The command supplied for this operation.</param>
    /// <param name="cancellationToken">Cancels waiting and supported downstream I/O; owned cleanup completes independently.</param>
    /// <returns>The asynchronous operation result, including its status or value when completed.</returns>
    Task<BotControlCommandResult> DispatchAsync(BotControlCommandRecord command, CancellationToken cancellationToken = default);
}
