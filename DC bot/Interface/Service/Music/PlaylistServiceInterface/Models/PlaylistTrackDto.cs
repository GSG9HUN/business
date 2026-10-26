namespace DC_bot.Interface.Service.Music.PlaylistServiceInterface.Models;

/// <summary>
/// Carries playlist track values across the playlist presentation boundary.
/// </summary>
/// <param name="OrderNumber">The order number supplied for this operation.</param>
/// <param name="Source">The source supplied for this operation.</param>
/// <param name="TrackIdentifier">The track identifier supplied for this operation.</param>
/// <param name="TrackUri">The track uri supplied for this operation.</param>
public sealed record PlaylistTrackDto(int OrderNumber, string Source, string TrackIdentifier, string TrackUri);
