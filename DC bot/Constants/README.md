# Constants

Centralizes identifiers used to select localized messages and validation errors.

## Overview

- Defines stable message and validation keys used by commands and services.
- Connects application outcomes to the English and Hungarian catalogs.
- Keeps message identifiers separate from translated text.

## Classes

### LocalizationKeys

**File:** `DC bot/Constants/LocalizationKeys.cs`

**Purpose:** Contains all localization keys used throughout the application.

Keys cover help/usage, playback, filters, queues, playlists, discovery, voting, music status and 24/7. `HelpCommandDetails` and `HelpCommandUnknown` support detailed help; `MusicConfigUsage` selects `music-config_command_usage`. Language catalogs use matching placeholders.

---

### ValidationErrorKeys

**File:** `DC bot/Constants/ValidationErrorKeys.cs`

**Purpose:** Contains all validation error keys used for user and system validation.

---

## Related Components

- `DC bot/README.md` — Parent feature overview and neighboring components.
- `DC bot/Startup/DependencyInjection/` — Registers the runtime implementations used by these components.
