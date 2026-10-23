using API.Responses.Queue;
using DC_bot.Interface.Service.Persistence.Models.Queue;

namespace API.Mapping;

/// <summary>
/// Builds queue responses from persisted items, excluding undecodable track payloads.
/// </summary>
public static class QueueResponseMapper
{

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

            tracks.Add(mappedTrack);
            position++;
        }

        return new QueueResponse(guildId.ToString(), tracks.Count, tracks);
    }
}
