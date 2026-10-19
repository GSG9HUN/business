using DC_bot.BotControl;
using DC_bot.Db;
using DC_bot.Entities.Playback;
using DC_bot.Interface.Service.Persistence.Models.Queue;
using DC_bot.Repositories.Playback;
using DC_bot.Repositories.Shared;
using Microsoft.EntityFrameworkCore;

namespace DC_bot.Repositories.Queue;

internal sealed class QueueClaimService(IDbContextFactory<BotDbContext> factory)
{
public Task<QueueClaimResult> ClaimForStartAsync(
        ulong guildId, Guid operationId, long? queueItemId = null,
        CancellationToken cancellationToken = default)
    {
        if (operationId == Guid.Empty)
        {
            throw new ArgumentException("Operation ID is required.", nameof(operationId));
        }

        return PostgreSqlConcurrencyHelper.ExecuteInSerializableTransactionWithRetryAsync(
            factory, async (db, ct) =>
            {
                var pending = await db.GuildQueueItems.SingleOrDefaultAsync(
                    q => q.GuildId == guildId && q.State == QueueItemState.Starting, ct);
               
                if (pending is not null)
                {
                    return pending.PlaybackOperationId == operationId
                        ? new QueueClaimResult(QueueClaimStatus.Claimed,
                            new QueueStartClaim(operationId, QueueItemMapper.ToRecord(pending)))
                        : new QueueClaimResult(QueueClaimStatus.Busy);
                }

                var query = db.GuildQueueItems.Where(q => q.GuildId == guildId);
                var item = queueItemId.HasValue
                    ? await query.SingleOrDefaultAsync(q => q.Id == queueItemId.Value, ct)
                    : await query.Where(q => q.State == QueueItemState.Queued)
                        .OrderBy(q => q.Position).ThenBy(q => q.Id).FirstOrDefaultAsync(ct);
               
                if (item is null)
                {
                    return new QueueClaimResult(QueueClaimStatus.Empty);
                }

                item.StartPreviousState = item.State;
                item.StartPreviousOperationId = item.PlaybackOperationId;
                item.PlaybackOperationId = operationId;
                item.State = QueueItemState.Starting;
                item.PlaybackNodeAddress = null;
                item.PlaybackNodeSessionId = null;
                item.PlaybackTransportConfirmed = false;
                await db.SaveChangesAsync(ct);
                await QueueRealtimeNotifier.NotifyQueueUpdatedAsync(db, guildId, MobileRealtimeEventNames.QueueSnapshotChanged, ct);
              
                return new QueueClaimResult(QueueClaimStatus.Claimed,
                    new QueueStartClaim(operationId, QueueItemMapper.ToRecord(item)));
            }, cancellationToken);
    }

    public Task<bool> CompleteStartAsync(
        ulong guildId, long queueItemId, Guid operationId,
        string trackIdentifier, string realtimeEventName,
        bool clearSourceMetadata = false,
        CancellationToken cancellationToken = default)
    {
        return PostgreSqlConcurrencyHelper.ExecuteInSerializableTransactionWithRetryAsync(
            factory, async (db, ct) =>
            {
                var item = await db.GuildQueueItems.SingleOrDefaultAsync(q =>
                    q.Id == queueItemId && q.GuildId == guildId &&
                    q.PlaybackOperationId == operationId, ct);
                if (item is null) return false;
                if (item.State == QueueItemState.Playing) return true;
                if (item.State != QueueItemState.Starting) return false;

                var now = DateTimeOffset.UtcNow;
                var previous = await db.GuildQueueItems.Where(q =>
                    q.GuildId == guildId && q.Id != queueItemId &&
                    q.State == QueueItemState.Playing).ToListAsync(ct);
                foreach (var old in previous)
                {
                    old.State = QueueItemState.Skipped;
                    old.SkippedAtUtc = now;
                }
                item.State = QueueItemState.Playing;
                item.StartPreviousState = null;
                item.StartPreviousOperationId = null;
                item.TrackIdentifier = trackIdentifier;
                item.PlayedAtUtc = null;
                item.SkippedAtUtc = null;
                if (clearSourceMetadata)
                {
                    item.SourceQuery = null;
                    item.SourceSearchMode = null;
                }

                var state = await db.GuildPlaybackStates.SingleOrDefaultAsync(
                    p => p.GuildId == guildId, ct);
                if (state is null)
                {
                    state = new GuildPlaybackStateEntity { GuildId = guildId };
                    db.GuildPlaybackStates.Add(state);
                }
                state.CurrentTrackIdentifier = trackIdentifier;
                state.QueueItemId = queueItemId;
                state.IsPaused = false;
                state.PositionSeconds = 0;
                state.PositionUpdatedAtUtc = now;
                state.UpdatedAtUtc = now;
                await db.SaveChangesAsync(ct);
                await QueueRealtimeNotifier.NotifyQueueUpdatedAsync(
                    db, guildId, MobileRealtimeEventNames.QueueItemClaimed, ct);
                await PlaybackRealtimeNotifier.NotifyPlaybackUpdatedAsync(
                    db, guildId, realtimeEventName, ct);
                return true;
            }, cancellationToken);
    }

    public Task<bool> ReleaseStartAsync(
        ulong guildId, long queueItemId, Guid operationId, bool skip,
        CancellationToken cancellationToken = default)
    {
        return PostgreSqlConcurrencyHelper.ExecuteInSerializableTransactionWithRetryAsync(
            factory, async (db, ct) =>
            {
                var item = await db.GuildQueueItems.SingleOrDefaultAsync(q =>
                    q.GuildId == guildId && q.Id == queueItemId &&
                    q.State == QueueItemState.Starting &&
                    q.PlaybackOperationId == operationId, ct);
                if (item is null) return false;
                item.State = skip ? QueueItemState.Skipped :
                    item.StartPreviousState ?? QueueItemState.Queued;
                item.StartPreviousState = null;
                item.PlaybackOperationId = skip ? null : item.StartPreviousOperationId;
                item.StartPreviousOperationId = null;
                if (skip) item.SkippedAtUtc = DateTimeOffset.UtcNow;
                await db.SaveChangesAsync(ct);
                await QueueRealtimeNotifier.NotifyQueueUpdatedAsync(
                    db, guildId, MobileRealtimeEventNames.QueueSnapshotChanged, ct);
                return true;
            }, cancellationToken);
    }

    public Task<bool> CompleteAttemptAsync(
        ulong guildId, long queueItemId, Guid operationId, bool played,
        CancellationToken cancellationToken = default)
    {
        return PostgreSqlConcurrencyHelper.ExecuteInSerializableTransactionWithRetryAsync(
            factory, async (db, ct) =>
            {
                var item = await db.GuildQueueItems.SingleOrDefaultAsync(q =>
                    q.GuildId == guildId && q.Id == queueItemId &&
                    q.PlaybackOperationId == operationId &&
                    (q.State == QueueItemState.Playing || q.State == QueueItemState.Starting), ct);
                if (item is null) return false;
                item.State = played ? QueueItemState.Played : QueueItemState.Skipped;
                item.StartPreviousState = null;
                item.StartPreviousOperationId = null;
                if (played) item.PlayedAtUtc = DateTimeOffset.UtcNow;
                else item.SkippedAtUtc = DateTimeOffset.UtcNow;
                await db.SaveChangesAsync(ct);
                await QueueRealtimeNotifier.NotifyQueueUpdatedAsync(
                    db, guildId, MobileRealtimeEventNames.QueueSnapshotChanged, ct);
                return true;
            }, cancellationToken);
    }
}