namespace DC_bot.Interface.Service.Music.PlaylistServiceInterface.Models;

public sealed record PlaylistTrackDto(
    int OrderNumber,
    string Source,
    string TrackIdentifier,
    string TrackUri);
