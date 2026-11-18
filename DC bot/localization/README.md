# localization

Contains the English and Hungarian message catalogs selected by LocalizationService.

## Overview

- Provides English and Hungarian command descriptions, usage, validation messages and operation results.
- Uses matching keys and formatting placeholders across both catalogs.
- Resolves guild language preferences through `LocalizationService`.

## Translation Catalogs

### en.json

**Purpose:** Supplies default English command descriptions, usage, validation and operation messages.

**Files:**

- `en.json`

---

### hu.json

**Purpose:** Supplies Hungarian translations with keys and formatting placeholders matching the English catalog.

**Files:**

- `hu.json`

---

### Catalog Formatting

**Purpose:** Resolves visible messages using the selected guild language.

**Validation Checks:**

- Every command and alias defines `{commandName}_command_usage`; `{0}` receives the configured prefix from `ResponseBuilder`.
- Description keys are selected through `ICommand.DescriptionKey`.
- Playback responses distinguish confirmed changes, rejection and reconciliation; output volume and internal filter gain are separate values.
- Pause/resume placeholders receive the captured track author and title.
- The `music_config_usage` alias remains available for callers using that identifier.

**Usage:**

```csharp
var title = localizationService.Get(guildId, LocalizationKeys.HistoryCommandTitle);
```

## Related Components

- `DC bot/README.md` — Parent feature overview and neighboring components.
- `DC bot/Startup/DependencyInjection/` — Registers the runtime implementations used by these components.
