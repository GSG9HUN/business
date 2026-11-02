namespace DC_bot.Configuration;

/// <summary>Defines shared saved-playlist limits for the bot and API.</summary>
public sealed class PlaylistOptions
{

    /// <summary>Gets or sets the maximum number of saved playlists per guild.</summary>
    public int MaxPlaylistsPerGuild { get; set; } = 5;

    /// <summary>Gets or sets the maximum number of tracks per saved playlist.</summary>
    public int MaxTracksPerPlaylist { get; set; } = 100;

    /// <summary>Gets or sets the maximum number of tracks accepted by one import.</summary>
    public int MaxImportedTracks { get; set; } = 100;
}
