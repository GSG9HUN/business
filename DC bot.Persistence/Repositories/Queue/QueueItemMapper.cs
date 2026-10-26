using DC_bot.Entities.Queue;
using DC_bot.Interface.Service.Persistence.Models.Queue;

namespace DC_bot.Repositories.Queue;

/// <summary>
/// Maps queue entities to immutable contracts while preserving source, batch, and playback-operation metadata.
/// </summary>
internal static class QueueItemMapper
{

    /// <summary>
    /// Maps a tracked queue entity into its immutable queue contract including batch, source, and operation identity.
    /// </summary>
    /// <param name = "entity">The entity value used by this storage contract.</param>
    /// <returns>The mapped, normalized, or configured value described above.</returns>
    internal static QueueItemRecord ToRecord(GuildQueueItemEntity entity)
    {
        return new QueueItemRecord(entity.Id, entity.GuildId, entity.Position, entity.TrackIdentifier, entity.SourceQuery, entity.SourceSearchMode, entity.RequestedBy, entity.State, entity.BatchId, entity.BatchItemId, entity.AddedAtUtc, entity.PlayedAtUtc, entity.SkippedAtUtc);
    }
}
