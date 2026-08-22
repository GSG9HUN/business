using DC_bot.Db;
using DC_bot.Entities.MobileAppUserSettings;
using DC_bot.Interface.Service.Persistence.MobileAppUserSettings;
using DC_bot.Interface.Service.Persistence.Models.MobileAppUserSettings;
using DC_bot.Repositories.Shared;
using Microsoft.EntityFrameworkCore;

namespace DC_bot.Repositories.MobileAppUserSettings;

public class MobileAppUserSettingsRepository(IDbContextFactory<BotDbContext> dbContextFactory)
    : IMobileAppUserSettingsRepository
{
    public async Task EnsureExistsAsync(ulong discordUserId, CancellationToken ct = default)
    {
        await using var db = await dbContextFactory.CreateDbContextAsync(ct);

        var exists = await db.MobileAppUserSettings
            .AsNoTracking()
            .AnyAsync(x => x.DiscordUserId == discordUserId, ct);

        if (exists)
        {
            return;
        }

        db.MobileAppUserSettings.Add(CreateDefault(discordUserId));
        await PostgreSqlConcurrencyHelper.SaveChangesIgnoringUniqueViolationAsync(db, ct);
    }

    public async Task<MobileAppUserSettingsRecord> GetOrCreateAsync(ulong discordUserId, CancellationToken ct = default)
    {
        await using var db = await dbContextFactory.CreateDbContextAsync(ct);

        var entity = await db.MobileAppUserSettings
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.DiscordUserId == discordUserId, ct);

        if (entity is not null)
        {
            return MapToRecord(entity);
        }

        var created = CreateDefault(discordUserId);

        db.MobileAppUserSettings.Add(created);
        var inserted = await PostgreSqlConcurrencyHelper.SaveChangesIgnoringUniqueViolationAsync(db, ct);
        if (inserted)
        {
            return MapToRecord(created);
        }

        var existing = await db.MobileAppUserSettings
            .AsNoTracking()
            .FirstAsync(x => x.DiscordUserId == discordUserId, ct);

        return MapToRecord(existing);
    }

    public async Task<MobileAppUserSettingsRecord> PatchAsync(
        ulong discordUserId,
        MobileAppUserSettingsPatchRecord patch,
        CancellationToken ct = default)
    {
        if (!patch.HasChanges)
        {
            return await GetOrCreateAsync(discordUserId, ct);
        }

        await EnsureExistsAsync(discordUserId, ct);

        await using var db = await dbContextFactory.CreateDbContextAsync(ct);

        var now = DateTimeOffset.UtcNow;

        await db.MobileAppUserSettings
            .Where(x => x.DiscordUserId == discordUserId)
            .ExecuteUpdateAsync(setters => setters
                    .SetProperty(x => x.LanguageCode, x => patch.LanguageCode ?? x.LanguageCode)
                    .SetProperty(x => x.Theme, x => patch.Theme ?? x.Theme)
                    .SetProperty(x => x.HapticFeedbackEnabled, x => patch.HapticFeedbackEnabled ?? x.HapticFeedbackEnabled)
                    .SetProperty(x => x.SoundEffectsEnabled, x => patch.SoundEffectsEnabled ?? x.SoundEffectsEnabled)
                    .SetProperty(x => x.TelemetryEnabled, x => patch.TelemetryEnabled ?? x.TelemetryEnabled)
                    .SetProperty(x => x.UpdatedAtUtc, _ => now),
                ct);

        var entity = await db.MobileAppUserSettings
            .AsNoTracking()
            .FirstAsync(x => x.DiscordUserId == discordUserId, ct);

        return MapToRecord(entity);
    }

    private static MobileAppUserSettingsEntity CreateDefault(ulong discordUserId) => new()
    {
        DiscordUserId = discordUserId,
        LanguageCode = MobileAppLanguageCode.English,
        Theme = MobileAppTheme.System,
        HapticFeedbackEnabled = true,
        SoundEffectsEnabled = true,
        TelemetryEnabled = false,
        UpdatedAtUtc = DateTimeOffset.UtcNow
    };

    private static MobileAppUserSettingsRecord MapToRecord(MobileAppUserSettingsEntity entity) =>
        new(
            entity.DiscordUserId,
            entity.LanguageCode,
            entity.Theme,
            entity.HapticFeedbackEnabled,
            entity.SoundEffectsEnabled,
            entity.TelemetryEnabled,
            entity.UpdatedAtUtc);
}
