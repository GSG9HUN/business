using DC_bot.Interface.Service.Persistence.Models.MobileAppUserSettings;

namespace DC_bot.Interface.Service.Persistence.MobileAppUserSettings;

/// <summary>
/// Defines shared storage operations for mobile app user settings repository, including caller cancellation and immutable result contracts.
/// </summary>
public interface IMobileAppUserSettingsRepository
{

    /// <summary>
    /// Creates missing guild or settings state using the owning repository&apos;s defaults and unique-conflict policy.
    /// </summary>
    /// <param name = "discordUserId">The discord user id value used by this storage contract.</param>
    /// <param name = "ct">Cancels supported database work and retry backoff; owned rollback uses its own deadline.</param>
    /// <returns>A task representing completion of the documented operation.</returns>
    Task EnsureExistsAsync(ulong discordUserId, CancellationToken ct = default);

    /// <summary>
    /// Reads the guild playback state or mobile-user settings, creating documented defaults when missing.
    /// </summary>
    /// <param name = "discordUserId">The discord user id value used by this storage contract.</param>
    /// <param name = "ct">Cancels supported database work and retry backoff; owned rollback uses its own deadline.</param>
    /// <returns>A task yielding the persisted or mapped result described above.</returns>
    Task<MobileAppUserSettingsRecord> GetOrCreateAsync(ulong discordUserId, CancellationToken ct = default);

    /// <summary>
    /// Applies only supplied settings fields, preserving fields omitted from the patch.
    /// </summary>
    /// <param name = "discordUserId">The discord user id value used by this storage contract.</param>
    /// <param name = "patch">The patch value used by this storage contract.</param>
    /// <param name = "ct">Cancels supported database work and retry backoff; owned rollback uses its own deadline.</param>
    /// <returns>A task yielding the persisted or mapped result described above.</returns>
    Task<MobileAppUserSettingsRecord> PatchAsync(
        ulong discordUserId,
        MobileAppUserSettingsPatchRecord patch,
        CancellationToken ct = default);
}
