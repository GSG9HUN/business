# localization

Contains the English and Hungarian message catalogs selected by LocalizationService.

## Overview

- Both catalogs define matching `twenty_four_seven_command_*` keys for the command description, mode status, invalid argument, and update failure.
- Constants identify translation keys; ResponseBuilder and commands format the selected text using guild preferences and fallback lookup.
- Add matching keys and compatible formatting placeholders across catalogs. Translating visible text does not change command identifiers or repository contracts.

## Translation Catalogs

- `en.json` — Default English messages.
- `hu.json` — Hungarian messages.

**Usage:**

```csharp
var title = localizationService.Get(guildId, LocalizationKeys.HistoryCommandTitle);
```

## Related Components

- `DC bot/README.md` — Parent feature overview and neighboring components.
- `DC bot/Startup/DependencyInjection/` — Registers the runtime implementations used by these components.
