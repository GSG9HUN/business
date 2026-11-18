# Filters

Stores durable, guild-shared custom effect definitions independently of runtime playback.

## Overview

- Preset names are unique within a guild.
- Leaving clears temporary player settings, not saved definitions.

## Entities

### FilterPresetEntity

**Purpose:** Stores GuildId/Name as the composite key, SchemaVersion and DefinitionJson, and UpdatedBy/UpdatedAtUtc audit values. No selected preset or current player output percentage is persisted here.

## Related Components

- `DC bot.Persistence/Configurations/Filters/FilterPresetConfiguration.cs` - PostgreSQL mapping.
- `DC bot.Persistence/Repositories/Filters/FilterPresetRepository.cs` - Reads and writes.
