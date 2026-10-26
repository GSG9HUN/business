namespace API.Responses.Playlists;

/// <summary>
/// Carries the playlist detail payload returned to HTTP and mobile clients.
/// </summary>
/// <param name="Id">The id value for this operation.</param>
/// <param name="GuildId">The guild id value for this operation.</param>
/// <param name="Name">The name value for this operation.</param>
/// <param name="SongCount">The song count value for this operation.</param>
/// <param name="Duration">The duration value for this operation.</param>
/// <param name="Tracks">The tracks value for this operation.</param>
public sealed record PlaylistDetailResponse(
    string Id,
    string GuildId,
    string Name,
    int SongCount,
    int Duration,
    IReadOnlyList<PlaylistTrackResponse> Tracks);
