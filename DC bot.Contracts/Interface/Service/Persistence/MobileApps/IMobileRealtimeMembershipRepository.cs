namespace DC_bot.Interface.Service.Persistence.MobileApps;

public interface IMobileRealtimeMembershipRepository
{
    Task UpsertGuildMembershipAsync(
        string connectionId,
        ulong discordUserId,
        ulong guildId,
        CancellationToken cancellationToken = default);

    Task RemoveGuildMembershipAsync(
        string connectionId,
        ulong discordUserId,
        ulong guildId,
        CancellationToken cancellationToken = default);

    Task RemoveConnectionMembershipsAsync(
        string connectionId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<ulong>> GetAuthorizedSubscribedUserIdsAsync(
        ulong guildId,
        CancellationToken cancellationToken = default);
}
