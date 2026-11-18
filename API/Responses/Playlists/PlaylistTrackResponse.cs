namespace API.Responses.Playlists;

/// <summary>Describes a saved playlist track and its optional source artwork.</summary>
/// <param name="OrderNumber">The saved track position.</param>
/// <param name="Title">The track title.</param>
/// <param name="Author">The track author.</param>
/// <param name="Duration">The duration in seconds.</param>
/// <param name="TrackUri">The saved source URL.</param>
/// <param name="ArtworkUri">The source artwork URL, or null when unavailable.</param>
public sealed record PlaylistTrackResponse(
    int OrderNumber,
    string Title,
    string Author,
    int Duration,
    string TrackUri,
    string? ArtworkUri = null);
