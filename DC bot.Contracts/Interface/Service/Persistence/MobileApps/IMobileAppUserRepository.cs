using DC_bot.Interface.Service.Persistence.Models.MobileApps;

namespace DC_bot.Interface.Service.Persistence.MobileApps;

/// <summary>
/// Defines shared storage operations for mobile app user repository, including caller cancellation and immutable result contracts.
/// </summary>
public interface IMobileAppUserRepository
{

    /// <summary>
    /// Creates or updates the mobile user&apos;s Discord display metadata.
    /// </summary>
    /// <param name = "user">The user value used by this storage contract.</param>
    /// <param name = "ct">Cancels supported database work and retry backoff; owned rollback uses its own deadline.</param>
    /// <returns>A task representing completion of the documented operation.</returns>
    Task UpsertUserAsync(MobileAppUserUpsertRecord user, CancellationToken ct = default);

    /// <summary>
    /// Synchronizes the user&apos;s visible guild metadata using the shared PostgreSQL unique-conflict retry policy.
    /// </summary>
    /// <param name = "discordUserId">The discord user id value used by this storage contract.</param>
    /// <param name = "guilds">The guilds value used by this storage contract.</param>
    /// <param name = "ct">Cancels supported database work and retry backoff; owned rollback uses its own deadline.</param>
    /// <returns>A task representing completion of the documented operation.</returns>
    Task SyncUserGuildsAsync(
        ulong discordUserId,
        IReadOnlyCollection<MobileAppUserGuildUpsertRecord> guilds,
        CancellationToken ct = default);

    /// <summary>
    /// Reads the mobile user&apos;s stored profile, returning null when no profile exists.
    /// </summary>
    /// <param name = "discordUserId">The discord user id value used by this storage contract.</param>
    /// <param name = "ct">Cancels supported database work and retry backoff; owned rollback uses its own deadline.</param>
    /// <returns>The matching value, or null when no value satisfies the documented lookup.</returns>
    Task<MobileAppUserRecord?> GetUserAsync(ulong discordUserId, CancellationToken ct = default);

    /// <summary>
    /// Lists the guilds stored as visible to the requested mobile user.
    /// </summary>
    /// <param name = "discordUserId">The discord user id value used by this storage contract.</param>
    /// <param name = "ct">Cancels supported database work and retry backoff; owned rollback uses its own deadline.</param>
    /// <returns>The matching records in the documented order, or an empty collection when none match.</returns>
    Task<IReadOnlyList<MobileAppUserGuildRecord>> GetGuildsForUserAsync(ulong discordUserId, CancellationToken ct = default);

    /// <summary>
    /// Reads one visible guild for the mobile user, returning null when membership is absent.
    /// </summary>
    /// <param name = "discordUserId">The discord user id value used by this storage contract.</param>
    /// <param name = "guildId">The Discord guild whose persisted state is addressed.</param>
    /// <param name = "ct">Cancels supported database work and retry backoff; owned rollback uses its own deadline.</param>
    /// <returns>The matching value, or null when no value satisfies the documented lookup.</returns>
    Task<MobileAppUserGuildRecord?> GetGuildForUserAsync(ulong discordUserId, ulong guildId, CancellationToken ct = default);

    /// <summary>
    /// Checks persisted user-to-guild membership before authorizing guild-scoped operations.
    /// </summary>
    /// <param name = "discordUserId">The discord user id value used by this storage contract.</param>
    /// <param name = "guildId">The Discord guild whose persisted state is addressed.</param>
    /// <param name = "ct">Cancels supported database work and retry backoff; owned rollback uses its own deadline.</param>
    /// <returns>True when the documented check or conditional mutation succeeds; otherwise false.</returns>
    Task<bool> HasGuildAccessAsync(ulong discordUserId, ulong guildId, CancellationToken ct = default);
}
