using API.Responses.Queue;
using DC_bot.Interface.Service.Persistence.Models.Queue;

namespace API.Mapping;

public static class QueueResponseMapper
{
    public static QueueResponse Map(ulong guildId, IReadOnlyList<QueueItemRecord> queueItems)
    {
        var tracks = new List<QueueTrackResponse>(queueItems.Count);
        var position = 1;

        foreach (var item in queueItems)
        {
            var mappedTrack = TrackResponseMapper.TryMapQueueTrack(
                item.TrackIdentifier,
                position,
                item.RequestedBy);

            if (mappedTrack is null)
            {
                continue;
            }

            tracks.Add(mappedTrack);
            position++;
        }

        return new QueueResponse(
            guildId.ToString(),
            tracks.Count,
            tracks);
    }
}