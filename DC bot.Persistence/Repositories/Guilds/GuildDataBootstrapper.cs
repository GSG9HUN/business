using DC_bot.Db;
using DC_bot.Entities.Guilds;
using DC_bot.Repositories.Shared;
using Microsoft.EntityFrameworkCore;

namespace DC_bot.Repositories.Guilds;

/// <summary>
/// Ensures guild ownership rows exist within the caller&apos;s existing context and transaction.
/// </summary>
internal static class GuildDataBootstrapper
{

    /// <summary>
    /// Creates missing guild or settings state using the owning repository&apos;s defaults and unique-conflict policy.
    /// </summary>
    /// <param name = "dbContext">The caller&apos;s EF context, whose current transaction remains owned by the caller.</param>
    /// <param name = "guildId">The Discord guild whose persisted state is addressed.</param>
    /// <param name = "cancellationToken">Cancels supported database work and retry backoff; owned rollback uses its own deadline.</param>
    /// <returns>A task representing completion of the documented operation.</returns>
    internal static async Task EnsureExistsAsync(BotDbContext dbContext, ulong guildId, CancellationToken cancellationToken)
    {
        var exists = await dbContext.GuildData.AnyAsync(guild => guild.GuildId == guildId, cancellationToken);

        if (exists)
        {
            return;
        }

        dbContext.GuildData.Add(new GuildDataEntity { GuildId = guildId, IsPremium = false, UpdatedAtUtc = DateTimeOffset.UtcNow });
        await PostgreSqlConcurrencyHelper.SaveChangesIgnoringUniqueViolationAsync(dbContext, cancellationToken);
    }
}
