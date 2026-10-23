namespace DC_bot.Entities.Playlists;

/// <summary>
/// Represents the persisted playlist track row mapped by the EF Core model.
/// </summary>
public class PlaylistTrackEntity
{

    /// <summary>
    /// The identifier of this persisted record.
    /// </summary>
    public long Id { get; set; }

    /// <summary>
    /// The identifier of the saved playlist containing this track.
    /// </summary>
    public long PlaylistId { get; set; }

    /// <summary>
    /// The track's order within the saved playlist.
    /// </summary>
    public int OrderNumber { get; set; }

    /// <summary>
    /// The provider from which the saved track originated.
    /// </summary>
    public string Source { get; set; } = string.Empty;

    /// <summary>
    /// The stored track identity used to resolve the audio track.
    /// </summary>
    public string TrackIdentifier { get; set; } = string.Empty;

    /// <summary>
    /// The URI of the saved track.
    /// </summary>
    public string TrackUri { get; set; } = string.Empty;

    /// <summary>
    /// The saved playlist containing this track.
    /// </summary>
    public PlaylistEntity Playlist { get; set; } = null !;
}
