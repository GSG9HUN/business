namespace API.Responses.Playlists;

/// <summary>Describes a saved playlist in a guild playlist listing.</summary>
/// <param name="Id">The playlist identifier.</param>
/// <param name="Name">The playlist name.</param>
/// <param name="SongCount">The saved track count.</param>
/// <param name="Duration">The total duration in seconds.</param>
/// <param name="ArtworkUri">The custom cover URL, first available track artwork, or null.</param>
public sealed record PlaylistSummaryResponse(
    string Id,
    string Name,
    int SongCount,
    int Duration,
    string? ArtworkUri = null);
