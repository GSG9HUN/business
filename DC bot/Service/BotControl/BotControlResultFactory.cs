using System.Text.Json;
using DC_bot.Interface.Service.BotControl;
using DC_bot.Interface.Service.BotControl.Models;
using DC_bot.Interface.Service.Persistence.Models.BotControlCommand;

namespace DC_bot.Service.BotControl;

public class BotControlResultFactory : IBotControlResultFactory
{
    public BotControlCommandResult Success(
        BotControlCommandRecord command,
        string message,
        object? data = null,
        ulong? voiceChannelId = null,
        ulong? textChannelId = null,
        bool shouldNotifyDiscord = true)
    {
        var resultJson = SerializeResult(
            command,
            success: true,
            message,
            errorCode: null,
            data,
            voiceChannelId,
            textChannelId,
            discordNotificationSent: false);

        return new BotControlCommandResult(
            true,
            message,
            resultJson,
            command.GuildId,
            command.UserId,
            voiceChannelId,
            textChannelId,
            shouldNotifyDiscord);
    }

    public BotControlCommandResult Failure(
        BotControlCommandRecord command,
        string message,
        string errorCode,
        object? data = null,
        ulong? voiceChannelId = null,
        ulong? textChannelId = null,
        bool shouldNotifyDiscord = true)
    {
        var resultJson = SerializeResult(
            command,
            success: false,
            message,
            errorCode,
            data,
            voiceChannelId,
            textChannelId,
            discordNotificationSent: false);

        return new BotControlCommandResult(
            false,
            message,
            resultJson,
            command.GuildId,
            command.UserId,
            voiceChannelId,
            textChannelId,
            shouldNotifyDiscord);
    }

    private static string SerializeResult(
        BotControlCommandRecord command,
        bool success,
        string message,
        string? errorCode,
        object? data,
        ulong? voiceChannelId,
        ulong? textChannelId,
        bool discordNotificationSent)
    {
        return JsonSerializer.Serialize(new
        {
            success,
            commandId = command.CommandId,
            type = command.Type,
            message,
            guildId = command.GuildId.ToString(),
            userId = command.UserId.ToString(),
            voiceChannelId = voiceChannelId?.ToString(),
            textChannelId = textChannelId?.ToString(),
            discordNotificationSent,
            errorCode,
            data
        });
    }
}
