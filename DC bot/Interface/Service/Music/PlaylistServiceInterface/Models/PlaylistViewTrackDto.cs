namespace DC_bot.Interface.Service.Music.PlaylistServiceInterface.Models;

/// <summary>
/// Contains a saved track&apos;s display metadata and playlist position.
/// </summary>
/// <param name="OrderNumber">The order number supplied for this operation.</param>
/// <param name="Title">The title supplied for this operation.</param>
/// <param name="Author">The author supplied for this operation.</param>
/// <param name="Duration">The duration supplied for this operation.</param>
/// <param name="TrackUri">The track uri supplied for this operation.</param>
public sealed record PlaylistViewTrackDto(int OrderNumber, string Title, string Author, TimeSpan Duration, string TrackUri);
