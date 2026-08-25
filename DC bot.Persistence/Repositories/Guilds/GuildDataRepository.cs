using DC_bot.Db;
using DC_bot.Entities.Guilds;
using DC_bot.Interface.Service.Persistence.Guilds;
using DC_bot.Repositories.Shared;
using Microsoft.EntityFrameworkCore;

namespace DC_bot.Repositories.Guilds;

public class GuildDataRepository(IDbContextFactory<BotDbContext> dbContextFactory) : IGuildDataRepository
{
    public async Task EnsureGuildExistsAsync(ulong guildId, CancellationToken cancellationToken = default)
    {
        await using var dbContext = await dbContextFactory.CreateDbContextAsync(cancellationToken);

        var exists = await dbContext.GuildData.AnyAsync(g => g.GuildId == guildId, cancellationToken);
        if (exists)
        {
            return;
        }

        dbContext.GuildData.Add(new GuildDataEntity
        {
            GuildId = guildId,
            IsPremium = false,
            PremiumUntilUtc = null,
            UpdatedAtUtc = DateTimeOffset.UtcNow
        });

        await PostgreSqlConcurrencyHelper.SaveChangesIgnoringUniqueViolationAsync(dbContext, cancellationToken);
    }

    public async Task<bool> IsPremiumAsync(ulong guildId, CancellationToken cancellationToken = default)
    {
        await using var dbContext = await dbContextFactory.CreateDbContextAsync(cancellationToken);

        var now = DateTimeOffset.UtcNow;

        return await dbContext.GuildData
            .AnyAsync(g => g.GuildId == guildId && g.IsPremium && (g.PremiumUntilUtc == null || g.PremiumUntilUtc > now),
                cancellationToken);
    }

    public async Task UpsertPremiumAsync(
        ulong guildId,
        bool isPremium,
        DateTimeOffset? premiumUntilUtc,
        CancellationToken cancellationToken = default)
    {
        await using var dbContext = await dbContextFactory.CreateDbContextAsync(cancellationToken);

        var guild = await dbContext.GuildData.FirstOrDefaultAsync(g => g.GuildId == guildId, cancellationToken);

        if (guild is null)
        {
            guild = new GuildDataEntity
            {
                GuildId = guildId,
                IsPremium = isPremium,
                PremiumUntilUtc = premiumUntilUtc,
                UpdatedAtUtc = DateTimeOffset.UtcNow
            };

            dbContext.GuildData.Add(guild);

            var inserted = await PostgreSqlConcurrencyHelper.SaveChangesIgnoringUniqueViolationAsync(
                dbContext,
                cancellationToken);
            if (inserted)
            {
                return;
            }

            guild = await dbContext.GuildData.FirstAsync(g => g.GuildId == guildId, cancellationToken);
        }

        ApplyPremium(guild, isPremium, premiumUntilUtc);

        await dbContext.SaveChangesAsync(cancellationToken);
    }

    private static void ApplyPremium(
        GuildDataEntity guild,
        bool isPremium,
        DateTimeOffset? premiumUntilUtc)
    {
        guild.IsPremium = isPremium;
        guild.PremiumUntilUtc = premiumUntilUtc;
        guild.UpdatedAtUtc = DateTimeOffset.UtcNow;
    }
}
