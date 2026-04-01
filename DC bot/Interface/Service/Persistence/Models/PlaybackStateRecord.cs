namespace DC_bot.Interface.Service.Persistence.Models;

public sealed record PlaybackStateRecord(
    ulong GuildId,
    bool IsRepeating,
    bool IsRepeatingList,
    string? CurrentTrackIdentifier,
    DateTimeOffset UpdatedAtUtc);
