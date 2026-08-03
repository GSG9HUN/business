namespace DC_bot.Interface.Service.Persistence.Models;

public sealed record PlaylistTrackRecord(
    long Id,
    long PlaylistId,
    int OrderNumber,
    string Source,
    string TrackIdentifier,
    string TrackUri);
