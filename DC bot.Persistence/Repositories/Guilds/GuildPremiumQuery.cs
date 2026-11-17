using DC_bot.Db;
using Microsoft.EntityFrameworkCore;

namespace DC_bot.Repositories.Guilds;

/// <summary>Checks active premium entitlement in the caller's database context and transaction.</summary>
internal static class GuildPremiumQuery
{

    /// <summary>Returns whether premium is enabled and has not expired.</summary>
    internal static Task<bool> IsActiveAsync(BotDbContext dbContext, ulong guildId, CancellationToken cancellationToken)
    {
        var now = DateTimeOffset.UtcNow;
        return dbContext.GuildData.AnyAsync(guild => guild.GuildId == guildId
            && guild.IsPremium
            && (guild.PremiumUntilUtc == null
                || guild.PremiumUntilUtc > now), cancellationToken);
    }
}
