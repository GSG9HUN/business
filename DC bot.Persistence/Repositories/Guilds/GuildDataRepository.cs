using DC_bot.Db;
using DC_bot.Entities.Guilds;
using DC_bot.Interface.Service.Persistence.Guilds;
using DC_bot.Interface.Service.Persistence.Models.Guilds;
using DC_bot.Repositories.Shared;
using Microsoft.EntityFrameworkCore;

namespace DC_bot.Repositories.Guilds;

/// <summary>
/// Reads and mutates guild data state using short-lived contexts from the injected EF factory.
/// </summary>
/// <param name = "dbContextFactory">The injected factory used to create a fresh context for each operation or transaction attempt.</param>
public class GuildDataRepository(IDbContextFactory<BotDbContext> dbContextFactory) : IGuildDataRepository
{

    /// <inheritdoc/>
    public async Task EnsureGuildExistsAsync(ulong guildId, CancellationToken cancellationToken = default)
    {
        await using var dbContext = await dbContextFactory.CreateDbContextAsync(cancellationToken);
        var exists = await dbContext.GuildData.AnyAsync(g => g.GuildId == guildId, cancellationToken);

        if (exists)
        {
            return;
        }

        dbContext.GuildData.Add(new GuildDataEntity { GuildId = guildId, IsPremium = false, PremiumUntilUtc = null, UpdatedAtUtc = DateTimeOffset.UtcNow });
        await PostgreSqlConcurrencyHelper.SaveChangesIgnoringUniqueViolationAsync(dbContext, cancellationToken);
    }

    /// <inheritdoc/>
    public async Task<bool> IsPremiumAsync(ulong guildId, CancellationToken cancellationToken = default)
    {
        await using var dbContext = await dbContextFactory.CreateDbContextAsync(cancellationToken);
        var now = DateTimeOffset.UtcNow;
        return await dbContext.GuildData.AnyAsync(g => g.GuildId == guildId && g.IsPremium && (g.PremiumUntilUtc == null || g.PremiumUntilUtc > now), cancellationToken);
    }

    /// <inheritdoc/>
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
            var inserted = await PostgreSqlConcurrencyHelper.SaveChangesIgnoringUniqueViolationAsync(dbContext, cancellationToken);

            if (inserted)
            {
                return;
            }

            guild = await dbContext.GuildData.FirstAsync(g => g.GuildId == guildId, cancellationToken);
        }

        ApplyPremium(guild, isPremium, premiumUntilUtc);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    /// <summary>
    /// Assigns the premium flag, optional expiry, and update timestamp to the guild entity.
    /// </summary>
    /// <param name = "guild">The guild value used by this storage contract.</param>
    /// <param name = "isPremium">The is premium value used by this storage contract.</param>
    /// <param name = "premiumUntilUtc">The premium until utc value used by this storage contract.</param>
    private static void ApplyPremium(GuildDataEntity guild, bool isPremium, DateTimeOffset? premiumUntilUtc)
    {
        guild.IsPremium = isPremium;
        guild.PremiumUntilUtc = premiumUntilUtc;
        guild.UpdatedAtUtc = DateTimeOffset.UtcNow;
    }
        
        
    /// <inheritdoc />
    public async Task<TwentyFourSevenSettingsRecord> GetTwentyFourSevenSettingsAsync(
        ulong guildId,
        CancellationToken cancellationToken = default)
    {
        await using var db = await dbContextFactory.CreateDbContextAsync(
            cancellationToken);

        return await db.GuildData.AsNoTracking()
            .Where(guild => guild.GuildId == guildId)
            .Select(guild => new TwentyFourSevenSettingsRecord(
                guild.IsTwentyFourSevenEnabled,
                guild.PreferredVoiceChannelId,
                guild.PreferredTextChannelId))
            .FirstOrDefaultAsync(cancellationToken)
            ?? new TwentyFourSevenSettingsRecord(false, null, null);
    }

    /// <inheritdoc />
    public async Task SetTwentyFourSevenSettingsAsync(
        ulong guildId,
        bool enabled,
        ulong? voiceChannelId,
        ulong? textChannelId,
        CancellationToken cancellationToken = default)
    {

        if (guildId == 0)
        {
            throw new ArgumentOutOfRangeException(nameof(guildId));
        }

        if (enabled
            && (voiceChannelId is null or 0
                || textChannelId is null or 0))
        {
            throw new ArgumentException(
                "Enabling twenty-four-seven mode requires voice and text channels.");
        }

        await EnsureGuildExistsAsync(guildId, cancellationToken);

        await using var db = await dbContextFactory.CreateDbContextAsync(
            cancellationToken);

        var guild = await db.GuildData.FirstAsync(guild => guild.GuildId == guildId, cancellationToken);
        guild.IsTwentyFourSevenEnabled = enabled;

        if (enabled)
        {
            guild.PreferredVoiceChannelId = voiceChannelId;
            guild.PreferredTextChannelId = textChannelId;
        }

        guild.UpdatedAtUtc = DateTimeOffset.UtcNow;
        await db.SaveChangesAsync(cancellationToken);
    }
}
