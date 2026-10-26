namespace API.Responses.Playlists;

/// <summary>
/// Carries the playlist track payload returned to HTTP and mobile clients.
/// </summary>
/// <param name="OrderNumber">The order number value for this operation.</param>
/// <param name="Title">The title value for this operation.</param>
/// <param name="Author">The author value for this operation.</param>
/// <param name="Duration">The duration value for this operation.</param>
/// <param name="TrackUri">The track uri value for this operation.</param>
public sealed record PlaylistTrackResponse(int OrderNumber, string Title, string Author, int Duration, string TrackUri);
