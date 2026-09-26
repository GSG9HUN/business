namespace DC_bot.Interface.Service.BotControl.Models;

public sealed record BotControlExecutionContext(
    ulong GuildId,
    ulong UserId,
    ulong? VoiceChannelId,
    ulong? TextChannelId,
    bool CanSendDiscordResponse);