using API.Responses.Queue;
using DC_bot.Interface.Service.Persistence.Models.Queue;

namespace API.Mapping;

/// <summary>
/// Builds queue responses from persisted items, excluding undecodable track payloads.
/// </summary>
public static class QueueResponseMapper
{

    /// <summary>Maps waiting tracks and a separate batch timeline without changing queue command indices.</summary>
    public static QueueResponse MapWithHistory(
        ulong guildId,
        IReadOnlyList<QueueItemRecord> queueItems,
        IReadOnlyList<QueueItemRecord> displayItems)
    {
        var displayTracks = new List<QueueTrackResponse>();

        var orderedItems = displayItems.OrderBy(item => item.SkippedAtUtc ?? item.PlayedAtUtc ?? DateTimeOffset.MaxValue)
            .ThenBy(item => item.Id);

        foreach (var item in orderedItems)
        {
            var track = TrackResponseMapper.TryMapQueueTrack(item.TrackIdentifier, item.Position, item.RequestedBy);

            if (track is not null)
            {
                displayTracks.Add(WithOccurrence(track, item));
            }
        }

        return Map(guildId, queueItems) with { DisplayTracks = displayTracks };
    }

    /// <summary>Preserves stable occurrence identity, batch membership, and persisted playback state.</summary>
    private static QueueTrackResponse WithOccurrence(QueueTrackResponse track, QueueItemRecord item)
    {
        return track with
        {
            QueueItemId = item.Id.ToString(),
            BatchId = item.EnqueueGroupId.ToString(),
            BatchPosition = item.BatchPosition,
            State = item.State == QueueItemState.Starting ? "Queued" : item.State.ToString(),
            CompletedAtUtc = item.SkippedAtUtc ?? item.PlayedAtUtc
        };
    }

    /// <summary>
    /// Maps decodable queued tracks into a response with contiguous one-based positions.
    /// </summary>
    /// <param name="guildId">The guild id value for this operation.</param>
    /// <param name="queueItems">The queue items value for this operation.</param>
    /// <returns>The computed map result.</returns>
    public static QueueResponse Map(ulong guildId, IReadOnlyList<QueueItemRecord> queueItems)
    {
        var tracks = new List<QueueTrackResponse>(queueItems.Count);
        var position = 1;

        foreach (var item in queueItems)
        {
            var mappedTrack = TrackResponseMapper.TryMapQueueTrack(item.TrackIdentifier, position, item.RequestedBy);

            if (mappedTrack is null)
            {
                continue;
            }

            tracks.Add(WithOccurrence(mappedTrack, item));
            position++;
        }

        return new QueueResponse(guildId.ToString(), tracks.Count, tracks);
    }
}
