namespace API.Responses.Playback;

/// <summary>
/// Carries the playback track payload returned to HTTP and mobile clients.
/// </summary>
/// <param name="Title">The title value for this operation.</param>
/// <param name="Author">The author value for this operation.</param>
/// <param name="Duration">The duration value for this operation.</param>
/// <param name="TrackUri">The track uri value for this operation.</param>
/// <param name="ArtworkUri">The artwork uri value for this operation.</param>
/// <param name="RequestedBy">The requested by value for this operation.</param>
public sealed record PlaybackTrackResponse(
    string Title,
    string Author,
    int Duration,
    string TrackUri,
    string? ArtworkUri,
    string? RequestedBy);
