namespace DC_bot.BotControl.Payloads;

public sealed record QueueEnqueueCommandPayload(
    string Query,
    string? SearchMode,
    string? RequestedBy,
    ulong? VoiceChannelId = null,
    ulong? TextChannelId = null);
