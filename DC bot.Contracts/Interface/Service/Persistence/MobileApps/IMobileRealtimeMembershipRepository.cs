namespace DC_bot.Interface.Service.Persistence.MobileApps;

/// <summary>
/// Defines shared storage operations for mobile realtime membership repository, including caller cancellation and immutable result contracts.
/// </summary>
public interface IMobileRealtimeMembershipRepository
{

    /// <summary>
    /// Records a realtime connection&apos;s guild subscription and user identity.
    /// </summary>
    /// <param name = "connectionId">The connection id value used by this storage contract.</param>
    /// <param name = "discordUserId">The discord user id value used by this storage contract.</param>
    /// <param name = "guildId">The Discord guild whose persisted state is addressed.</param>
    /// <param name = "cancellationToken">Cancels supported database work and retry backoff; owned rollback uses its own deadline.</param>
    /// <returns>A task representing completion of the documented operation.</returns>
    Task UpsertGuildMembershipAsync(
        string connectionId,
        ulong discordUserId,
        ulong guildId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Removes the realtime connection&apos;s subscription to the selected guild.
    /// </summary>
    /// <param name = "connectionId">The connection id value used by this storage contract.</param>
    /// <param name = "discordUserId">The discord user id value used by this storage contract.</param>
    /// <param name = "guildId">The Discord guild whose persisted state is addressed.</param>
    /// <param name = "cancellationToken">Cancels supported database work and retry backoff; owned rollback uses its own deadline.</param>
    /// <returns>A task representing completion of the documented operation.</returns>
    Task RemoveGuildMembershipAsync(
        string connectionId,
        ulong discordUserId,
        ulong guildId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Removes every persisted guild subscription for the disconnected realtime connection.
    /// </summary>
    /// <param name = "connectionId">The connection id value used by this storage contract.</param>
    /// <param name = "cancellationToken">Cancels supported database work and retry backoff; owned rollback uses its own deadline.</param>
    /// <returns>A task representing completion of the documented operation.</returns>
    Task RemoveConnectionMembershipsAsync(string connectionId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns subscribed user identifiers that still have persisted access to the requested guild.
    /// </summary>
    /// <param name = "guildId">The Discord guild whose persisted state is addressed.</param>
    /// <param name = "cancellationToken">Cancels supported database work and retry backoff; owned rollback uses its own deadline.</param>
    /// <returns>The matching records in the documented order, or an empty collection when none match.</returns>
    Task<IReadOnlyList<ulong>> GetAuthorizedSubscribedUserIdsAsync(ulong guildId, CancellationToken cancellationToken = default);
}
