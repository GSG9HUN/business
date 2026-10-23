namespace DC_bot.Interface.Service.Persistence.Models.GuildBotStatus;

/// <summary>
/// Carries immutable guild bot status values across the shared storage or mobile-command boundary.
/// </summary>
/// <param name = "GuildId">The Discord guild whose persisted state is addressed.</param>
/// <param name = "IsConnectedToVoice">The is connected to voice value used by this storage contract.</param>
/// <param name = "ConnectedVoiceChannelName">The connected voice channel name value used by this storage contract.</param>
/// <param name = "ConnectedVoiceUserCount">The connected voice user count value used by this storage contract.</param>
/// <param name = "UpdatedAtUtc">The updated at utc value used by this storage contract.</param>
public record GuildBotStatusRecord(
    ulong GuildId,
    bool IsConnectedToVoice,
    string? ConnectedVoiceChannelName,
    int ConnectedVoiceUserCount,
    DateTimeOffset UpdatedAtUtc);
