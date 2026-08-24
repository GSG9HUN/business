namespace API.Requests.Profile;

public sealed record UpdateProfileSettingsRequest(
    string? LanguageCode,
    string? Theme,
    bool? HapticFeedbackEnabled,
    bool? SoundEffectsEnabled,
    bool? TelemetryEnabled);
