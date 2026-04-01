using DC_bot.Interface.Service.Persistence;
using DC_bot.Persistence.Db;
using DC_bot.Persistence.Entities;
using Microsoft.EntityFrameworkCore;

namespace DC_bot.Persistence.Repositories;

public class GuildDataRepository(IDbContextFactory<BotDbContext> dbContextFactory) : IGuildDataRepository
{
    public async Task<bool> IsPremiumAsync(ulong guildId, CancellationToken cancellationToken = default)
    {
        await using var dbContext = await dbContextFactory.CreateDbContextAsync(cancellationToken);

        var id = ToDbGuildId(guildId);
        var now = DateTimeOffset.UtcNow;

        return await dbContext.GuildData
            .AnyAsync(g => g.GuildId == id && g.IsPremium && (g.PremiumUntilUtc == null || g.PremiumUntilUtc > now),
                cancellationToken);
    }

    public async Task UpsertPremiumAsync(
        ulong guildId,
        bool isPremium,
        DateTimeOffset? premiumUntilUtc,
        CancellationToken cancellationToken = default)
    {
        await using var dbContext = await dbContextFactory.CreateDbContextAsync(cancellationToken);
        var id = ToDbGuildId(guildId);

        var guild = await dbContext.GuildData.FirstOrDefaultAsync(g => g.GuildId == id, cancellationToken);

        if (guild is null)
        {
            guild = new GuildDataEntity
            {
                GuildId = id,
                IsPremium = isPremium,
                PremiumUntilUtc = premiumUntilUtc,
                UpdatedAtUtc = DateTimeOffset.UtcNow
            };

            dbContext.GuildData.Add(guild);
        }
        else
        {
            guild.IsPremium = isPremium;
            guild.PremiumUntilUtc = premiumUntilUtc;
            guild.UpdatedAtUtc = DateTimeOffset.UtcNow;
        }

        await dbContext.SaveChangesAsync(cancellationToken);
    }

    private static long ToDbGuildId(ulong guildId)
    {
        return checked((long)guildId);
    }
}
