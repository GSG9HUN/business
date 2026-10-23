# guildFiles

Groups local files used for guild-specific preferences.

## Overview

- The localization subfolder holds guild language selections managed by LocalizationService through IFileSystem.
- Queue, playback, saved playlists, and mobile session state live in PostgreSQL, not in this directory. Treat generated guild preference files as runtime data.

## Subfolders

### localization/

Stores the language choice associated with an individual guild.

**Files:**

- `README.md` — Folder overview and navigation to its source components.

**Purpose:** LocalizationService reads and writes these preferences and resolves messages from the shared translation catalog.

---

## Related Components

- `DC bot/README.md` — Parent feature overview and neighboring components.
- `DC bot/guildFiles/localization/README.md` — Detailed component contracts and examples for this subfolder.
- `DC bot/Startup/DependencyInjection/` — Registers the runtime implementations used by these components.
