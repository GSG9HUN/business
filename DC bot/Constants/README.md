# Constants

Centralizes identifiers used to select localized messages and validation errors.

## Overview

- ResponseHtml* title constants identify twelve existing command outcomes and are actively routed by ResponseBuilder. They represent design text, not command aliases or new runtime branches.

- Design localization keys cover compact lists, outcome counts, precise list empty/error titles and verified/unverified Lavalink health. Canonical CommandNames are unchanged.

- Additional localization constants cover status author labels, operation/state titles, known validation outcomes and separate help prefix/slash labels. CommandNames remains the authoritative identity source; presentation changes do not rename commands.
- `CommandNames` centralizes all 50 canonical Discord text names, matching slash root names and internal dispatch targets. Durable `BotControlCommandTypes` IDs remain independent.


- Queue and import identifiers use `QueueCommand*` and `ImportPlaylistCommand*`; catalog keys use `queue_command_*` and `import_playlist_command_*`. Usage keys follow the actual `queue` and `importPlaylist` command names.

- Defines stable message and validation keys used by commands and services.
- Connects application outcomes to the English and Hungarian catalogs.
- Keeps message identifiers separate from translated text.

## Classes

### CommandNames

**File:** `CommandNames.cs`

**Purpose:** Supplies compile-time canonical Discord command names to text entries, matching slash roots, dispatch targets, help metadata and response titles. Slash group/subcommand vocabulary remains separate.

---

### LocalizationKeys

**File:** `DC bot/Constants/LocalizationKeys.cs`

**Purpose:** Contains all localization keys used throughout the application.

`ResponseSuccessTitle`, `ResponseWarningTitle`, `ResponseErrorTitle` and `ResponseUsageTitle` identify status embed titles. `Response*Title` keys also cover concrete outcomes and command contexts. Unused generic status footer keys have been removed; usage guidance retains its footer. `ResponseQueueEmptyHint` receives the configured prefix.

Keys cover help/usage, playback, filters, queues, playlists, discovery, voting, music status and 24/7. `Help*` constants select category metadata, selectors, detail labels, prerequisites and example templates; `HelpCommandUnknown` handles direct lookup failures. `MusicConfigUsage` selects `music-config_command_usage`. Language catalogs use matching placeholders. `Pagination*` keys cover navigation labels, result titles, page/item counts, expiry, ownership denials and page-load failures.

---

### ValidationErrorKeys

**File:** `DC bot/Constants/ValidationErrorKeys.cs`

**Purpose:** Contains all validation error keys used for user and system validation.

---

## Related Components

- `DC bot/README.md` — Parent feature overview and neighboring components.
- `DC bot/Startup/DependencyInjection/` — Registers the runtime implementations used by these components.
