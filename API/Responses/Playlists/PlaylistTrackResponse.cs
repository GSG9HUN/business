namespace API.Responses.Playlists;

public sealed record PlaylistTrackResponse(
    int OrderNumber,
    string Title,
    string Author,
    int Duration,
    string TrackUri);
