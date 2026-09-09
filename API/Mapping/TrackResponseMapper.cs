using API.Responses.Playback;
using API.Responses.Playlists;
using API.Responses.Queue;
using DC_bot.Interface.Service.Persistence.Models.Playlists;
using Lavalink4NET.Tracks;

namespace API.Mapping;

internal static class TrackResponseMapper
{
    internal static QueueTrackResponse? TryMapQueueTrack(string trackIdentifier, int position)
    {
        var track = TryParse(trackIdentifier);
        return track is null
            ? null
            : new QueueTrackResponse(
                position,
                track.Title,
                track.Author,
                ToDurationSeconds(track.Duration),
                track.Uri?.ToString() ?? string.Empty,
                track.ArtworkUri?.ToString());
    }

    internal static PlaybackTrackResponse? TryMapPlaybackTrack(string trackIdentifier)
    {
        var track = TryParse(trackIdentifier);
        return track is null
            ? null
            : new PlaybackTrackResponse(
                track.Title,
                track.Author,
                ToDurationSeconds(track.Duration),
                track.Uri?.ToString() ?? string.Empty,
                track.ArtworkUri?.ToString());
    }

    internal static PlaylistTrackResponse? TryMapPlaylistTrack(PlaylistTrackRecord trackRecord)
    {
        var track = TryParse(trackRecord.TrackIdentifier);
        return track is null
            ? null
            : new PlaylistTrackResponse(
                trackRecord.OrderNumber,
                track.Title,
                track.Author,
                ToDurationSeconds(track.Duration),
                trackRecord.TrackUri);
    }

    internal static int SumDurations(IReadOnlyCollection<PlaylistTrackRecord> tracks)
    {
        var totalSeconds = 0L;

        foreach (var track in tracks)
        {
            var parsedTrack = TryParse(track.TrackIdentifier);
            if (parsedTrack is null)
            {
                continue;
            }

            totalSeconds += ToDurationSeconds(parsedTrack.Duration);
        }

        return totalSeconds >= int.MaxValue ? int.MaxValue : (int)totalSeconds;
    }

    internal static int ToDurationSeconds(TimeSpan duration)
    {
        if (duration <= TimeSpan.Zero)
        {
            return 0;
        }

        return duration.TotalSeconds >= int.MaxValue
            ? int.MaxValue
            : (int)duration.TotalSeconds;
    }

    private static LavalinkTrack? TryParse(string trackIdentifier)
    {
        try
        {
            return LavalinkTrack.Parse(trackIdentifier, null);
        }
        catch
        {
            return null;
        }
    }
}
