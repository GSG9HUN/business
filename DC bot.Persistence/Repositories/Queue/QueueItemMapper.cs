using DC_bot.Entities.Queue;
using DC_bot.Interface.Service.Persistence.Models.Queue;

namespace DC_bot.Repositories.Queue;

internal static class QueueItemMapper
{
    internal static QueueItemRecord ToRecord(GuildQueueItemEntity entity)
    {
        return new QueueItemRecord(
            entity.Id,
            entity.GuildId,
            entity.Position,
            entity.TrackIdentifier,
            entity.State,
            entity.AddedAtUtc,
            entity.PlayedAtUtc,
            entity.SkippedAtUtc);
    }
}
