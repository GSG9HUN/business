using DC_bot.BotControl;
using DC_bot.Interface.Service.BotControl.Models;
using DC_bot.Interface.Service.Persistence.Models.BotControlCommand;

namespace DC_bot.Interface.Service.BotControl;

public interface IBotControlContextResolver
{
    Task<BotControlExecutionContext> ResolveAsync(
        BotControlCommandRecord command,
        BotControlCommandChannelContext? channelContext,
        CancellationToken cancellationToken);
}