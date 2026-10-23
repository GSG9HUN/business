using System.Text.Json;
using DC_bot.Interface.Service.BotControl;
using DC_bot.Interface.Service.BotControl.Models;
using DC_bot.Interface.Service.Persistence.Models.BotControlCommand;

namespace DC_bot.Service.BotControl;

/// <summary>
/// Builds mobile-command outcomes and optional Discord-notification context.
/// </summary>
public class BotControlResultFactory : IBotControlResultFactory
{

    /// <summary>
    /// Builds a successful operation result with response and notification context.
    /// </summary>
    /// <param name="command">The command supplied for this operation.</param>
    /// <param name="message">The message supplied for this operation.</param>
    /// <param name="data">The data supplied for this operation.</param>
    /// <param name="voiceChannelId">The voice channel id supplied for this operation.</param>
    /// <param name="textChannelId">The text channel id supplied for this operation.</param>
    /// <param name="shouldNotifyDiscord">The should notify discord supplied for this operation.</param>
    /// <returns>The value produced for the supplied operation context.</returns>
    public BotControlCommandResult Success(
        BotControlCommandRecord command,
        string message,
        object? data = null,
        ulong? voiceChannelId = null,
        ulong? textChannelId = null,
        bool shouldNotifyDiscord = true)
    {
        var resultJson = SerializeResult(command, success: true, message, errorCode: null, data, voiceChannelId, textChannelId, discordNotificationSent: false);
        return new BotControlCommandResult(true, message, resultJson, command.GuildId, command.UserId, voiceChannelId, textChannelId, shouldNotifyDiscord);
    }

    /// <summary>
    /// Builds a failed operation result with error and notification context.
    /// </summary>
    /// <param name="command">The command supplied for this operation.</param>
    /// <param name="message">The message supplied for this operation.</param>
    /// <param name="errorCode">The error code supplied for this operation.</param>
    /// <param name="data">The data supplied for this operation.</param>
    /// <param name="voiceChannelId">The voice channel id supplied for this operation.</param>
    /// <param name="textChannelId">The text channel id supplied for this operation.</param>
    /// <param name="shouldNotifyDiscord">The should notify discord supplied for this operation.</param>
    /// <returns>The value produced for the supplied operation context.</returns>
    public BotControlCommandResult Failure(
        BotControlCommandRecord command,
        string message,
        string errorCode,
        object? data = null,
        ulong? voiceChannelId = null,
        ulong? textChannelId = null,
        bool shouldNotifyDiscord = true)
    {
        var resultJson = SerializeResult(command, success: false, message, errorCode, data, voiceChannelId, textChannelId, discordNotificationSent: false);
        return new BotControlCommandResult(false, message, resultJson, command.GuildId, command.UserId, voiceChannelId, textChannelId, shouldNotifyDiscord);
    }

    /// <summary>
    /// Serializes a structured mobile-command outcome for durable storage.
    /// </summary>
    /// <param name="command">The command supplied for this operation.</param>
    /// <param name="success">The success supplied for this operation.</param>
    /// <param name="message">The message supplied for this operation.</param>
    /// <param name="errorCode">The error code supplied for this operation.</param>
    /// <param name="data">The data supplied for this operation.</param>
    /// <param name="voiceChannelId">The voice channel id supplied for this operation.</param>
    /// <param name="textChannelId">The text channel id supplied for this operation.</param>
    /// <param name="discordNotificationSent">The discord notification sent supplied for this operation.</param>
    /// <returns>The formatted, serialized, or resolved text value.</returns>
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
        return JsonSerializer.Serialize(new { success, commandId = command.CommandId, type = command.Type, message, guildId = command.GuildId.ToString(), userId = command.UserId.ToString(), voiceChannelId = voiceChannelId?.ToString(), textChannelId = textChannelId?.ToString(), discordNotificationSent, errorCode, data });
    }
}
