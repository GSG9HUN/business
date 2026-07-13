namespace DC_bot.Interface.Service.Music.PlaylistServiceInterface.Models;

public sealed record PlaylistDto(
    string Name,
    IReadOnlyList<PlaylistTrackDto> Tracks);
