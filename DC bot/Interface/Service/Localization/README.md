# Localization Service Interfaces

This folder contains localization service interfaces.

## Files

### ILocalizationService.cs

**Purpose:** Multi-language support contract.

```csharp
public interface ILocalizationService
{
    string Get(string key, params object[] args);
    void LoadLanguage(ulong guildId);
    void SaveLanguage(ulong guildId, string language);
}
```

**Methods:**
- `Get()` - Retrieve localized string by key with optional formatting
- `LoadLanguage()` - Load language for guild
- `SaveLanguage()` - Save guild's language preference

**Implementation:** `Service/LocalizationService.cs`

**Usage:**
```csharp
// Get localized string
var description = localizationService.Get(LocalizationKeys.PlayCommandDescription);

// Get with formatting
var message = localizationService.Get("track_added", trackTitle, queuePosition);

// Change guild language
localizationService.SaveLanguage(guildId, "hu");
localizationService.LoadLanguage(guildId);
```

**Language Files:**
- `localization/eng.json` - English
- `localization/hu.json` - Hungarian
- `guildFiles/localization/{guildId}.json` - Guild preferences

---

## Related Components

- **Service/LocalizationService.cs** - Implementation
- **localization/** - Language files
- **Constants/LocalizationKeys.cs** - Key constants

