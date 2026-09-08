using DC_bot.Db;
using DC_bot.Entities.Playback;
using DC_bot.Interface.Service.Persistence.Models.Playback;
using DC_bot.Interface.Service.Persistence.Playback;
using DC_bot.Repositories.Guilds;
using DC_bot.Repositories.Shared;
using Microsoft.EntityFrameworkCore;

namespace DC_bot.Repositories.Playback;

public class PlaybackStateRepository(IDbContextFactory<BotDbContext> dbContextFactory) : IPlaybackStateRepository
{
    public async Task<PlaybackStateRecord> GetOrCreateAsync(ulong guildId, CancellationToken cancellationToken = default)
    {
        await using var dbContext = await dbContextFactory.CreateDbContextAsync(cancellationToken);

        await GuildDataBootstrapper.EnsureExistsAsync(dbContext, guildId, cancellationToken);

        var state = await dbContext.GuildPlaybackStates
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.GuildId == guildId, cancellationToken);

        if (state is null)
        {
            var now = DateTimeOffset.UtcNow;
            state = new GuildPlaybackStateEntity
            {
                GuildId = guildId,
                IsRepeating = false,
                IsRepeatingList = false,
                IsPaused = false,
                PositionSeconds = 0,
                PositionUpdatedAtUtc = null,
                CurrentTrackIdentifier = null,
                UpdatedAtUtc = now
            };

            dbContext.GuildPlaybackStates.Add(state);
            var inserted = await PostgreSqlConcurrencyHelper.SaveChangesIgnoringUniqueViolationAsync(
                dbContext,
                cancellationToken);
            if (!inserted)
            {
                state = await dbContext.GuildPlaybackStates
                    .AsNoTracking()
                    .FirstAsync(s => s.GuildId == guildId, cancellationToken);
            }
        }

