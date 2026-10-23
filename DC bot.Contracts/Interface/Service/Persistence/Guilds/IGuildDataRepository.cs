using DC_bot.Interface.Service.Persistence.Models.Guilds;

namespace DC_bot.Interface.Service.Persistence.Guilds;

/// <summary>
/// Defines shared storage operations for guild data repository, including caller cancellation and immutable result contracts.
/// </summary>
public interface IGuildDataRepository
{

    /// <summary>
    /// Ensures a default guild row exists while tolerating a concurrent insertion of the same guild.
    /// </summary>
    /// <param name = "guildId">The Discord guild whose persisted state is addressed.</param>
    /// <param name = "cancellationToken">Cancels supported database work and retry backoff; owned rollback uses its own deadline.</param>
    /// <returns>A task representing completion of the documented operation.</returns>
    Task EnsureGuildExistsAsync(ulong guildId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks that premium is enabled and has not expired at the time of the query.
    /// </summary>
    /// <param name = "guildId">The Discord guild whose persisted state is addressed.</param>
    /// <param name = "cancellationToken">Cancels supported database work and retry backoff; owned rollback uses its own deadline.</param>
    /// <returns>True when the documented check or conditional mutation succeeds; otherwise false.</returns>
    Task<bool> IsPremiumAsync(ulong guildId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates or updates the guild premium flags and expiry, recovering a competing insert through the shared conflict policy.
    /// </summary>
    /// <param name = "guildId">The Discord guild whose persisted state is addressed.</param>
    /// <param name = "isPremium">The is premium value used by this storage contract.</param>
    /// <param name = "premiumUntilUtc">The premium until utc value used by this storage contract.</param>
    /// <param name = "cancellationToken">Cancels supported database work and retry backoff; owned rollback uses its own deadline.</param>
    /// <returns>A task representing completion of the documented operation.</returns>
    Task UpsertPremiumAsync(
        ulong guildId,
        bool isPremium,
        DateTimeOffset? premiumUntilUtc,
        CancellationToken cancellationToken = default);
    
    
    /// <summary>
    /// Reads the guild's persistent voice connection preferences.
    /// </summary>
    /// <param name="guildId">The Discord guild whose preferences are requested.</param>
    /// <param name="cancellationToken">Cancels the database query.</param>
    /// <returns>The saved preferences, or disabled defaults when no guild row exists.</returns>
    Task<TwentyFourSevenSettingsRecord> GetTwentyFourSevenSettingsAsync(
        ulong guildId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates persistent voice mode while preserving preferred channels when disabling it.
    /// </summary>
    /// <param name="guildId">The Discord guild whose preferences are updated.</param>
    /// <param name="enabled">Whether persistent voice mode should be enabled.</param>
    /// <param name="voiceChannelId">The preferred voice channel when enabling the mode.</param>
    /// <param name="textChannelId">The preferred notification channel when enabling the mode.</param>
    /// <param name="cancellationToken">Cancels supported database operations.</param>
    /// <returns>A task that completes when the preferences have been saved.</returns>
    Task SetTwentyFourSevenSettingsAsync(
        ulong guildId,
        bool enabled,
        ulong? voiceChannelId,
        ulong? textChannelId,
        CancellationToken cancellationToken = default);
}
