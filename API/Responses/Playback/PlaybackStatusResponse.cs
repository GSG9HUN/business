using API.Responses.Guilds;

namespace API.Responses.Playback;

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

public sealed record PlaybackTrackResponse(
    string Title,
    string Author,
    int Duration,
    string TrackUri,
    string? ArtworkUri,
    string? RequestedBy);
