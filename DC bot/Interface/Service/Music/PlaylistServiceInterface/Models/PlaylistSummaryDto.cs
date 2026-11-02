namespace DC_bot.Interface.Service.Music.PlaylistServiceInterface.Models;

/// <summary>
/// Contains a saved playlist&apos;s name and track count for list responses.
/// </summary>
/// <param name="Name">The name supplied for this operation.</param>
/// <param name="TrackCount">The track count supplied for this operation.</param>
public sealed record PlaylistSummaryDto(string Name, int TrackCount);
