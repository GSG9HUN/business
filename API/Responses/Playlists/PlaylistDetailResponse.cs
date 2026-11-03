namespace API.Responses.Playlists;

/// <summary>Describes a saved playlist with its ordered tracks and optional cover.</summary>
/// <param name="Id">The playlist identifier.</param>
/// <param name="GuildId">The owning guild identifier.</param>
/// <param name="Name">The playlist name.</param>
/// <param name="SongCount">The saved track count.</param>
/// <param name="Duration">The total duration in seconds.</param>
/// <param name="Tracks">The decodable saved tracks in playlist order.</param>
/// <param name="ArtworkUri">The custom cover URL, first available track artwork, or null.</param>
public sealed record PlaylistDetailResponse(
    string Id,
    string GuildId,
    string Name,
    int SongCount,
    int Duration,
    IReadOnlyList<PlaylistTrackResponse> Tracks,
    string? ArtworkUri = null);
