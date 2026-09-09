namespace DC_bot.Interface.Service.Persistence.Models.Playback;

public record PlaybackStateRecord(
    ulong GuildId,
    bool IsRepeating,
    bool IsRepeatingList,
    string? CurrentTrackIdentifier,
    long? QueueItemId, 
    DateTimeOffset UpdatedAtUtc,
    bool IsPaused = false,
    int PositionSeconds = 0,
    DateTimeOffset? PositionUpdatedAtUtc = null);
