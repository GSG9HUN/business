namespace DC_bot.BotControl;

/// <summary>
/// Carries immutable bot control command channel context values across the shared storage or mobile-command boundary.
/// </summary>
/// <param name = "VoiceChannelId">The voice channel id value used by this storage contract.</param>
/// <param name = "TextChannelId">The text channel id value used by this storage contract.</param>
public sealed record BotControlCommandChannelContext(ulong? VoiceChannelId, ulong? TextChannelId);
