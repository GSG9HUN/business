using DC_bot.Interface.Service.BotControl.Models;
using DC_bot.Interface.Service.Persistence.Models.BotControlCommand;

namespace DC_bot.Interface.Service.BotControl;

/// <summary>
/// Defines the bot control result factory boundary so callers are independent of its concrete implementation.
/// </summary>
public interface IBotControlResultFactory
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
    BotControlCommandResult Success(
        BotControlCommandRecord command,
        string message,
        object? data = null,
        ulong? voiceChannelId = null,
        ulong? textChannelId = null,
        bool shouldNotifyDiscord = true);

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
    BotControlCommandResult Failure(
        BotControlCommandRecord command,
        string message,
        string errorCode,
        object? data = null,
        ulong? voiceChannelId = null,
        ulong? textChannelId = null,
        bool shouldNotifyDiscord = true);
}
