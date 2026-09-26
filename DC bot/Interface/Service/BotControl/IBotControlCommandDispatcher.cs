using DC_bot.Interface.Service.BotControl.Models;
using DC_bot.Interface.Service.Persistence.Models.BotControlCommand;

namespace DC_bot.Interface.Service.BotControl;

public interface IBotControlCommandDispatcher
{
    Task<BotControlCommandResult> DispatchAsync(
        BotControlCommandRecord command,
        CancellationToken cancellationToken);
}