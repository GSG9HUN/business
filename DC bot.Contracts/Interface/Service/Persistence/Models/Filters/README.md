# Filters

Contains immutable versioned effect preset values crossing the persistence boundary.

## Overview

- Definitions are opaque application JSON to persistence; no Lavalink SDK or bot executable dependency is introduced.

## Models

### FilterPresetRecord

**Purpose:** Carries Name, SchemaVersion, DefinitionJson, UpdatedBy and UpdatedAtUtc after guild-scoped lookup. Version one is a validated built-in filter map; player output volume is not part of the definition. Its volume filter field, when present, is internal gain compensation rather than output percentage.

## Related Components

- `DC bot.Contracts/Interface/Service/Persistence/Filters/IFilterPresetRepository.cs` - Producer.
- `DC bot/Service/Music/Playback/PlaybackFilterService.cs` - Validation, serialization and application.
