using DC_bot.Db;
using DC_bot.Entities.Guilds;
using Microsoft.EntityFrameworkCore;

namespace DC_bot.Repositories.Guilds;

internal static class GuildDataBootstrapper
{
    internal static async Task EnsureExistsAsync(
        BotDbContext dbContext,
        ulong guildId,
        CancellationToken cancellationToken)
    {
        var exists = await dbContext.GuildData.AnyAsync(guild => guild.GuildId == guildId, cancellationToken);
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
