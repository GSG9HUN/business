namespace DC_bot.Interface.Service.Music.PlaylistServiceInterface.Models;

/// <summary>
/// Carries outcome and response data for load playlist operations.
/// </summary>
/// <param name="Status">The status supplied for this operation.</param>
/// <param name="Playlist">The playlist supplied for this operation.</param>
public sealed record LoadPlaylistResult(LoadPlaylistStatus Status, PlaylistDto? Playlist);
