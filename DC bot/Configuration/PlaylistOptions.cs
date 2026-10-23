namespace DC_bot.Configuration;

/// <summary>
/// Defines playlist and track limits applied to saved-playlist operations.
/// </summary>
public sealed class PlaylistOptions
{

    /// <summary>
    /// The maximum number of saved playlists allowed per guild.
    /// </summary>
    public int MaxPlaylistsPerGuild { get; set; } = 5;

    /// <summary>
    /// The maximum number of tracks allowed in a saved playlist.
    /// </summary>
    public int MaxTracksPerPlaylist { get; set; } = 250;

    /// <summary>
    /// The maximum number of tracks accepted from a playlist import.
    /// </summary>
    public int MaxImportedTracks { get; set; } = 100;
}
