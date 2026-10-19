namespace DC_bot.Configuration;

public sealed class PlaylistOptions
{
    public int MaxPlaylistsPerGuild { get; set; } = 5;
    public int MaxTracksPerPlaylist { get; set; } = 250;
    public int MaxImportedTracks { get; set; } = 100;
}
