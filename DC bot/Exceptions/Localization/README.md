# Localization

Represents failures while loading or resolving translation resources.

## Overview

- LocalizationService supplies context about missing or invalid localization data for logging and caller handling.
- Keep file I/O and translation lookup behavior in the localization service; exception types only describe the failure.

## Classes

### LocalizationException

**File:** `DC bot/Exceptions/Localization/LocalizationException.cs`

**Purpose:** Exception thrown when localization operations fail.

**Implements / inherits:** `BotException`.

**Properties:**

```csharp
// The language code involved in the failed localization operation.
public string LanguageCode { get; }
```

---

## Related Components

- `DC bot/Exceptions/README.md` — Parent feature overview and neighboring components.
- `DC bot/README.md` — Project setup and architecture.
- `DC bot/Startup/DependencyInjection/` — Registers the runtime implementations used by these components.
