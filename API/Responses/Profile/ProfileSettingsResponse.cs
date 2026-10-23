namespace API.Responses.Profile;

/// <summary>
/// Carries the profile settings payload returned to HTTP and mobile clients.
/// </summary>
/// <param name="LanguageCode">The language code value for this operation.</param>
/// <param name="Theme">The theme value for this operation.</param>
/// <param name="HapticFeedbackEnabled">The haptic feedback enabled value for this operation.</param>
/// <param name="SoundEffectsEnabled">The sound effects enabled value for this operation.</param>
/// <param name="TelemetryEnabled">The telemetry enabled value for this operation.</param>
/// <param name="UpdatedAtUtc">The updated at utc value for this operation.</param>
public sealed record ProfileSettingsResponse(
    string LanguageCode,
    string Theme,
    bool HapticFeedbackEnabled,
    bool SoundEffectsEnabled,
    bool TelemetryEnabled,
    DateTimeOffset UpdatedAtUtc);
