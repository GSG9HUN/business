namespace DC_bot.Interface.Service.BotControl.Models;

public sealed record BotControlCommandResult(
    bool Success,
    string Message,
    string ResultJson,
    ulong GuildId,
    ulong UserId,
    ulong? VoiceChannelId,
    ulong? TextChannelId,
    bool ShouldNotifyDiscord);