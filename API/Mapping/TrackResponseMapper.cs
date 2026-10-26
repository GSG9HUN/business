using API.Responses.Playback;
using API.Responses.Playlists;
using API.Responses.Queue;
using DC_bot.Interface.Service.Persistence.Models.Playlists;
using Lavalink4NET.Tracks;

namespace API.Mapping;

/// <summary>
/// Decodes Lavalink tracks into API payloads and clamps duration values to supported integer seconds.
/// </summary>
internal static class TrackResponseMapper
{

    /// <summary>
    /// Decodes a queued track into its position, metadata, duration, and requester, or returns null.
    /// </summary>
    /// <param name="trackIdentifier">The track identifier value for this operation.</param>
    /// <param name="position">The position value for this operation.</param>
    /// <param name="requestedBy">The requested by value for this operation.</param>
    /// <returns>The converted value, or null when the input is absent or cannot be represented.</returns>
    internal static QueueTrackResponse? TryMapQueueTrack(string trackIdentifier, int position, string? requestedBy = null)
    {
        var track = TryParse(trackIdentifier);
        return track is null ? null : new QueueTrackResponse(position, track.Title, track.Author, ToDurationSeconds(track.Duration), track.Uri?.ToString() ?? string.Empty, track.ArtworkUri?.ToString(), requestedBy);
    }

    /// <summary>
    /// Decodes the current track into playback metadata and requester information, or returns null.
    /// </summary>
    /// <param name="trackIdentifier">The track identifier value for this operation.</param>
    /// <param name="requestedBy">The requested by value for this operation.</param>
    /// <returns>The converted value, or null when the input is absent or cannot be represented.</returns>
    internal static PlaybackTrackResponse? TryMapPlaybackTrack(string trackIdentifier, string? requestedBy = null)
    {
        var track = TryParse(trackIdentifier);
        return track is null ? null : new PlaybackTrackResponse(track.Title, track.Author, ToDurationSeconds(track.Duration), track.Uri?.ToString() ?? string.Empty, track.ArtworkUri?.ToString(), requestedBy);
    }

    /// <summary>
    /// Decodes a playlist track into its stored order and metadata, or returns null.
    /// </summary>
    /// <param name="trackRecord">The track record value for this operation.</param>
    /// <returns>The converted value, or null when the input is absent or cannot be represented.</returns>
    internal static PlaylistTrackResponse? TryMapPlaylistTrack(PlaylistTrackRecord trackRecord)
    {
        var track = TryParse(trackRecord.TrackIdentifier);
        return track is null ? null : new PlaylistTrackResponse(trackRecord.OrderNumber, track.Title, track.Author, ToDurationSeconds(track.Duration), trackRecord.TrackUri);
    }

    /// <summary>
    /// Sums decodable playlist durations in seconds, saturating at the maximum integer value.
    /// </summary>
    /// <param name="tracks">The tracks value for this operation.</param>
    /// <returns>The computed sum durations result.</returns>
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

    /// <summary>
    /// Converts a duration to nonnegative whole seconds, saturating at the maximum integer value.
    /// </summary>
    /// <param name="duration">The duration value for this operation.</param>
    /// <returns>The computed to duration seconds result.</returns>
    internal static int ToDurationSeconds(TimeSpan duration)
    {
        if (duration <= TimeSpan.Zero)
        {
            return 0;
        }

        return duration.TotalSeconds >= int.MaxValue ? int.MaxValue : (int)duration.TotalSeconds;
    }

    /// <summary>
    /// Tries to decode a persisted Lavalink track and returns null for an invalid payload.
    /// </summary>
    /// <param name="trackIdentifier">The track identifier value for this operation.</param>
    /// <returns>The converted value, or null when the input is absent or cannot be represented.</returns>
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
