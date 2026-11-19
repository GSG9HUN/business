namespace API.Requests.Playlists;

/// <summary>Sets a playlist cover URL, or restores automatic artwork when null.</summary>
/// <param name="ArtworkUri">An absolute HTTP or HTTPS image URL, or null to clear the custom cover.</param>
public sealed record SetPlaylistArtworkRequest(string? ArtworkUri);
