using DC_bot.Interface.Service.BotControl.Models;
using DC_bot.Interface.Service.Persistence.Models.BotControlCommand;

namespace DC_bot.Interface.Service.BotControl;

public interface IBotControlResultFactory
{
    BotControlCommandResult Success(
        BotControlCommandRecord command,
        string message,
        object? data = null,
        ulong? voiceChannelId = null,
        ulong? textChannelId = null,
        bool shouldNotifyDiscord = true);

    BotControlCommandResult Failure(
        BotControlCommandRecord command,
        string message,
        string errorCode,
        object? data = null,
        ulong? voiceChannelId = null,
        ulong? textChannelId = null,
        bool shouldNotifyDiscord = true);
}