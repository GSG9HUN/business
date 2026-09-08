namespace API.Responses.Playlists;

public sealed record PlaylistSummaryResponse(
    string Id,
    string Name,
    int SongCount,
    int Duration);
