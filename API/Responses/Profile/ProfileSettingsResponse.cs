namespace API.Responses.Profile;

public sealed record ProfileSettingsResponse(
    string LanguageCode,
    string Theme,
    bool HapticFeedbackEnabled,
    bool SoundEffectsEnabled,
    bool TelemetryEnabled,
    DateTimeOffset UpdatedAtUtc);
