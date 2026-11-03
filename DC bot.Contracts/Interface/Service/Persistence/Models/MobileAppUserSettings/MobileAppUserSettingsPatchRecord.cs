namespace DC_bot.Interface.Service.Persistence.Models.MobileAppUserSettings;

/// <summary>
/// Carries immutable mobile app user settings patch values across the shared storage or mobile-command boundary.
/// </summary>
/// <param name = "LanguageCode">The language code value used by this storage contract.</param>
/// <param name = "Theme">The theme value used by this storage contract.</param>
/// <param name = "HapticFeedbackEnabled">The haptic feedback enabled value used by this storage contract.</param>
/// <param name = "SoundEffectsEnabled">The sound effects enabled value used by this storage contract.</param>
/// <param name = "TelemetryEnabled">The telemetry enabled value used by this storage contract.</param>
public sealed record MobileAppUserSettingsPatchRecord(
    string? LanguageCode,
    string? Theme,
    bool? HapticFeedbackEnabled,
    bool? SoundEffectsEnabled,
    bool? TelemetryEnabled)
{

    /// <summary>
    /// Whether at least one optional settings field was supplied in this patch.
    /// </summary>
    public bool HasChanges => LanguageCode is not null || Theme is not null || HapticFeedbackEnabled is not null || SoundEffectsEnabled is not null || TelemetryEnabled is not null;
}
