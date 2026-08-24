namespace DC_bot.Interface.Service.Persistence.Models.MobileAppUserSettings;

public sealed record MobileAppUserSettingsPatchRecord(
    string? LanguageCode,
    string? Theme,
    bool? HapticFeedbackEnabled,
    bool? SoundEffectsEnabled,
    bool? TelemetryEnabled)
{
    public bool HasChanges =>
        LanguageCode is not null ||
        Theme is not null ||
        HapticFeedbackEnabled is not null ||
        SoundEffectsEnabled is not null ||
        TelemetryEnabled is not null;
}