using DC_bot.Db;
using DC_bot.Entities.Queue;
using Microsoft.EntityFrameworkCore;

namespace DC_bot.Repositories.Queue;

/// <summary>Synchronizes durable queue edits with the canonical batch without deleting referenced history.</summary>
internal static class QueueRepeatListRepository
{

    /// <summary>Excludes removed canonical occurrences and saves the surviving repeat order in the caller's transaction.</summary>
    internal static async Task SynchronizeAsync(
        BotDbContext dbContext,
        ulong guildId,
        IReadOnlyList<GuildQueueItemEntity> previousQueue,
        IReadOnlyList<GuildQueueItemEntity> updatedQueue,
        CancellationToken cancellationToken)
    {
        var batchIds = previousQueue.Where(item => item.BatchId.HasValue)
            .Select(item => item.BatchId!.Value)
            .Distinct()
            .ToArray();
        var canonical = await dbContext.PlaybackBatchItems.Where(item => item.Batch.GuildId == guildId
                && batchIds.Contains(item.BatchId))
            .OrderBy(item => item.Position)
            .ToListAsync(cancellationToken);
        var previousIds = previousQueue.Where(item => item.BatchItemId.HasValue)
            .Select(item => item.BatchItemId!.Value)
            .ToHashSet();
        var updatedIds = updatedQueue.Where(item => item.BatchItemId.HasValue)
            .Select(item => item.BatchItemId!.Value)
            .ToHashSet();

        foreach (var item in canonical)
        {
            if (previousIds.Contains(item.Id) && !updatedIds.Contains(item.Id))
            {
                item.IsRemoved = true;
            }
        }

        foreach (var batch in canonical.GroupBy(item => item.BatchId))
        {
            var lookup = batch.ToDictionary(item => item.Id);
            var ordered = batch.Where(item => !item.IsRemoved && !previousIds.Contains(item.Id))
                .ToList();
            ordered.AddRange(updatedQueue.Where(item => item.BatchId == batch.Key && item.BatchItemId.HasValue)
                .Select(item => lookup[item.BatchItemId!.Value])
                .Where(item => !item.IsRemoved)
                .DistinctBy(item => item.Id));
            ordered.AddRange(batch.Where(item => item.IsRemoved));

            for (var index = 0; index < ordered.Count; index++)
            {
                ordered[index].Position = -index - 1;
            }

            await dbContext.SaveChangesAsync(cancellationToken);

            for (var index = 0; index < ordered.Count; index++)
            {
                ordered[index].Position = index;
            }

            await dbContext.SaveChangesAsync(cancellationToken);
        }

    }

    /// <summary>Prevents the active list from refilling while retaining canonical and occurrence history.</summary>
    internal static async Task ClearAsync(BotDbContext dbContext, ulong guildId, CancellationToken cancellationToken)
    {
        var activeIds = dbContext.GuildPlaybackStates.Where(state => state.GuildId == guildId)
            .Select(state => state.ActiveBatchId);
        await dbContext.PlaybackBatchItems.Where(item => item.Batch.GuildId == guildId && activeIds.Contains(item.BatchId))
            .ExecuteUpdateAsync(setters => setters.SetProperty(item => item.IsRemoved, true), cancellationToken);
    }
}
