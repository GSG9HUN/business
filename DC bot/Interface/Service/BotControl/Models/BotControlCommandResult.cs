namespace DC_bot.Interface.Service.BotControl.Models;

/// <summary>
/// Contains durable command results and optional Discord-notification metadata.
/// </summary>
/// <param name="Success">The success supplied for this operation.</param>
/// <param name="Message">The message supplied for this operation.</param>
/// <param name="ResultJson">The result json supplied for this operation.</param>
/// <param name="GuildId">The Discord guild whose state or operation is addressed.</param>
/// <param name="UserId">The user id supplied for this operation.</param>
/// <param name="VoiceChannelId">The voice channel id supplied for this operation.</param>
/// <param name="TextChannelId">The text channel id supplied for this operation.</param>
/// <param name="ShouldNotifyDiscord">The should notify discord supplied for this operation.</param>
public sealed record BotControlCommandResult(
    bool Success,
    string Message,
    string ResultJson,
    ulong GuildId,
    ulong UserId,
    ulong? VoiceChannelId,
    ulong? TextChannelId,
    bool ShouldNotifyDiscord);
