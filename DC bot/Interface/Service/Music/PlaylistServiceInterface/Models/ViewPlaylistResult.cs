namespace DC_bot.Interface.Service.Music.PlaylistServiceInterface.Models;

/// <summary>
/// Carries outcome and response data for view playlist operations.
/// </summary>
/// <param name="Status">The status supplied for this operation.</param>
/// <param name="PlaylistName">The playlist name supplied for this operation.</param>
/// <param name="Tracks">The tracks supplied for this operation.</param>
public sealed record ViewPlaylistResult(
    ViewPlaylistStatus Status,
    string PlaylistName,
    IReadOnlyList<PlaylistViewTrackDto> Tracks);
