namespace DC_bot.Interface.Service.Persistence.Models.Playlists;

public sealed record PlaylistRecord(
    long Id,
    ulong GuildId,
    string Name);
