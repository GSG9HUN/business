using DC_bot.Db;
using DC_bot.Entities.GuildBotStatus;
using DC_bot.Interface.Service.Persistence.GuildBotStatus;
using DC_bot.Interface.Service.Persistence.Models.GuildBotStatus;
using Microsoft.EntityFrameworkCore;

namespace DC_bot.Repositories.GuildBotStatus;

public class GuildBotStatusRepository(IDbContextFactory<BotDbContext> dbContextFactory): IGuildBotStatusRepository
{
    public async Task UpsertConnectedVoiceAsync(ulong guildId, ulong voiceChannelId, string voiceChannelName, int voiceUserCount,
        CancellationToken cancellationToken = default)
    {
        await using var db = await dbContextFactory.CreateDbContextAsync(cancellationToken);
        var transaction = await db.Database.BeginTransactionAsync(cancellationToken);
        try
        {
            var guildBotStatusEntity = await db.GuildBotStatus.FirstOrDefaultAsync(x => x.GuildId == guildId, cancellationToken);
            
            if(guildBotStatusEntity is not null)
            {
                guildBotStatusEntity.ConnectedVoiceChannelName = voiceChannelName;
                guildBotStatusEntity.ConnectedVoiceUserCount = voiceUserCount;
                guildBotStatusEntity.UpdatedAtUtc = DateTime.UtcNow;
                await db.SaveChangesAsync(cancellationToken);
                await transaction.CommitAsync(cancellationToken);
                return;
            }
            
            db.GuildBotStatus.Add(new GuildBotStatusEntity
            {
                GuildId = guildId,
                ConnectedVoiceChannelName = voiceChannelName,
                ConnectedVoiceUserCount = voiceUserCount,
                UpdatedAtUtc = DateTime.UtcNow
            });
            
            await db.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
        }
        catch (Exception )
        {
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }
    }

    public async Task MarkDisconnectedVoiceAsync(ulong guildId, CancellationToken cancellationToken = default)
    {
        await using var db = await dbContextFactory.CreateDbContextAsync(cancellationToken);
        var transaction = await db.Database.BeginTransactionAsync(cancellationToken);
        try
        {
            var guildBotStatusEntity = await db.GuildBotStatus.FirstOrDefaultAsync(x => x.GuildId == guildId, cancellationToken);
            if (guildBotStatusEntity is null)
            {
                return;
            }
            
            guildBotStatusEntity.ConnectedVoiceChannelName = null;
            guildBotStatusEntity.ConnectedVoiceUserCount = 0;
            guildBotStatusEntity.UpdatedAtUtc = DateTime.UtcNow; 
            
            await db.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
        }
        catch (Exception )
        {
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }
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
}