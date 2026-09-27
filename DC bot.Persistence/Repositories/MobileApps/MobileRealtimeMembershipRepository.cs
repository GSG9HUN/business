using DC_bot.Db;
using DC_bot.Entities.MobileApps;
using DC_bot.Interface.Service.Persistence.MobileApps;
using Microsoft.EntityFrameworkCore;

namespace DC_bot.Repositories.MobileApps;

public class MobileRealtimeMembershipRepository(IDbContextFactory<BotDbContext> dbContextFactory)
    : IMobileRealtimeMembershipRepository
{
    public async Task UpsertGuildMembershipAsync(
        string connectionId,
        ulong discordUserId,
        ulong guildId,
        CancellationToken cancellationToken = default)
    {
        await using var dbContext = await dbContextFactory.CreateDbContextAsync(cancellationToken);

        var existing = await dbContext.MobileRealtimeGuildMemberships
            .FirstOrDefaultAsync(
                membership => membership.ConnectionId == connectionId && membership.GuildId == guildId,
                cancellationToken);

        if (existing is null)
        {
            dbContext.MobileRealtimeGuildMemberships.Add(new MobileRealtimeGuildMembershipEntity
            {
                ConnectionId = connectionId,
                DiscordUserId = discordUserId,
                GuildId = guildId,
                JoinedAtUtc = DateTimeOffset.UtcNow
            });
        }
        else
        {
            existing.DiscordUserId = discordUserId;
            existing.JoinedAtUtc = DateTimeOffset.UtcNow;
        }

        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task RemoveGuildMembershipAsync(
        string connectionId,
        ulong discordUserId,
        ulong guildId,
        CancellationToken cancellationToken = default)
    {
        await using var dbContext = await dbContextFactory.CreateDbContextAsync(cancellationToken);

        await dbContext.MobileRealtimeGuildMemberships
            .Where(membership =>
                membership.ConnectionId == connectionId &&
                membership.DiscordUserId == discordUserId &&
                membership.GuildId == guildId)
            .ExecuteDeleteAsync(cancellationToken);
    }

    public async Task RemoveConnectionMembershipsAsync(
        string connectionId,
        CancellationToken cancellationToken = default)
    {
        await using var dbContext = await dbContextFactory.CreateDbContextAsync(cancellationToken);

        await dbContext.MobileRealtimeGuildMemberships
            .Where(membership => membership.ConnectionId == connectionId)
            .ExecuteDeleteAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<ulong>> GetAuthorizedSubscribedUserIdsAsync(
        ulong guildId,
        CancellationToken cancellationToken = default)
    {
        await using var dbContext = await dbContextFactory.CreateDbContextAsync(cancellationToken);

        return await dbContext.MobileRealtimeGuildMemberships
            .AsNoTracking()
            .Where(membership => membership.GuildId == guildId)
            .Join(
                dbContext.UserGuilds.AsNoTracking(),
                membership => new { membership.DiscordUserId, membership.GuildId },
                access => new { access.DiscordUserId, access.GuildId },
                (membership, _) => membership.DiscordUserId)
            .Distinct()
            .ToListAsync(cancellationToken);
    }
}
