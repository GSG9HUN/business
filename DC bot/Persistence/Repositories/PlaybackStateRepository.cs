using DC_bot.Interface.Service.Persistence;
using DC_bot.Interface.Service.Persistence.Models;
using DC_bot.Persistence.Db;
using DC_bot.Persistence.Entities;
using Microsoft.EntityFrameworkCore;

namespace DC_bot.Persistence.Repositories;

public class PlaybackStateRepository(IDbContextFactory<BotDbContext> dbContextFactory) : IPlaybackStateRepository
{
    public async Task<PlaybackStateRecord> GetOrCreateAsync(ulong guildId, CancellationToken cancellationToken = default)
    {
        await using var dbContext = await dbContextFactory.CreateDbContextAsync(cancellationToken);

        await EnsureGuildDataExistsAsync(dbContext, guildId, cancellationToken);

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
                CurrentTrackIdentifier = null,
                UpdatedAtUtc = now
            };

            dbContext.GuildPlaybackStates.Add(state);
            await dbContext.SaveChangesAsync(cancellationToken);
        }

        return new PlaybackStateRecord(
            guildId,
            state.IsRepeating,
            state.IsRepeatingList,
            state.CurrentTrackIdentifier,
            state.QueueItemId,
            state.UpdatedAtUtc);
    }

    public async Task SetRepeatStateAsync(
        ulong guildId,
        bool isRepeating,
        bool isRepeatingList,
        CancellationToken cancellationToken = default)
    {
        await using var dbContext = await dbContextFactory.CreateDbContextAsync(cancellationToken);

        await EnsureGuildDataExistsAsync(dbContext, guildId, cancellationToken);

        var state = await dbContext.GuildPlaybackStates
            .FirstOrDefaultAsync(s => s.GuildId == guildId, cancellationToken);

        if (state is null)
        {
            state = new GuildPlaybackStateEntity
            {
                GuildId = guildId,
                IsRepeating = isRepeating,
                IsRepeatingList = isRepeatingList,
                UpdatedAtUtc = DateTimeOffset.UtcNow
            };
            dbContext.GuildPlaybackStates.Add(state);
        }
        else
        {
            state.IsRepeating = isRepeating;
            state.IsRepeatingList = isRepeatingList;
            state.UpdatedAtUtc = DateTimeOffset.UtcNow;
        }

        await dbContext.SaveChangesAsync(cancellationToken);
    }
    

    public async Task SetCurrentTrackAsync(
        ulong guildId,
        string? trackIdentifier,
        long? queueItemId,
        CancellationToken cancellationToken = default)
    {
        await using var dbContext = await dbContextFactory.CreateDbContextAsync(cancellationToken);

        await EnsureGuildDataExistsAsync(dbContext, guildId, cancellationToken);

        var state = await dbContext.GuildPlaybackStates
            .FirstOrDefaultAsync(s => s.GuildId == guildId, cancellationToken);

        if (state is null)
        {
            state = new GuildPlaybackStateEntity
            {
                GuildId = guildId,
                IsRepeating = false,
                IsRepeatingList = false,
                CurrentTrackIdentifier = trackIdentifier,
                QueueItemId = queueItemId,
                UpdatedAtUtc = DateTimeOffset.UtcNow
            };
            dbContext.GuildPlaybackStates.Add(state);
        }
        else
        {
            state.CurrentTrackIdentifier = trackIdentifier;
            state.QueueItemId = queueItemId;
            state.UpdatedAtUtc = DateTimeOffset.UtcNow;
        }

        await dbContext.SaveChangesAsync(cancellationToken);
    }
    private static async Task EnsureGuildDataExistsAsync(
        BotDbContext dbContext,
        ulong guildId,
        CancellationToken cancellationToken)
    {
        var exists = await dbContext.GuildData.AnyAsync(g => g.GuildId == guildId, cancellationToken);
        if (exists)
        {
            return;
        }

        dbContext.GuildData.Add(new GuildDataEntity
        {
            GuildId = guildId,
            IsPremium = false,
            UpdatedAtUtc = DateTimeOffset.UtcNow
        });

        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
