using DC_bot.BotControl;
using DC_bot.Interface.Service.BotControl.Models;
using DC_bot.Interface.Service.Persistence.Models.BotControlCommand;

namespace DC_bot.Interface.Service.BotControl;

/// <summary>
/// Defines the bot control context resolver boundary so callers are independent of its concrete implementation.
/// </summary>
public interface IBotControlContextResolver
{

    /// <summary>
    /// Resolves the command&apos;s guild, member, channels, and notification permissions before mobile dispatch.
    /// </summary>
    /// <param name="command">The command supplied for this operation.</param>
    /// <param name="channelContext">The channel context supplied for this operation.</param>
    /// <param name="cancellationToken">Cancels waiting and supported downstream I/O; owned cleanup completes independently.</param>
    /// <returns>The asynchronous operation result, including its status or value when completed.</returns>
    Task<BotControlExecutionContext> ResolveAsync(
        BotControlCommandRecord command,
        BotControlCommandChannelContext? channelContext,
        CancellationToken cancellationToken = default);
}
