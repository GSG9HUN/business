using DC_bot.Interface.Service.BotControl.Models;

namespace DC_bot.Interface.Service.BotControl;

public interface IBotControlDiscordResponseService
{
    Task<bool> TrySendAsync(
        BotControlCommandResult result,
        CancellationToken cancellationToken);
}