namespace API.Responses.Playlists;

public sealed record PlaylistTrackResponse(
    int OrderNumber,
    string Title,
    string Author,
    TimeSpan Duration,
    string TrackUri);