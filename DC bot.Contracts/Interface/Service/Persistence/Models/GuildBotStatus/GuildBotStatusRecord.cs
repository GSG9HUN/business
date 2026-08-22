namespace DC_bot.Interface.Service.Persistence.Models.GuildBotStatus;

public record GuildBotStatusRecord(
    ulong GuildId,
    bool IsConnectedToVoice,
    string? ConnectedVoiceChannelName,
    int ConnectedVoiceUserCount,
    DateTimeOffset UpdatedAtUtc);