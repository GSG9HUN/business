# MobileAppUserSettings

Defines shared records and state values for per-user mobile language, theme, feedback, sound, and telemetry preferences.

## Overview

- Persistence maps entities into these values, and application services or API mappers consume them without database-provider dependencies.
- Keep wire response DTOs in API and tracked entities in DC bot.Persistence. Changing a record affects both repository implementations and their consumers.

## Classes

### MobileAppLanguageCode

**File:** `DC bot.Contracts/Interface/Service/Persistence/Models/MobileAppUserSettings/MobileAppLanguageCode.cs`

**Purpose:** Defines supported mobile language codes and their case-insensitive input normalization.

**Properties:**

```csharp
// The language codes accepted by application settings validation.
public static string SupportedValues { get; }
```

**Methods:**

- `TryNormalize` — Trims and validates playlist names or normalizes supported mobile language codes, returning false for unsupported input.

```csharp
public static bool TryNormalize(string? languageCode, out string normalized);
```

**Usage:**

The caller supplies the component and argument values used in this example.

```csharp
var result = MobileAppLanguageCode.TryNormalize(languageCode, out var normalized);
```

---

### MobileAppTheme

**File:** `DC bot.Contracts/Interface/Service/Persistence/Models/MobileAppUserSettings/MobileAppTheme.cs`

**Purpose:** Defines accepted mobile theme values and normalization for optional theme patches.

**Methods:**

- `IsValid` — Checks whether the supplied name or theme satisfies its documented contract constraints.

```csharp
public static bool IsValid(string value);
```

- `Normalize` — Trims and lowercases a mobile theme, returning null for blank input.

```csharp
public static string? Normalize(string? value);
```

**Usage:**

The caller supplies the component and argument values used in this example.

```csharp
var result = MobileAppTheme.IsValid(value);
```

---

## Models and Results

### MobileAppUserSettingsPatchRecord

**File:** `DC bot.Contracts/Interface/Service/Persistence/Models/MobileAppUserSettings/MobileAppUserSettingsPatchRecord.cs`

**Purpose:** Carries immutable mobile app user settings patch values across the shared storage or mobile-command boundary.

**Properties:**

```csharp
// Whether at least one optional settings field was supplied in this patch.
public bool HasChanges { get; }
```

**Record declaration:**

```csharp
public sealed record MobileAppUserSettingsPatchRecord(
    string? LanguageCode,
    string? Theme,
    bool? HapticFeedbackEnabled,
    bool? SoundEffectsEnabled,
    bool? TelemetryEnabled);
```

---

### MobileAppUserSettingsRecord

**File:** `DC bot.Contracts/Interface/Service/Persistence/Models/MobileAppUserSettings/MobileAppUserSettingsRecord.cs`

**Purpose:** Carries immutable mobile app user settings values across the shared storage or mobile-command boundary.

**Record declaration:**

```csharp
public record MobileAppUserSettingsRecord(
    ulong DiscordUserId,
    string LanguageCode,
    string Theme,
    bool HapticFeedbackEnabled,
    bool SoundEffectsEnabled,
    bool TelemetryEnabled,
    DateTimeOffset UpdatedAtUtc);
```

---

## Related Components

- `DC bot.Contracts/Interface/Service/Persistence/Models/README.md` — Parent feature overview and neighboring components.
- `DC bot.Contracts/README.md` — Project setup and architecture.
- `DC bot.Persistence/` — Repository implementations and database mappings that consume these shared contracts.
