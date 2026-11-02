# Localization

Defines translation lookup, formatting, and guild language selection.

## Overview

- LocalizationService loads translation resources and uses the file-system abstraction for persisted guild preferences.
- Visible responses should use keys from Constants and retain a fallback language. Repository storage and audio playback do not belong in this contract.

## Interfaces

### ILocalizationService

**File:** `DC bot/Interface/Service/Localization/ILocalizationService.cs`

**Purpose:** Defines the localization service boundary so callers are independent of its concrete implementation.

**Methods:**

- `Get` — Resolves a localized value for the supplied key and formatting arguments.

```csharp
string Get(ulong guildId, string key, params object[] args);
```

- `Get` — Resolves a localized value for the supplied key and formatting arguments.

```csharp
string Get(string key, params object[] args);
```

- `LoadLanguage` — Loads the guild's saved language preference and dictionary.

```csharp
void LoadLanguage(ulong guildId);
```

- `SaveLanguage` — Persists the guild's language preference and updates cached translations.

```csharp
void SaveLanguage(ulong guildId, string language);
```

**Usage:**

The caller supplies the component and argument values used in this example.

```csharp
var result = localizationService.Get(guildId, key, args);
```

---

## Related Components

- `DC bot/Interface/Service/README.md` — Parent feature overview and neighboring components.
- `DC bot/README.md` — Project setup and architecture.
- `DC bot/Startup/DependencyInjection/` — Registers the runtime implementations used by these components.
