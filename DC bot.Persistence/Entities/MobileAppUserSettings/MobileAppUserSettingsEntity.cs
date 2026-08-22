using DC_bot.Entities.MobileApps;

namespace DC_bot.Entities.MobileAppUserSettings;

public class MobileAppUserSettingsEntity
{
    public ulong DiscordUserId { get; set; }
    public string LanguageCode { get; set; } = String.Empty;
    public string Theme { get; set; } = String.Empty;
    public bool HapticFeedbackEnabled { get; set; }
    public bool SoundEffectsEnabled { get; set; }
    public bool TelemetryEnabled { get; set; }
    public DateTimeOffset UpdatedAtUtc { get; set; }

    public MobileAppUserEntity User { get; set; } = null!;
}