using DC_bot.Interface.Service.Persistence.Models.MobileAppUserSettings;

namespace DC_bot.Interface.Service.Persistence.MobileAppUserSettings;

public interface IMobileAppUserSettingsRepository
{
    Task EnsureExistsAsync(ulong discordUserId, CancellationToken ct = default);

    Task<MobileAppUserSettingsRecord> GetOrCreateAsync(
        ulong discordUserId,
        CancellationToken ct = default);

    Task<MobileAppUserSettingsRecord> UpdateAsync(
        MobileAppUserSettingsRecord settings,
        CancellationToken ct = default);
}