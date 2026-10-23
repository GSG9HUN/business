# localization

Stores the language choice associated with an individual guild.

## Overview

- LocalizationService reads and writes these preferences and resolves messages from the shared translation catalog.
- Do not use this folder for queue snapshots or bot credentials. Keep runtime preference filenames out of fixed documentation inventories.

## Related Components

- `DC bot/guildFiles/README.md` — Parent feature overview and neighboring components.
- `DC bot/README.md` — Project setup and architecture.
- `DC bot/Startup/DependencyInjection/` — Registers the runtime implementations used by these components.
