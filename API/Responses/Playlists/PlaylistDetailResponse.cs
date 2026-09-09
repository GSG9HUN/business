namespace API.Responses.Playlists;

public sealed record PlaylistDetailResponse(
    string Id,
    string GuildId,
    string Name,
    int SongCount,
    int Duration,
    IReadOnlyList<PlaylistTrackResponse> Tracks);
