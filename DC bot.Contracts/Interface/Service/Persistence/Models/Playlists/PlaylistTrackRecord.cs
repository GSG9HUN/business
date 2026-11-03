namespace DC_bot.Interface.Service.Persistence.Models.Playlists;

/// <summary>
/// Carries immutable playlist track values across the shared storage or mobile-command boundary.
/// </summary>
/// <param name = "Id">The id value used by this storage contract.</param>
/// <param name = "PlaylistId">The durable playlist identifier.</param>
/// <param name = "OrderNumber">The order number value used by this storage contract.</param>
/// <param name = "Source">The source value used by this storage contract.</param>
/// <param name = "TrackIdentifier">The encoded source-track identity stored independently of any runtime player.</param>
/// <param name = "TrackUri">The track uri value used by this storage contract.</param>
public sealed record PlaylistTrackRecord(
    long Id,
    long PlaylistId,
    int OrderNumber,
    string Source,
    string TrackIdentifier,
    string TrackUri);