        return new PlaybackStateRecord(
            guildId,
            state.IsRepeating,
            state.IsRepeatingList,
            state.CurrentTrackIdentifier,
            state.QueueItemId,
            state.UpdatedAtUtc,
            state.IsPaused,
            state.PositionSeconds,
            state.PositionUpdatedAtUtc);
    }

    public async Task SetRepeatStateAsync(
        ulong guildId,
        bool isRepeating,
        bool isRepeatingList,
        CancellationToken cancellationToken = default)
    {
        await using var dbContext = await dbContextFactory.CreateDbContextAsync(cancellationToken);

        await GuildDataBootstrapper.EnsureExistsAsync(dbContext, guildId, cancellationToken);

        var state = await dbContext.GuildPlaybackStates
            .FirstOrDefaultAsync(s => s.GuildId == guildId, cancellationToken);

        if (state is null)
        {
            state = new GuildPlaybackStateEntity
            {
                GuildId = guildId,
                IsRepeating = isRepeating,
                IsRepeatingList = isRepeatingList,
                IsPaused = false,
                PositionSeconds = 0,
                PositionUpdatedAtUtc = null,
                UpdatedAtUtc = DateTimeOffset.UtcNow
            };
            dbContext.GuildPlaybackStates.Add(state);

            var inserted = await PostgreSqlConcurrencyHelper.SaveChangesIgnoringUniqueViolationAsync(
                dbContext,
                cancellationToken);
            if (inserted)
            {
                return;
            }

            state = await dbContext.GuildPlaybackStates.FirstAsync(s => s.GuildId == guildId, cancellationToken);
        }

        ApplyRepeatState(state, isRepeating, isRepeatingList);

        await dbContext.SaveChangesAsync(cancellationToken);
    }
    

    public async Task SetCurrentTrackAsync(
        ulong guildId,
        string? trackIdentifier,
        long? queueItemId,
        CancellationToken cancellationToken = default)
    {
        await using var dbContext = await dbContextFactory.CreateDbContextAsync(cancellationToken);

        await GuildDataBootstrapper.EnsureExistsAsync(dbContext, guildId, cancellationToken);

        var state = await dbContext.GuildPlaybackStates
            .FirstOrDefaultAsync(s => s.GuildId == guildId, cancellationToken);

        if (state is null)
        {
            state = new GuildPlaybackStateEntity
            {
                GuildId = guildId,
                IsRepeating = false,
                IsRepeatingList = false,
                IsPaused = false,
                PositionSeconds = 0,
                PositionUpdatedAtUtc = trackIdentifier is null ? null : DateTimeOffset.UtcNow,
                CurrentTrackIdentifier = trackIdentifier,
                QueueItemId = queueItemId,
                UpdatedAtUtc = DateTimeOffset.UtcNow
            };
            dbContext.GuildPlaybackStates.Add(state);

            var inserted = await PostgreSqlConcurrencyHelper.SaveChangesIgnoringUniqueViolationAsync(
                dbContext,
                cancellationToken);
            if (inserted)
            {
                return;
            }

            state = await dbContext.GuildPlaybackStates.FirstAsync(s => s.GuildId == guildId, cancellationToken);
        }

        ApplyCurrentTrack(state, trackIdentifier, queueItemId);

        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task SetPlaybackPositionAsync(
        ulong guildId,
        TimeSpan position,
        bool isPaused,
        CancellationToken cancellationToken = default)
    {
        await using var dbContext = await dbContextFactory.CreateDbContextAsync(cancellationToken);

        await GuildDataBootstrapper.EnsureExistsAsync(dbContext, guildId, cancellationToken);

        var state = await dbContext.GuildPlaybackStates
            .FirstOrDefaultAsync(s => s.GuildId == guildId, cancellationToken);

        if (state is null)
        {
            state = new GuildPlaybackStateEntity
            {
                GuildId = guildId,
                IsRepeating = false,
                IsRepeatingList = false,
                IsPaused = isPaused,
                PositionSeconds = ToPositionSeconds(position),
                PositionUpdatedAtUtc = DateTimeOffset.UtcNow,
                UpdatedAtUtc = DateTimeOffset.UtcNow
            };
            dbContext.GuildPlaybackStates.Add(state);

            var inserted = await PostgreSqlConcurrencyHelper.SaveChangesIgnoringUniqueViolationAsync(
                dbContext,
                cancellationToken);
            if (inserted)
            {
                return;
            }

            state = await dbContext.GuildPlaybackStates.FirstAsync(s => s.GuildId == guildId, cancellationToken);
        }

        ApplyPlaybackPosition(state, position, isPaused);

        await dbContext.SaveChangesAsync(cancellationToken);
    }

    private static void ApplyRepeatState(
        GuildPlaybackStateEntity state,
        bool isRepeating,
        bool isRepeatingList)
    {
        state.IsRepeating = isRepeating;
        state.IsRepeatingList = isRepeatingList;
        state.UpdatedAtUtc = DateTimeOffset.UtcNow;
    }

    private static void ApplyCurrentTrack(
        GuildPlaybackStateEntity state,
        string? trackIdentifier,
        long? queueItemId)
    {
        state.CurrentTrackIdentifier = trackIdentifier;
        state.QueueItemId = queueItemId;
        state.IsPaused = false;
        state.PositionSeconds = 0;
        state.PositionUpdatedAtUtc = trackIdentifier is null ? null : DateTimeOffset.UtcNow;
        state.UpdatedAtUtc = DateTimeOffset.UtcNow;
    }

    private static void ApplyPlaybackPosition(
        GuildPlaybackStateEntity state,
        TimeSpan position,
        bool isPaused)
    {
        state.IsPaused = isPaused;
        state.PositionSeconds = ToPositionSeconds(position);
        state.PositionUpdatedAtUtc = DateTimeOffset.UtcNow;
        state.UpdatedAtUtc = DateTimeOffset.UtcNow;
    }

    private static int ToPositionSeconds(TimeSpan position)
    {
        if (position <= TimeSpan.Zero)
        {
            return 0;
        }

        return position.TotalSeconds >= int.MaxValue
            ? int.MaxValue
            : (int)position.TotalSeconds;
    }
}
