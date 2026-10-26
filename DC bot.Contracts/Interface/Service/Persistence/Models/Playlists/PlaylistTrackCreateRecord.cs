namespace DC_bot.Interface.Service.Persistence.Models.Playlists;

/// <summary>
/// Carries immutable playlist track create values across the shared storage or mobile-command boundary.
/// </summary>
/// <param name = "Source">The source value used by this storage contract.</param>
/// <param name = "TrackIdentifier">The encoded source-track identity stored independently of any runtime player.</param>
/// <param name = "TrackUri">The track uri value used by this storage contract.</param>
public sealed record PlaylistTrackCreateRecord(string Source, string TrackIdentifier, string TrackUri);
