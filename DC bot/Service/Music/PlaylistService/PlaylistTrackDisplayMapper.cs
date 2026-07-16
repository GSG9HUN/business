using DC_bot.Interface.Service.Music;
using DC_bot.Interface.Service.Music.PlaylistServiceInterface.Models;
using DC_bot.Interface.Service.Persistence.Models;
using Microsoft.Extensions.Logging;

namespace DC_bot.Service.Music.PlaylistService;

internal sealed class PlaylistTrackDisplayMapper(
    ITrackSerializer trackSerializer,
    ILogger<PlaylistService> logger)
{
    internal static PlaylistTrackDto MapToDto(PlaylistTrackRecord track)
    {
        return new PlaylistTrackDto(
            track.OrderNumber,
            track.Source,
            track.TrackIdentifier,
            track.TrackUri);
    }

    internal List<PlaylistViewTrackDto> MapDisplayTracks(
        IReadOnlyCollection<PlaylistTrackRecord> tracks,
        string playlistName,
        ulong guildId)
    {
        var viewTracks = new List<PlaylistViewTrackDto>(tracks.Count);
        foreach (var track in tracks)
        {
            try
            {
                var lavaLinkTrack = trackSerializer.Deserialize(track.TrackIdentifier);
                viewTracks.Add(new PlaylistViewTrackDto(
                    track.OrderNumber,
                    lavaLinkTrack.Title,
                    lavaLinkTrack.Author,
                    lavaLinkTrack.Duration,
                    track.TrackUri));
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex,
                    "Skipping unparsable track in playlist {PlaylistName} for guild {GuildId}. PlaylistTrackId: {PlaylistTrackId}",
                    playlistName,
                    guildId,
                    track.Id);
            }
        }

        return viewTracks;
    }
}
