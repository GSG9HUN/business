using DC_bot.BotControl;
using DC_bot.Db;
using DC_bot.Entities.Queue;
using DC_bot.Interface.Service.Persistence.Models.Queue;
using DC_bot.Repositories.Playback;
using DC_bot.Repositories.Shared;
using Microsoft.EntityFrameworkCore;

namespace DC_bot.Repositories.Queue;

/// <summary>
/// Coordinates canonical batch repetition, closure, and history through short-lived EF contexts.
/// </summary>
/// <param name = "factory">The injected factory used to create a fresh context for each operation or transaction attempt.</param>
internal sealed class QueueBatchRepository(IDbContextFactory<BotDbContext> factory)
{

    /// <summary>
    /// Enables repeat for the live canonical batch only when active items exist and track repeat is disabled.
    /// </summary>
    /// <param name = "guildId">The Discord guild whose persisted state is addressed.</param>
    /// <param name = "ct">Cancels supported database work and retry backoff; owned rollback uses its own deadline.</param>
    /// <returns>True when the documented check or conditional mutation succeeds; otherwise false.</returns>
    internal Task<bool> TryEnableRepeatAsync(ulong guildId, CancellationToken ct) => PostgreSqlConcurrencyHelper.ExecuteInSerializableTransactionWithRetryAsync(factory, async (db, token) =>
    {
        var batch = await QueueBatchStore.GetActiveAsync(db, guildId, create: false, token);

        if (batch is null)
        {
            return false;
        }

        var state = await db.GuildPlaybackStates.SingleAsync(x => x.GuildId == guildId, token);

        if (state.IsRepeating
            || !await QueueBatchStore.HasActiveItemsAsync(db, batch.Id, token))
        {
            return false;
        }

        state.IsRepeatingList = true;
        state.UpdatedAtUtc = DateTimeOffset.UtcNow;
        await db.SaveChangesAsync(token);
        await PlaybackRealtimeNotifier.NotifyPlaybackUpdatedAsync(db, guildId, MobileRealtimeEventNames.RepeatModeChanged, token);
        return true;
    }, ct);

    /// <summary>
    /// Reads canonical items of the active batch in canonical playback order.
    /// </summary>
    /// <param name = "guildId">The Discord guild whose persisted state is addressed.</param>
    /// <param name = "ct">Cancels supported database work and retry backoff; owned rollback uses its own deadline.</param>
    /// <returns>The matching records in the documented order, or an empty collection when none match.</returns>
    internal Task<IReadOnlyList<QueueItemToEnqueue>> GetItemsAsync(ulong guildId, CancellationToken ct) => PostgreSqlConcurrencyHelper.ExecuteInSerializableTransactionWithRetryAsync<IReadOnlyList<QueueItemToEnqueue>>(factory, async (db, token) =>
    {
        var batch = await QueueBatchStore.GetActiveAsync(db, guildId, create: false, token);

        if (batch is null)
        {
            return[];
        }

        return await db.PlaybackBatchItems.Where(x => x.BatchId == batch.Id)
            .OrderBy(x => x.Position)
            .Select(x => new QueueItemToEnqueue(x.TrackIdentifier, x.SourceQuery, x.SourceSearchMode, x.RequestedBy))
            .ToListAsync(token);
    }, ct);

