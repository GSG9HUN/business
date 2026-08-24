using DC_bot.Db;
using DC_bot.Entities.GuildBotStatus;
using DC_bot.Interface.Service.Persistence.GuildBotStatus;
using DC_bot.Interface.Service.Persistence.Models.GuildBotStatus;
using DC_bot.Repositories.Shared;
using Microsoft.EntityFrameworkCore;

namespace DC_bot.Repositories.GuildBotStatus;

public class GuildBotStatusRepository(IDbContextFactory<BotDbContext> dbContextFactory): IGuildBotStatusRepository
{
    public async Task UpsertConnectedVoiceAsync(ulong guildId, ulong voiceChannelId, string voiceChannelName, int voiceUserCount,
        CancellationToken cancellationToken = default)
    {
        await using var db = await dbContextFactory.CreateDbContextAsync(cancellationToken);

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

            var inserted = await PostgreSqlConcurrencyHelper.SaveChangesIgnoringUniqueViolationAsync(
                db,
                cancellationToken);
            if (inserted)
            {
                return;
            }

            guildBotStatusEntity = await db.GuildBotStatus.FirstAsync(x => x.GuildId == guildId, cancellationToken);
        }

        ApplyConnectedVoice(guildBotStatusEntity, voiceChannelName, voiceUserCount);
        await db.SaveChangesAsync(cancellationToken);
    }

    public async Task MarkDisconnectedVoiceAsync(ulong guildId, CancellationToken cancellationToken = default)
    {
        await using var db = await dbContextFactory.CreateDbContextAsync(cancellationToken);

        var guildBotStatusEntity = await db.GuildBotStatus.FirstOrDefaultAsync(x => x.GuildId == guildId, cancellationToken);
        if (guildBotStatusEntity is null)
        {
            return;
        }
            
        guildBotStatusEntity.ConnectedVoiceChannelName = null;
        guildBotStatusEntity.ConnectedVoiceUserCount = 0;
        guildBotStatusEntity.UpdatedAtUtc = DateTimeOffset.UtcNow;
            
        await db.SaveChangesAsync(cancellationToken);
    }

    public async Task<IReadOnlyDictionary<ulong, GuildBotStatusRecord>> GetByGuildIdsAsync(IReadOnlyCollection<ulong> guildIds, CancellationToken cancellationToken = default)
    {
        if (guildIds.Count == 0)
        {
            return new Dictionary<ulong, GuildBotStatusRecord>();
        }
        
        await using var db = await dbContextFactory.CreateDbContextAsync(cancellationToken);
        
        return await db.GuildBotStatus
            .AsNoTracking()
            .Where(x => guildIds.Contains(x.GuildId))
            .Select(x => new GuildBotStatusRecord(
                x.GuildId,
                x.ConnectedVoiceChannelName != null,
                x.ConnectedVoiceChannelName,
                x.ConnectedVoiceUserCount,
                x.UpdatedAtUtc))
            .ToDictionaryAsync(x => x.GuildId, cancellationToken);
        
    }

    private static void ApplyConnectedVoice(
        GuildBotStatusEntity entity,
        string voiceChannelName,
        int voiceUserCount)
    {
        entity.ConnectedVoiceChannelName = voiceChannelName;
        entity.ConnectedVoiceUserCount = voiceUserCount;
        entity.UpdatedAtUtc = DateTimeOffset.UtcNow;
    }
}
