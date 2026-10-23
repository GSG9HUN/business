namespace DC_bot.Interface.Service.Persistence.Models.Guilds;

/// <summary>
/// Contains the persistent voice connection preferences of a guild.
/// </summary>
/// <param name="IsEnabled">Whether twenty-four-seven mode is enabled.</param>
/// <param name="VoiceChannelId">The preferred voice channel identifier.</param>
/// <param name="TextChannelId">The preferred playback notification channel identifier.</param>
public sealed record TwentyFourSevenSettingsRecord(
    bool IsEnabled,
    ulong? VoiceChannelId,
    ulong? TextChannelId);