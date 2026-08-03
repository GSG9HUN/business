namespace DC_bot.Interface.Service.Persistence.Models;

public sealed record PlaylistSummaryRecord(
    long Id,
    ulong GuildId,
    string Name,
    int TrackCount);
