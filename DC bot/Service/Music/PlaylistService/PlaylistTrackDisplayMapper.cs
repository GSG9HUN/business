using DC_bot.Interface.Service.Music.PlaylistServiceInterface.Models;
using DC_bot.Interface.Service.Music.Tracks;
using DC_bot.Interface.Service.Persistence.Models.Playlists;
using Microsoft.Extensions.Logging;

namespace DC_bot.Service.Music.PlaylistService;

/// <summary>
/// Maps stored tracks to display DTOs, logging entries whose encoded tracks cannot be restored.
/// </summary>
/// <param name="trackSerializer">The track serializer dependency used by this boundary.</param>
/// <param name="logger">The structured logger used at this operation boundary.</param>
internal sealed class PlaylistTrackDisplayMapper(ITrackSerializer trackSerializer, ILogger<PlaylistService> logger)
{

    /// <summary>
    /// Maps persisted playlist ordering and source identity into a response DTO.
    /// </summary>
    /// <param name="track">The track supplied for this operation.</param>
    /// <returns>The value produced for the supplied operation context.</returns>
    internal static PlaylistTrackDto MapToDto(PlaylistTrackRecord track)
    {
        return new PlaylistTrackDto(track.OrderNumber, track.Source, track.TrackIdentifier, track.TrackUri);
    }

    /// <summary>
    /// Restores display metadata and logs stored tracks whose identifiers cannot be parsed.
    /// </summary>
    /// <param name="tracks">The tracks supplied for this operation.</param>
    /// <param name="playlistName">The playlist name supplied for this operation.</param>
    /// <param name="guildId">The Discord guild whose state or operation is addressed.</param>
    /// <returns>The value produced for the supplied operation context.</returns>
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
                viewTracks.Add(new PlaylistViewTrackDto(track.OrderNumber, lavaLinkTrack.Title, lavaLinkTrack.Author, lavaLinkTrack.Duration, track.TrackUri));
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Skipping unparsable track in playlist {PlaylistName} for guild {GuildId}. PlaylistTrackId: {PlaylistTrackId}", playlistName, guildId, track.Id);
            }
        }

        return viewTracks;
    }
}
