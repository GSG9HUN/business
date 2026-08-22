namespace API.Responses.Guilds;

public sealed record GuildBotStatusResponse(
    bool IsOnline,
    string? ConnectedVoiceChannelName,
    int ConnectedVoiceUserCount);
