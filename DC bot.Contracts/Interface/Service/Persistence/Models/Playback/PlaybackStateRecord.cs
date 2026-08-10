namespace DC_bot.Interface.Service.Persistence.Models.Playback;

public record PlaybackStateRecord(
    ulong GuildId,
    bool IsRepeating,
    bool IsRepeatingList,
    string? CurrentTrackIdentifier,
    long? QueueItemId, 
    DateTimeOffset UpdatedAtUtc);
