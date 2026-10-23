namespace DC_bot.Interface.Service.Music.PlaylistServiceInterface.Models;

/// <summary>
/// Carries playlist values across the playlist presentation boundary.
/// </summary>
/// <param name="Name">The name supplied for this operation.</param>
/// <param name="Tracks">The tracks supplied for this operation.</param>
public sealed record PlaylistDto(string Name, IReadOnlyList<PlaylistTrackDto> Tracks);
