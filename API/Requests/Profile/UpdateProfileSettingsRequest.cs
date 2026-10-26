namespace API.Requests.Profile;

/// <summary>
/// Carries client input for update profile settings processing; handlers and endpoint filters validate its values.
/// </summary>
/// <param name="LanguageCode">The language code value for this operation.</param>
/// <param name="Theme">The theme value for this operation.</param>
/// <param name="HapticFeedbackEnabled">The haptic feedback enabled value for this operation.</param>
/// <param name="SoundEffectsEnabled">The sound effects enabled value for this operation.</param>
/// <param name="TelemetryEnabled">The telemetry enabled value for this operation.</param>
public sealed record UpdateProfileSettingsRequest(
    string? LanguageCode,
    string? Theme,
    bool? HapticFeedbackEnabled,
    bool? SoundEffectsEnabled,
    bool? TelemetryEnabled);
