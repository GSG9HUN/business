using DC_bot.Interface.Service.Persistence.Models;
using DC_bot.Persistence.Entities;

namespace DC_bot.Persistence.Repositories;

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
