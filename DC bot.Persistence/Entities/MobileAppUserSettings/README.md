# MobileAppUserSettings

Represents the database rows used for per-user mobile language, theme, feedback, sound, and telemetry preferences.

## Overview

- Feature repositories read and mutate these values in short-lived EF contexts, then map results into DC bot.Contracts records.
- Entity properties describe persisted state. Keep HTTP response formatting, Discord objects, and playback transport behavior outside these types.

## Classes

### MobileAppUserSettingsEntity

**File:** `DC bot.Persistence/Entities/MobileAppUserSettings/MobileAppUserSettingsEntity.cs`

**Purpose:** Represents the persisted mobile app user settings row mapped by the EF Core model.

**Properties:**

```csharp
// The Discord user identifier.
public ulong DiscordUserId { get; set; }
public string LanguageCode { get; set; }
public string Theme { get; set; }
public bool HapticFeedbackEnabled { get; set; }
public bool SoundEffectsEnabled { get; set; }
public bool TelemetryEnabled { get; set; }
public DateTimeOffset UpdatedAtUtc { get; set; }
public MobileAppUserEntity User { get; set; }
```

---

## Related Components

- `DC bot.Persistence/Entities/README.md` — Parent feature overview and neighboring components.
- `DC bot.Persistence/README.md` — Project setup and architecture.
- `DC bot.Persistence/Entities/MobileApps/` — Represents the database rows used for mobile identities, guild access, refresh sessions, and realtime subscription membership.
- `DC bot.Contracts/Interface/Service/Persistence/` — Shared interfaces and immutable values implemented by persistence.
