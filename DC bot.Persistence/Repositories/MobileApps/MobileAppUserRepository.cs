using DC_bot.Db;
using DC_bot.Entities.MobileApps;
using DC_bot.Interface.Service.Persistence.MobileApps;
using DC_bot.Interface.Service.Persistence.Models.MobileApps;
using DC_bot.Repositories.Shared;
using Microsoft.EntityFrameworkCore;

namespace DC_bot.Repositories.MobileApps;

public class MobileAppUserRepository(IDbContextFactory<BotDbContext> dbContextFactory) : IMobileAppUserRepository
{
    public async Task UpsertUserAsync(MobileAppUserUpsertRecord user, CancellationToken cancellationToken = default)
    {
        await using var dbContext = await dbContextFactory.CreateDbContextAsync(cancellationToken);
        var now = DateTimeOffset.UtcNow;

        var savedUser =
            await dbContext.MobileAppUsers.FirstOrDefaultAsync(u => u.DiscordUserId == user.DiscordUserId,
                cancellationToken);

        if (savedUser is null)
        {
            var newUser = new MobileAppUserEntity
            {
                DiscordUserId = user.DiscordUserId,
                Username = user.Username,
                GlobalName = user.GlobalName,
                AvatarHash = user.AvatarHash,
                CreatedAtUtc = now,
                LastLoginAtUtc = now
            };
            dbContext.MobileAppUsers.Add(newUser);

            var inserted = await PostgreSqlConcurrencyHelper.SaveChangesIgnoringUniqueViolationAsync(
                dbContext,
                cancellationToken);
            if (inserted)
            {
                return;
            }

            savedUser = await dbContext.MobileAppUsers.FirstAsync(
                u => u.DiscordUserId == user.DiscordUserId,
                cancellationToken);
        }

        ApplyUser(savedUser, user, now);
        await dbContext.SaveChangesAsync(cancellationToken);
    }


    public async Task SyncUserGuildsAsync(ulong discordUserId,
        IReadOnlyCollection<MobileAppUserGuildUpsertRecord> guilds, CancellationToken cancellationToken = default)
    {
        await PostgreSqlConcurrencyHelper.ExecuteWithUniqueViolationRetryAsync(
            ct => SyncUserGuildsOnceAsync(discordUserId, guilds, ct),
            cancellationToken);
    }

    private async Task SyncUserGuildsOnceAsync(ulong discordUserId,
        IReadOnlyCollection<MobileAppUserGuildUpsertRecord> guilds, CancellationToken cancellationToken)
    {
        await using var dbContext = await dbContextFactory.CreateDbContextAsync(cancellationToken);

        var incoming = guilds
            .GroupBy(g => g.GuildId)
            .ToDictionary(g => g.Key, g => g.Last());

        var guildIds = incoming.Keys.ToArray();

        var knownGuildIds = await dbContext.GuildData
            .Where(g => guildIds.Contains(g.GuildId))
            .Select(g => g.GuildId)
            .ToListAsync(cancellationToken);

        var knownSet = knownGuildIds.ToHashSet();

        var existing = await dbContext.UserGuilds
            .Where(g => g.DiscordUserId == discordUserId)
            .ToListAsync(cancellationToken);

        var now = DateTimeOffset.UtcNow;

        foreach (var row in existing)
        {
            if (!knownSet.Contains(row.GuildId))
            {
                dbContext.UserGuilds.Remove(row);
                continue;
            }

            ApplyUserGuild(row, incoming[row.GuildId], now);
        }

        var existingIds = existing.Select(x => x.GuildId).ToHashSet();
        foreach (var guildId in knownGuildIds)
        {
            if (existingIds.Contains(guildId))
            {
                continue;
            }

            var update = incoming[guildId];
            var newRow = new UserGuildEntity
            {
                DiscordUserId = discordUserId,
                GuildId = guildId,
                Permissions = update.Permissions,
                IsOwner = update.IsOwner,
                Name = update.Name,
                IconHash = update.IconHash,
                LastSeenAtUtc = now
            };
            dbContext.UserGuilds.Add(newRow);
        }

        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<MobileAppUserRecord?> GetUserAsync(ulong discordUserId,
        CancellationToken cancellationToken = default)
    {
        await using var db = await dbContextFactory.CreateDbContextAsync(cancellationToken);

        return await db.MobileAppUsers
            .AsNoTracking()
            .Where(x => x.DiscordUserId == discordUserId)
            .Select(x => new MobileAppUserRecord(
                x.DiscordUserId,
                x.Username,
                x.GlobalName,
                x.AvatarHash,
                x.CreatedAtUtc,
                x.LastLoginAtUtc))
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<MobileAppUserGuildRecord>> GetGuildsForUserAsync(ulong discordUserId,
        CancellationToken cancellationToken = default)
    {
        await using var dbContext = await dbContextFactory.CreateDbContextAsync(cancellationToken);

        return await dbContext.UserGuilds.AsNoTracking()
            .Where(g => g.DiscordUserId == discordUserId)
            .OrderBy(g => g.GuildId)
            .Select(g => new MobileAppUserGuildRecord(
                g.GuildId,
                g.Name,
                g.IconHash,
                g.Permissions,
                g.IsOwner,
                g.LastSeenAtUtc
            ))
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> HasGuildAccessAsync(ulong discordUserId, ulong guildId,
        CancellationToken cancellationToken = default)
    {
        await using var dbContext = await dbContextFactory.CreateDbContextAsync(cancellationToken);

        return await dbContext.UserGuilds.AsNoTracking()
            .AnyAsync(g => g.GuildId == guildId && g.DiscordUserId == discordUserId, cancellationToken);
    }

    private static void ApplyUser(
        MobileAppUserEntity entity,
        MobileAppUserUpsertRecord user,
        DateTimeOffset now)
    {
        entity.Username = user.Username;
        entity.GlobalName = user.GlobalName;
        entity.AvatarHash = user.AvatarHash;
        entity.LastLoginAtUtc = now;
    }

    private static void ApplyUserGuild(
        UserGuildEntity entity,
        MobileAppUserGuildUpsertRecord update,
        DateTimeOffset now)
    {
        entity.Permissions = update.Permissions;
        entity.IsOwner = update.IsOwner;
        entity.LastSeenAtUtc = now;
        entity.Name = update.Name;
        entity.IconHash = update.IconHash;
    }
}