    /// <summary>
    /// Re-enqueues the entire canonical batch only when list repeat is enabled and no active items remain.
    /// </summary>
    /// <param name = "guildId">The Discord guild whose persisted state is addressed.</param>
    /// <param name = "ct">Cancels supported database work and retry backoff; owned rollback uses its own deadline.</param>
    /// <returns>True when the documented check or conditional mutation succeeds; otherwise false.</returns>
    internal Task<bool> RestartAsync(ulong guildId, CancellationToken ct) => PostgreSqlConcurrencyHelper.ExecuteInSerializableTransactionWithRetryAsync(factory, async (db, token) =>
    {
        var batch = await QueueBatchStore.GetActiveAsync(db, guildId, create: false, token);

        if (batch is null
            || await QueueBatchStore.HasActiveItemsAsync(db, batch.Id, token))
        {
            return false;
        }

        var state = await db.GuildPlaybackStates.SingleAsync(x => x.GuildId == guildId, token);

        if (!state.IsRepeatingList)
        {
            return false;
        }

        var items = await db.PlaybackBatchItems.Where(x => x.BatchId == batch.Id)
            .OrderBy(x => x.Position)
            .ToListAsync(token);

        if (items.Count == 0)
        {
            return false;
        }

        var maxPosition = await db.GuildQueueItems.Where(x => x.GuildId == guildId)
            .Select(x => (int? )x.Position)
            .MaxAsync(token) ?? -1;
        var now = DateTimeOffset.UtcNow;
        db.GuildQueueItems.AddRange(items.Select((x, index) => new GuildQueueItemEntity { GuildId = guildId, BatchId = batch.Id, BatchItemId = x.Id, Position = checked(maxPosition + index + 1), TrackIdentifier = x.TrackIdentifier, SourceQuery = x.SourceQuery, SourceSearchMode = x.SourceSearchMode, RequestedBy = x.RequestedBy, State = QueueItemState.Queued, AddedAtUtc = now }));
        await db.SaveChangesAsync(token);
        await QueueRealtimeNotifier.NotifyQueueUpdatedAsync(db, guildId, MobileRealtimeEventNames.QueueItemsAdded, token);
        return true;
    }, ct);

    /// <summary>
    /// Closes an idle or explicitly left batch according to the caller&apos;s ownership and active-item policy.
    /// </summary>
    /// <param name = "guildId">The Discord guild whose persisted state is addressed.</param>
    /// <param name = "explicitLeave">Whether batch closure is requested by an explicit leave rather than idle completion.</param>
    /// <param name = "ct">Cancels supported database work and retry backoff; owned rollback uses its own deadline.</param>
    /// <returns>True when the documented check or conditional mutation succeeds; otherwise false.</returns>
    internal Task<bool> CloseAsync(ulong guildId, bool explicitLeave, CancellationToken ct) => PostgreSqlConcurrencyHelper.ExecuteInSerializableTransactionWithRetryAsync(factory, async (db, token) =>
    {
        var closed = await QueueBatchStore.CloseAsync(db, guildId, explicitLeave, token);

        if (closed && explicitLeave)
        {
            await QueueRealtimeNotifier.NotifyQueueUpdatedAsync(db, guildId, MobileRealtimeEventNames.QueueSnapshotChanged, token);
            await PlaybackRealtimeNotifier.NotifyPlaybackUpdatedAsync(db, guildId, MobileRealtimeEventNames.RepeatModeChanged, token);
        }

        return closed;
    }, ct);

    /// <summary>
    /// Reads recent playback batches with a bounded history limit and canonical track counts.
    /// </summary>
    /// <param name = "guildId">The Discord guild whose persisted state is addressed.</param>
    /// <param name = "limit">The requested history bound; the implementation clamps it to the supported range.</param>
    /// <param name = "ct">Cancels supported database work and retry backoff; owned rollback uses its own deadline.</param>
    /// <returns>The matching records in the documented order, or an empty collection when none match.</returns>
    internal async Task<IReadOnlyList<PlaybackBatchRecord>> GetRecentAsync(ulong guildId, int limit, CancellationToken ct)
    {
        await using var db = await factory.CreateDbContextAsync(ct);
        return await db.PlaybackBatches.AsNoTracking()
            .Where(x => x.GuildId == guildId)
            .OrderByDescending(x => x.Id)
            .Take(Math.Clamp(limit, 1, 25))
            .Select(x => new PlaybackBatchRecord(x.Id, x.CreatedAtUtc, x.CompletedAtUtc, db.PlaybackBatchItems.Count(item => item.BatchId == x.Id)))
            .ToListAsync(ct);
    }
}
