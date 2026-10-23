using DC_bot.Entities.MobileApps;

namespace DC_bot.Entities.MobileAppUserSettings;

/// <summary>
/// Represents the persisted mobile app user settings row mapped by the EF Core model.
/// </summary>
public class MobileAppUserSettingsEntity
{

    /// <summary>
    /// The Discord user identifier.
    /// </summary>
    public ulong DiscordUserId { get; set; }

    /// <summary>
    /// The language code used for localized application content.
    /// </summary>
    public string LanguageCode { get; set; } = String.Empty;

    /// <summary>
    /// The selected application appearance mode.
    /// </summary>
    public string Theme { get; set; } = String.Empty;

    /// <summary>
    /// Whether haptic feedback is enabled in the application.
    /// </summary>
    public bool HapticFeedbackEnabled { get; set; }

    /// <summary>
    /// Whether application sound effects are enabled.
    /// </summary>
    public bool SoundEffectsEnabled { get; set; }

    /// <summary>
    /// Whether application telemetry is enabled.
    /// </summary>
    public bool TelemetryEnabled { get; set; }

    /// <summary>
    /// The UTC time of the most recent persisted update.
    /// </summary>
    public DateTimeOffset UpdatedAtUtc { get; set; }

    /// <summary>
    /// The user associated with this object.
    /// </summary>
    public MobileAppUserEntity User { get; set; } = null !;
}
