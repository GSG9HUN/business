namespace DC_bot.BotControl;

public sealed record BotControlCommandChannelContext(
    ulong? VoiceChannelId,
    ulong? TextChannelId);