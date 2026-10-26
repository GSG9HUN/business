namespace DC_bot.Interface.Service.Persistence.Models.MobileAppUserSettings;

/// <summary>
/// Carries immutable mobile app user settings values across the shared storage or mobile-command boundary.
/// </summary>
/// <param name = "DiscordUserId">The discord user id value used by this storage contract.</param>
/// <param name = "LanguageCode">The language code value used by this storage contract.</param>
/// <param name = "Theme">The theme value used by this storage contract.</param>
/// <param name = "HapticFeedbackEnabled">The haptic feedback enabled value used by this storage contract.</param>
/// <param name = "SoundEffectsEnabled">The sound effects enabled value used by this storage contract.</param>
/// <param name = "TelemetryEnabled">The telemetry enabled value used by this storage contract.</param>
/// <param name = "UpdatedAtUtc">The updated at utc value used by this storage contract.</param>
public record MobileAppUserSettingsRecord(
    ulong DiscordUserId,
    string LanguageCode,
    string Theme,
    bool HapticFeedbackEnabled,
    bool SoundEffectsEnabled,
    bool TelemetryEnabled,
    DateTimeOffset UpdatedAtUtc);
