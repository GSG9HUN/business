namespace API.Responses.Guilds;

/// <summary>
/// Carries the guild bot status payload returned to HTTP and mobile clients.
/// </summary>
/// <param name="IsOnline">The is online value for this operation.</param>
/// <param name="ConnectedVoiceChannelName">The connected voice channel name value for this operation.</param>
/// <param name="ConnectedVoiceUserCount">The connected voice user count value for this operation.</param>
public sealed record GuildBotStatusResponse(bool IsOnline, string? ConnectedVoiceChannelName, int ConnectedVoiceUserCount);
