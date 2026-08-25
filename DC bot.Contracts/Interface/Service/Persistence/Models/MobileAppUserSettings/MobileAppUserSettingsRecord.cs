namespace DC_bot.Interface.Service.Persistence.Models.MobileAppUserSettings;

public record MobileAppUserSettingsRecord( 
    ulong DiscordUserId,
    string LanguageCode,
    string Theme,
    bool HapticFeedbackEnabled,
    bool SoundEffectsEnabled,
    bool TelemetryEnabled,
    DateTimeOffset UpdatedAtUtc);