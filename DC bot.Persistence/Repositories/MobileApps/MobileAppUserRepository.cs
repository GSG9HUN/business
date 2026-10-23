using DC_bot.Db;
using DC_bot.Entities.MobileApps;
using DC_bot.Interface.Service.Persistence.MobileApps;
using DC_bot.Interface.Service.Persistence.Models.MobileApps;
using DC_bot.Repositories.Shared;
using Microsoft.EntityFrameworkCore;

namespace DC_bot.Repositories.MobileApps;

/// <summary>
/// Reads and mutates mobile app user state using short-lived contexts from the injected EF factory.
/// </summary>
/// <param name = "dbContextFactory">The injected factory used to create a fresh context for each operation or transaction attempt.</param>
public class MobileAppUserRepository(IDbContextFactory<BotDbContext> dbContextFactory) : IMobileAppUserRepository
{

    /// <inheritdoc/>
    public async Task UpsertUserAsync(MobileAppUserUpsertRecord user, CancellationToken cancellationToken = default)
    {
        await using var dbContext = await dbContextFactory.CreateDbContextAsync(cancellationToken);
        var now = DateTimeOffset.UtcNow;
        var savedUser = await dbContext.MobileAppUsers.FirstOrDefaultAsync(u => u.DiscordUserId == user.DiscordUserId, cancellationToken);

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
            var inserted = await PostgreSqlConcurrencyHelper.SaveChangesIgnoringUniqueViolationAsync(dbContext, cancellationToken);

            if (inserted)
            {
                return;
            }

            savedUser = await dbContext.MobileAppUsers.FirstAsync(u => u.DiscordUserId == user.DiscordUserId, cancellationToken);
        }

        ApplyUser(savedUser, user, now);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    /// <inheritdoc/>
    public async Task SyncUserGuildsAsync(
        ulong discordUserId,
        IReadOnlyCollection<MobileAppUserGuildUpsertRecord> guilds,
        CancellationToken cancellationToken = default)
    {
        await PostgreSqlConcurrencyHelper.ExecuteWithUniqueViolationRetryAsync(ct => SyncUserGuildsOnceAsync(discordUserId, guilds, ct), cancellationToken);
    }

    /// <summary>
    /// Reconciles one snapshot of the user&apos;s guild membership in a single persistence attempt.
    /// </summary>
    /// <param name = "discordUserId">The discord user id value used by this storage contract.</param>
    /// <param name = "guilds">The guilds value used by this storage contract.</param>
    /// <param name = "cancellationToken">Cancels supported database work and retry backoff; owned rollback uses its own deadline.</param>
    /// <returns>A task representing completion of the documented operation.</returns>
    private async Task SyncUserGuildsOnceAsync(
        ulong discordUserId,
        IReadOnlyCollection<MobileAppUserGuildUpsertRecord> guilds,
        CancellationToken cancellationToken)
    {
        await using var dbContext = await dbContextFactory.CreateDbContextAsync(cancellationToken);
        var incoming = guilds.GroupBy(g => g.GuildId)
            .ToDictionary(g => g.Key, g => g.Last());
        var guildIds = incoming.Keys.ToArray();
        var knownGuildIds = await dbContext.GuildData.Where(g => guildIds.Contains(g.GuildId))
            .Select(g => g.GuildId)
            .ToListAsync(cancellationToken);
        var knownSet = knownGuildIds.ToHashSet();
        var existing = await dbContext.UserGuilds.Where(g => g.DiscordUserId == discordUserId)
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

        var existingIds = existing.Select(x => x.GuildId)
            .ToHashSet();

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

    /// <inheritdoc/>
    public async Task<MobileAppUserRecord?> GetUserAsync(ulong discordUserId, CancellationToken cancellationToken = default)
    {
        await using var db = await dbContextFactory.CreateDbContextAsync(cancellationToken);
        return await db.MobileAppUsers.AsNoTracking()
            .Where(x => x.DiscordUserId == discordUserId)
            .Select(x => new MobileAppUserRecord(x.DiscordUserId, x.Username, x.GlobalName, x.AvatarHash, x.CreatedAtUtc, x.LastLoginAtUtc))
            .FirstOrDefaultAsync(cancellationToken);
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<MobileAppUserGuildRecord>> GetGuildsForUserAsync(
        ulong discordUserId,
        CancellationToken cancellationToken = default)
    {
        await using var dbContext = await dbContextFactory.CreateDbContextAsync(cancellationToken);
        return await dbContext.UserGuilds.AsNoTracking()
            .Where(g => g.DiscordUserId == discordUserId)
            .OrderBy(g => g.GuildId)
            .Select(g => new MobileAppUserGuildRecord(g.GuildId, g.Name, g.IconHash, g.Permissions, g.IsOwner, g.LastSeenAtUtc))
            .ToListAsync(cancellationToken);
    }

    /// <inheritdoc/>
    public async Task<MobileAppUserGuildRecord?> GetGuildForUserAsync(
        ulong discordUserId,
        ulong guildId,
        CancellationToken cancellationToken = default)
    {
        await using var dbContext = await dbContextFactory.CreateDbContextAsync(cancellationToken);
        return await dbContext.UserGuilds.AsNoTracking()
            .Where(g => g.DiscordUserId == discordUserId && g.GuildId == guildId)
            .Select(g => new MobileAppUserGuildRecord(g.GuildId, g.Name, g.IconHash, g.Permissions, g.IsOwner, g.LastSeenAtUtc))
            .FirstOrDefaultAsync(cancellationToken);
    }

    /// <inheritdoc/>
    public async Task<bool> HasGuildAccessAsync(ulong discordUserId, ulong guildId, CancellationToken cancellationToken = default)
    {
        await using var dbContext = await dbContextFactory.CreateDbContextAsync(cancellationToken);
        return await dbContext.UserGuilds.AsNoTracking()
            .AnyAsync(g => g.GuildId == guildId && g.DiscordUserId == discordUserId, cancellationToken);
    }

    /// <summary>
    /// Copies Discord user display metadata into a tracked mobile user entity.
    /// </summary>
    /// <param name = "entity">The entity value used by this storage contract.</param>
    /// <param name = "user">The user value used by this storage contract.</param>
    /// <param name = "now">The now value used by this storage contract.</param>
    private static void ApplyUser(MobileAppUserEntity entity, MobileAppUserUpsertRecord user, DateTimeOffset now)
    {
        entity.Username = user.Username;
        entity.GlobalName = user.GlobalName;
        entity.AvatarHash = user.AvatarHash;
        entity.LastLoginAtUtc = now;
    }

    /// <summary>
    /// Copies the supplied guild membership metadata into a tracked user-guild entity.
    /// </summary>
    /// <param name = "entity">The entity value used by this storage contract.</param>
    /// <param name = "update">The update value used by this storage contract.</param>
    /// <param name = "now">The now value used by this storage contract.</param>
    private static void ApplyUserGuild(UserGuildEntity entity, MobileAppUserGuildUpsertRecord update, DateTimeOffset now)
    {
        entity.Permissions = update.Permissions;
        entity.IsOwner = update.IsOwner;
        entity.LastSeenAtUtc = now;
        entity.Name = update.Name;
        entity.IconHash = update.IconHash;
    }
}
