using DC_bot.Db;
using DC_bot.Entities.Playback;
using DC_bot.Entities.Queue;
using DC_bot.Interface.Service.Persistence.Models.Queue;
using Microsoft.EntityFrameworkCore;

namespace DC_bot.Repositories.Queue;

/// <summary>
/// Maintains canonical batch membership and legacy adoption within the caller&apos;s current database context and transaction.
/// </summary>
internal static class QueueBatchStore
{

    /// <summary>
    /// Resolves or creates the active batch, adopting eligible legacy queue and repeat-snapshot entries when necessary.
    /// </summary>
    /// <param name = "db">The caller&apos;s EF context, whose current transaction remains owned by the caller.</param>
    /// <param name = "guildId">The Discord guild whose persisted state is addressed.</param>
    /// <param name = "create">Whether a new canonical batch may be created when no suitable batch is active.</param>
    /// <param name = "ct">Cancels supported database work and retry backoff; owned rollback uses its own deadline.</param>
    /// <returns>The matching value, or null when no value satisfies the documented lookup.</returns>
    internal static async Task<PlaybackBatchEntity?> GetActiveAsync(BotDbContext db, ulong guildId, bool create, CancellationToken ct)
    {
        var state = await db.GuildPlaybackStates.SingleOrDefaultAsync(x => x.GuildId == guildId, ct);

        if (state?.ActiveBatchId is { } activeId)
        {
            var active = await db.PlaybackBatches.SingleAsync(x => x.Id == activeId && x.GuildId == guildId, ct);

            if (active.CompletedAtUtc is null)
            {
                if (!create
                    || state.IsRepeating
                    || state.IsRepeatingList
                    || await HasActiveItemsAsync(db, activeId, ct))
                {
                    return active;
                }

                active.CompletedAtUtc = DateTimeOffset.UtcNow;
            }

            state.ActiveBatchId = null;
        }

        var legacy = await db.GuildQueueItems.Where(x => x.GuildId == guildId && x.BatchId == null && (x.State == QueueItemState.Queued || x.State == QueueItemState.Playing || x.State == QueueItemState.Starting))
            .OrderBy(x => x.Position)
            .ThenBy(x => x.Id)
            .ToListAsync(ct);

        if (!create && legacy.Count == 0)
        {
            return null;
        }

        if (state is null)
        {
            state = new GuildPlaybackStateEntity
            {
                GuildId = guildId,
                UpdatedAtUtc = DateTimeOffset.UtcNow
            };
            db.GuildPlaybackStates.Add(state);
        }

        var batch = new PlaybackBatchEntity
        {
            GuildId = guildId,
            CreatedAtUtc = DateTimeOffset.UtcNow
        };
        await db.SaveChangesAsync(ct);
        db.PlaybackBatches.Add(batch);
        await db.SaveChangesAsync(ct);
        state.ActiveBatchId = batch.Id;
        var saved = state.IsRepeatingList ? await db.GuildRepeatListItems.Where(x => x.GuildId == guildId)
            .OrderBy(x => x.Position)
            .ToListAsync(ct) : [];
        var canonical = saved.Select((x, i) => new PlaybackBatchItemEntity { BatchId = batch.Id, Position = i, TrackIdentifier = x.TrackIdentifier, AddedAtUtc = x.AddedAtUtc })
            .ToList();
        var assigned = new HashSet<PlaybackBatchItemEntity>();
        var pendingAssignments = new List<(GuildQueueItemEntity Row, PlaybackBatchItemEntity Item)>();

        foreach (var row in legacy)
        {
            var item = canonical.FirstOrDefault(x => x.TrackIdentifier == row.TrackIdentifier && !assigned.Contains(x));

            if (item is null)
            {
                item = new PlaybackBatchItemEntity
                {
                    BatchId = batch.Id,
                    Position = canonical.Count,
                    TrackIdentifier = row.TrackIdentifier,
                    SourceQuery = row.SourceQuery,
                    SourceSearchMode = row.SourceSearchMode,
                    RequestedBy = row.RequestedBy,
                    AddedAtUtc = row.AddedAtUtc
                };
                canonical.Add(item);
            }

            assigned.Add(item);
            row.BatchId = batch.Id;
            pendingAssignments.Add((row, item));
        }

        db.PlaybackBatchItems.AddRange(canonical);
        await db.SaveChangesAsync(ct);

        foreach (var(row, item)in pendingAssignments)
        {
            row.BatchItemId = item.Id;
        }

        await db.SaveChangesAsync(ct);
        return batch;
    }

    /// <summary>
    /// Checks for queued, starting, or playing items belonging to the selected batch.
    /// </summary>
    /// <param name = "db">The caller&apos;s EF context, whose current transaction remains owned by the caller.</param>
    /// <param name = "batchId">The canonical playback batch identifier.</param>
    /// <param name = "ct">Cancels supported database work and retry backoff; owned rollback uses its own deadline.</param>
    /// <returns>True when the documented check or conditional mutation succeeds; otherwise false.</returns>
    internal static Task<bool> HasActiveItemsAsync(BotDbContext db, long batchId, CancellationToken ct) => db.GuildQueueItems.AnyAsync(x => x.BatchId == batchId && (x.State == QueueItemState.Queued || x.State == QueueItemState.Playing || x.State == QueueItemState.Starting), ct);

    /// <summary>
    /// Closes an idle or explicitly left batch according to the caller&apos;s ownership and active-item policy.
    /// </summary>
    /// <param name = "db">The caller&apos;s EF context, whose current transaction remains owned by the caller.</param>
    /// <param name = "guildId">The Discord guild whose persisted state is addressed.</param>
    /// <param name = "explicitLeave">Whether batch closure is requested by an explicit leave rather than idle completion.</param>
    /// <param name = "ct">Cancels supported database work and retry backoff; owned rollback uses its own deadline.</param>
    /// <returns>True when the documented check or conditional mutation succeeds; otherwise false.</returns>
    internal static async Task<bool> CloseAsync(BotDbContext db, ulong guildId, bool explicitLeave, CancellationToken ct)
    {
        var batch = await GetActiveAsync(db, guildId, create: false, ct);

        if (batch is null)
        {
            return true;
        }

        var state = await db.GuildPlaybackStates.SingleAsync(x => x.GuildId == guildId, ct);

        if (!explicitLeave
            && (state.IsRepeating
            || state.IsRepeatingList
            || await HasActiveItemsAsync(db, batch.Id, ct)))
        {
            return false;
        }

        if (explicitLeave)
        {
            var active = await db.GuildQueueItems.Where(x => x.BatchId == batch.Id && (x.State == QueueItemState.Queued || x.State == QueueItemState.Playing || x.State == QueueItemState.Starting))
                .ToListAsync(ct);

            if (active.Any(x => x.State == QueueItemState.Starting))
            {
                throw new InvalidOperationException("Unresolved start must be settled before closing a playback batch.");
            }

            foreach (var item in active)
            {
                item.State = QueueItemState.Skipped;
                item.SkippedAtUtc = DateTimeOffset.UtcNow;
            }

            state.IsRepeatingList = false;
            state.IsRepeating = false;
            db.GuildRepeatListItems.RemoveRange(await db.GuildRepeatListItems.Where(x => x.GuildId == guildId)
                .ToListAsync(ct));
        }

        batch.CompletedAtUtc = DateTimeOffset.UtcNow;
        state.ActiveBatchId = null;
        state.UpdatedAtUtc = DateTimeOffset.UtcNow;
        await db.SaveChangesAsync(ct);
        return true;
    }
}
