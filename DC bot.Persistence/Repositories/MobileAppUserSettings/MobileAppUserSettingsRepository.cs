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

    public async Task<MobileAppUserSettingsRecord> UpdateAsync(
        MobileAppUserSettingsRecord settings,
        CancellationToken ct = default)
    {
        await using var db = await dbContextFactory.CreateDbContextAsync(ct);

        var entity = await db.MobileAppUserSettings
            .FirstOrDefaultAsync(x => x.DiscordUserId == settings.DiscordUserId, ct);

        if (entity is null)
        {
            entity = CreateDefault(settings.DiscordUserId);
            ApplySettings(entity, settings);
            db.MobileAppUserSettings.Add(entity);

            var inserted = await PostgreSqlConcurrencyHelper.SaveChangesIgnoringUniqueViolationAsync(db, ct);
            if (inserted)
            {
                return MapToRecord(entity);
            }

            entity = await db.MobileAppUserSettings
                .FirstAsync(x => x.DiscordUserId == settings.DiscordUserId, ct);
        }

        ApplySettings(entity, settings);
        await db.SaveChangesAsync(ct);

        return MapToRecord(entity);
    }

    private static void ApplySettings(
        MobileAppUserSettingsEntity entity,
        MobileAppUserSettingsRecord settings)
    {
        entity.LanguageCode = settings.LanguageCode;
        entity.Theme = settings.Theme;
        entity.HapticFeedbackEnabled = settings.HapticFeedbackEnabled;
        entity.SoundEffectsEnabled = settings.SoundEffectsEnabled;
        entity.TelemetryEnabled = settings.TelemetryEnabled;
        entity.UpdatedAtUtc = DateTimeOffset.UtcNow;
    }

    private static MobileAppUserSettingsEntity CreateDefault(ulong discordUserId) => new()
    {
        DiscordUserId = discordUserId,
        LanguageCode = "eng",
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
