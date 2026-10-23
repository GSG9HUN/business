namespace DC_bot.Interface.Service.BotControl.Models;

/// <summary>
/// Contains guild, user, channel, and notification permissions resolved for a mobile command.
/// </summary>
/// <param name="GuildId">The Discord guild whose state or operation is addressed.</param>
/// <param name="UserId">The user id supplied for this operation.</param>
/// <param name="VoiceChannelId">The voice channel id supplied for this operation.</param>
/// <param name="TextChannelId">The text channel id supplied for this operation.</param>
/// <param name="CanSendDiscordResponse">The can send discord response supplied for this operation.</param>
public sealed record BotControlExecutionContext(
    ulong GuildId,
    ulong UserId,
    ulong? VoiceChannelId,
    ulong? TextChannelId,
    bool CanSendDiscordResponse);
