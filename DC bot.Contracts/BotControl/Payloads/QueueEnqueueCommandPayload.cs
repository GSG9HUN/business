namespace DC_bot.BotControl.Payloads;

/// <summary>
/// Carries immutable queue enqueue command values across the shared storage or mobile-command boundary.
/// </summary>
/// <param name = "Query">The query value used by this storage contract.</param>
/// <param name = "SearchMode">The search mode value used by this storage contract.</param>
/// <param name = "RequestedBy">The optional requester display value, subject to the storage length limit.</param>
/// <param name = "VoiceChannelId">The voice channel id value used by this storage contract.</param>
/// <param name = "TextChannelId">The text channel id value used by this storage contract.</param>
public sealed record QueueEnqueueCommandPayload(
    string Query,
    string? SearchMode,
    string? RequestedBy,
    ulong? VoiceChannelId = null,
    ulong? TextChannelId = null);
