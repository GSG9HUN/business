namespace DC_bot.Interface.Service.Persistence.Models.Playlists;

public sealed record PlaylistSummaryRecord(
    long Id,
    ulong GuildId,
    string Name,
    int TrackCount);
