using API.Responses.Guilds;

namespace API.Responses.Playback;

/// <summary>
/// Carries the playback status payload returned to HTTP and mobile clients.
/// </summary>
/// <param name="GuildId">The guild id value for this operation.</param>
/// <param name="GuildName">The guild name value for this operation.</param>
/// <param name="GuildIconUrl">The guild icon url value for this operation.</param>
/// <param name="BotStatus">The bot status value for this operation.</param>
/// <param name="CurrentTrack">The current track value for this operation.</param>
/// <param name="IsPlaying">The is playing value for this operation.</param>
/// <param name="IsPaused">The is paused value for this operation.</param>
/// <param name="PositionSeconds">The position seconds value for this operation.</param>
/// <param name="QueueTrackCount">The queue track count value for this operation.</param>
/// <param name="IsRepeating">The is repeating value for this operation.</param>
/// <param name="IsRepeatingList">The is repeating list value for this operation.</param>
/// <param name="UpdatedAtUtc">The updated at utc value for this operation.</param>
public sealed record PlaybackStatusResponse(
    string GuildId,
    string GuildName,
    string? GuildIconUrl,
    GuildBotStatusResponse BotStatus,
    PlaybackTrackResponse? CurrentTrack,
    bool IsPlaying,
    bool IsPaused,
    int PositionSeconds,
    int QueueTrackCount,
    bool IsRepeating,
    bool IsRepeatingList,
    DateTimeOffset UpdatedAtUtc);
