# localization

Contains the English and Hungarian message catalogs selected by LocalizationService.

## Overview

- Existing response titles and safe bodies now follow the supplied HTML wording, including playlist outcomes, pause/resume/ping/repeat, queue/effect labels and short enabled messages. Dynamic values stay parameterized; unsupported runtime states are not invented. English equivalents use matching placeholders.

- Saved-playlist rows accept order/title only. Compact nowplaying and music-status templates, lyrics track/page headings, committed operation counts and contextual empty/error titles have matching EN/HU placeholders. Searchpicker prompt body contains only numbered results.

- Status author labels, outcome-specific titles, help category headings and controls are localized in both catalogs. Dynamic values in selected volume, seek-target, playlist and queue results use Markdown emphasis. Category footers include the localized category name; prefix/slash guidance is displayed separately.

- Help example syntax is stored in 22 shared English/Hungarian templates with matching placeholders. Help labels use LocalizationKeys constants; nine unused legacy keys were removed from each catalog.

- Favorite usage documents only the registered American-English `favorite` entry point. Obsolete alternate-spelling usage and equivalent-entry help text are removed.

- Every text usage also documents its verified slash route and options. Confirmations, text-only defaults, one-based positions, quoted multiword names and decimal-point multipliers are explicit. Hungarian effect profiles use consistent vocabulary; executable keywords remain unchanged.

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

- Status titles use matching `response_*_title` keys for concrete outcomes and command contexts. Compact replies omit repeated status footers; the usage footer receives the configured prefix for its help command.
- Empty queue guidance receives the configured prefix and also shows the slash play command.
- 24/7 status describes the existing five-minute empty-channel cleanup policy; disabling it does not promise immediate leave at track completion.
- Every command and alias defines `{commandName}_command_usage`; `{0}` receives the configured prefix from `ResponseBuilder`.
- Description keys are selected through `ICommand.DescriptionKey`. The `help_v2_*` catalog entries supply seven category names/descriptions, selectors, navigation, registry/page counts, command-detail labels, prerequisites, examples and direct-lookup fallback guidance in both languages.
- `pagination_*` keys translate navigation, page/item counts, list titles and interaction failures; labels and errors use the requesting guild's language.
- Playback responses distinguish confirmed changes, rejected requests and outcomes awaiting confirmation; output volume and internal effect volume are separate values.
- Hungarian text preserves accented characters. User-facing guidance describes the result and next step without exposing audio-server or persistence terminology.
- The clear-command confirmation refers to the waiting queue in both catalogs.
- Pause/resume placeholders receive the captured track author and title.
- The `music_config_usage` alias remains available for callers using that identifier.

**Usage:**

```csharp
var title = localizationService.Get(guildId, LocalizationKeys.HistoryCommandTitle);
```

## Related Components

- `DC bot/README.md` — Parent feature overview and neighboring components.
- `DC bot/Startup/DependencyInjection/` — Registers the runtime implementations used by these components.
