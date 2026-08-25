namespace DC_bot.Interface.Service.Persistence.Models.Playlists;

public sealed record PlaylistTrackCreateRecord(
    string Source,
    string TrackIdentifier,
    string TrackUri);
