# Filters

Maps guild-shared custom filter presets to PostgreSQL storage.

## Overview

- Reuse the numeric Discord identifier conversion and a guild/name composite key.
- Keep versioned definitions in jsonb and record the saving administrator and UTC timestamp.

## Configurations

### FilterPresetConfiguration

**Purpose:** Maps FilterPresetEntity to filter_preset. Names are bounded to 32 characters, definitions use jsonb, and the composite primary key enforces guild-local uniqueness.

## Related Components

- `DC bot.Persistence/Db/BotDbContext.cs` - Applies the configuration and exposes FilterPresets.
- `DC bot.Persistence/Migrations/20261117144346_AddFilterPresets.cs` - Schema creation.
