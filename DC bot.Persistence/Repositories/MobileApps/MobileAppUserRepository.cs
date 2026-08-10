using DC_bot.Db;
using DC_bot.Entities.MobileApps;
using DC_bot.Interface.Service.Persistence.MobileApps;
using DC_bot.Interface.Service.Persistence.Models.MobileApps;
using Microsoft.EntityFrameworkCore;

namespace DC_bot.Repositories.MobileApps;

public class MobileAppUserRepository(IDbContextFactory<BotDbContext> dbContextFactory) : IMobileAppUserRepository
{
    public async Task UpsertUserAsync(MobileAppUserUpsertRecord user, CancellationToken cancellationToken = default)
    {
        await using var dbContext = await dbContextFactory.CreateDbContextAsync(cancellationToken);
        var transaction = await dbContext.Database.BeginTransactionAsync(cancellationToken);
        try
        {
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
                    CreatedAtUtc = DateTimeOffset.UtcNow,
                    LastLoginAtUtc = DateTimeOffset.UtcNow
                };
                dbContext.MobileAppUsers.Add(newUser);

                await dbContext.SaveChangesAsync(cancellationToken);
                await transaction.CommitAsync(cancellationToken);

                return;
            }

            savedUser.Username = user.Username;
            savedUser.GlobalName = user.GlobalName;
            savedUser.AvatarHash = user.AvatarHash;
            savedUser.LastLoginAtUtc = DateTimeOffset.UtcNow;

            await dbContext.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
        }
        catch (Exception)
        {
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }
    }


    public async Task SyncUserGuildsAsync(ulong discordUserId,
        IReadOnlyCollection<MobileAppUserGuildUpsertRecord> guilds, CancellationToken cancellationToken = default)
    {
        await using var dbContext = await dbContextFactory.CreateDbContextAsync(cancellationToken);
        var transaction = await dbContext.Database.BeginTransactionAsync(cancellationToken);
        try
        {
            //TODO kapott guild adatok
            var incoming = guilds
                .GroupBy(g => g.GuildId)
                .ToDictionary(g => g.Key, g => g.Last());

            //TODO ID-k
            var guildIds = incoming.Keys.ToArray();

            //TODO azok a guild adatok amelyeken a bot fent van...
            // kérdés kitöröljük-e ha leavel a bot?
            // TODO checkolni
            var knownGuildIds = await dbContext.GuildData
                .Where(g => guildIds.Contains(g.GuildId))
                .Select(g => g.GuildId)
                .ToListAsync(cancellationToken);

            var knownSet = knownGuildIds.ToHashSet();

            //TODO azok a guild adatok amelyeken a felhasználónak van hozzáférése
            var existing = await dbContext.UserGuilds
                .Where(g => g.DiscordUserId == discordUserId)
                .ToListAsync(cancellationToken);

            var now = DateTimeOffset.UtcNow;

            foreach (var row in existing)
            {
                //Ha azok a guild adatok amelyeken a felhasználónak van hozzáférése, de már a bot leavelt akkor töröljük
                if (!knownSet.Contains(row.GuildId))
                {
                    dbContext.UserGuilds.Remove(row);
                    continue;
                }

                var update = incoming[row.GuildId];
                row.Permissions = update.Permissions;
                row.IsOwner = update.IsOwner;
                row.LastSeenAtUtc = now;
            }

            var existingIds = existing.Select(x => x.GuildId).ToHashSet();
            foreach (var guildId in knownGuildIds)
            {
                if (existingIds.Contains(guildId)) continue;

                var update = incoming[guildId];
                var newRow = new UserGuildEntity
                {
                    DiscordUserId = discordUserId,
                    GuildId = guildId,
                    Permissions = update.Permissions,
                    IsOwner = update.IsOwner,
                    LastSeenAtUtc = now
                };
                dbContext.UserGuilds.Add(newRow);
            }

            await dbContext.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
        }
        catch (Exception)
        {
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }
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
}
