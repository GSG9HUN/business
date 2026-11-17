namespace DC_bot.Configuration;

/// <summary>Defines shared saved-playlist count limits for guilds without active premium.</summary>
/// <remarks>
/// Consumers bypass these count limits when guild premium is enabled and has no expiry or has not expired.
/// Name, ownership, uniqueness, and storage validation still apply to premium guilds.
/// </remarks>
public sealed class PlaylistOptions
{

    /// <summary>Gets or sets the maximum number of saved playlists per guild without active premium. Defaults to 5.</summary>
    public int MaxPlaylistsPerGuild { get; set; } = 5;

    /// <summary>Gets or sets the maximum number of tracks per saved playlist for guilds without active premium. Defaults to 100.</summary>
    public int MaxTracksPerPlaylist { get; set; } = 100;

    /// <summary>Gets or sets the maximum number of tracks per import for guilds without active premium. Defaults to 100.</summary>
    public int MaxImportedTracks { get; set; } = 100;
}
