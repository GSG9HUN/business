namespace DC_bot.Interface.Service.Persistence.Models;

public sealed record PlaylistTrackCreateRecord(
    string Source,
    string TrackIdentifier,
    string TrackUri);
