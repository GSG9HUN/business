using DC_bot.BotControl;
using DC_bot.Db;
using DC_bot.Entities.GuildBotStatus;
using DC_bot.Interface.Service.Persistence.GuildBotStatus;
using DC_bot.Interface.Service.Persistence.Models.GuildBotStatus;
using DC_bot.Repositories.Shared;
using Microsoft.EntityFrameworkCore;

namespace DC_bot.Repositories.GuildBotStatus;

/// <summary>
/// Reads and mutates guild bot status state using short-lived contexts from the injected EF factory.
/// </summary>
/// <param name = "dbContextFactory">The injected factory used to create a fresh context for each operation or transaction attempt.</param>
public class GuildBotStatusRepository(IDbContextFactory<BotDbContext> dbContextFactory) : IGuildBotStatusRepository
{

    /// <inheritdoc/>
    public async Task UpsertConnectedVoiceAsync(
        ulong guildId,
        ulong voiceChannelId,
        string voiceChannelName,
        int voiceUserCount,
        CancellationToken cancellationToken = default)
    {
        await using var db = await dbContextFactory.CreateDbContextAsync(cancellationToken);
        await using var transaction = await db.Database.BeginTransactionAsync(cancellationToken);
        try
        {
            var guildBotStatusEntity = await db.GuildBotStatus.FirstOrDefaultAsync(x => x.GuildId == guildId, cancellationToken);

            if (guildBotStatusEntity is null)
            {
                guildBotStatusEntity = new GuildBotStatusEntity
                {
                    GuildId = guildId,
                    ConnectedVoiceChannelName = voiceChannelName,
                    ConnectedVoiceUserCount = voiceUserCount,
                    UpdatedAtUtc = DateTimeOffset.UtcNow
                };
                db.GuildBotStatus.Add(guildBotStatusEntity);
                var inserted = await PostgreSqlConcurrencyHelper.SaveChangesIgnoringUniqueViolationAsync(db, cancellationToken);

                if (inserted)
                {
                    await GuildBotStatusRealtimeNotifier.NotifyGuildBotStatusUpdatedAsync(db, guildId, MobileRealtimeEventNames.BotJoinedVoiceChannel, cancellationToken);
                    await transaction.CommitAsync(cancellationToken);
                    return;
                }

                guildBotStatusEntity = await db.GuildBotStatus.FirstAsync(x => x.GuildId == guildId, cancellationToken);
            }

            var eventName = GetConnectedVoiceEventName(guildBotStatusEntity, voiceChannelName, voiceUserCount);
            ApplyConnectedVoice(guildBotStatusEntity, voiceChannelName, voiceUserCount);
            await db.SaveChangesAsync(cancellationToken);
            await GuildBotStatusRealtimeNotifier.NotifyGuildBotStatusUpdatedAsync(db, guildId, eventName, cancellationToken);
            await transaction.CommitAsync(cancellationToken);
        }
        catch (Exception operationException)
        {
            await TransactionCleanup.TryRollbackAsync(transaction, operationException);
            throw;
        }
    }

    /// <inheritdoc/>
    public async Task MarkDisconnectedVoiceAsync(ulong guildId, CancellationToken cancellationToken = default)
    {
        await using var db = await dbContextFactory.CreateDbContextAsync(cancellationToken);
        await using var transaction = await db.Database.BeginTransactionAsync(cancellationToken);
        try
        {
            var guildBotStatusEntity = await db.GuildBotStatus.FirstOrDefaultAsync(x => x.GuildId == guildId, cancellationToken);

            if (guildBotStatusEntity is null)
            {
                await transaction.CommitAsync(cancellationToken);
                return;
            }

            var wasConnected = guildBotStatusEntity.ConnectedVoiceChannelName is not null;
            guildBotStatusEntity.ConnectedVoiceChannelName = null;
            guildBotStatusEntity.ConnectedVoiceUserCount = 0;
            guildBotStatusEntity.UpdatedAtUtc = DateTimeOffset.UtcNow;
            await db.SaveChangesAsync(cancellationToken);
            await GuildBotStatusRealtimeNotifier.NotifyGuildBotStatusUpdatedAsync(db, guildId, wasConnected ? MobileRealtimeEventNames.BotLeftVoiceChannel : MobileRealtimeEventNames.GuildBotStatusChanged, cancellationToken);
            await transaction.CommitAsync(cancellationToken);
        }
        catch (Exception operationException)
        {
            await TransactionCleanup.TryRollbackAsync(transaction, operationException);
            throw;
        }
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyDictionary<ulong, GuildBotStatusRecord>> GetByGuildIdsAsync(
        IReadOnlyCollection<ulong> guildIds,
        CancellationToken cancellationToken = default)
    {
        if (guildIds.Count == 0)
        {
            return new Dictionary<ulong, GuildBotStatusRecord>();
        }

        await using var db = await dbContextFactory.CreateDbContextAsync(cancellationToken);
        return await db.GuildBotStatus.AsNoTracking()
            .Where(x => guildIds.Contains(x.GuildId))
            .Select(x => new GuildBotStatusRecord(x.GuildId, x.ConnectedVoiceChannelName != null, x.ConnectedVoiceChannelName, x.ConnectedVoiceUserCount, x.UpdatedAtUtc))
            .ToDictionaryAsync(x => x.GuildId, cancellationToken);
    }

    /// <summary>
    /// Copies confirmed voice-channel identity and occupancy into persisted guild status.
    /// </summary>
    /// <param name = "entity">The entity value used by this storage contract.</param>
    /// <param name = "voiceChannelName">The voice channel name value used by this storage contract.</param>
    /// <param name = "voiceUserCount">The voice user count value used by this storage contract.</param>
    private static void ApplyConnectedVoice(GuildBotStatusEntity entity, string voiceChannelName, int voiceUserCount)
    {
        entity.ConnectedVoiceChannelName = voiceChannelName;
        entity.ConnectedVoiceUserCount = voiceUserCount;
        entity.UpdatedAtUtc = DateTimeOffset.UtcNow;
    }

    /// <summary>
    /// Selects the voice event corresponding to a first connection, channel move, or occupancy update.
    /// </summary>
    /// <param name = "entity">The entity value used by this storage contract.</param>
    /// <param name = "voiceChannelName">The voice channel name value used by this storage contract.</param>
    /// <param name = "voiceUserCount">The voice user count value used by this storage contract.</param>
    /// <returns>The mapped, normalized, or configured value described above.</returns>
    private static string GetConnectedVoiceEventName(GuildBotStatusEntity entity, string voiceChannelName, int voiceUserCount)
    {
        if (entity.ConnectedVoiceChannelName is null)
        {
            return MobileRealtimeEventNames.BotJoinedVoiceChannel;
        }

        if (
            !string.Equals(
                entity.ConnectedVoiceChannelName,
                voiceChannelName,
                StringComparison.Ordinal))
        {
            return MobileRealtimeEventNames.BotJoinedVoiceChannel;
        }

        if (entity.ConnectedVoiceUserCount != voiceUserCount)
        {
            return MobileRealtimeEventNames.BotVoiceUserCountChanged;
        }

        return MobileRealtimeEventNames.GuildBotStatusChanged;
    }
}
