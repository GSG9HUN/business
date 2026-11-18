# Filters

Defines SDK-independent storage contracts for versioned guild-shared effect presets.

## Overview

- Scope every preset operation by guild ID and normalized name.
- Keep runtime players, output volume, SDK types and authorization policies outside persistence contracts.

## Interfaces

### IFilterPresetRepository

**Purpose:** SaveAsync inserts a name or explicitly overwrites it, returning false for an existing name without overwrite. GetAsync, ListAsync and DeleteAsync retain guild scope. List pages contain ten entries; saved capacity is not capped.

**Validation Checks:** The bot validates names, schema version and bounded built-in filter values before saving/applying. Manage Guild authorization belongs to PlaybackFilterService.

## Related Components

- `DC bot.Contracts/Interface/Service/Persistence/Models/Filters/FilterPresetRecord.cs` - Immutable stored definition.
- `DC bot.Persistence/Repositories/Filters/FilterPresetRepository.cs` - EF implementation.
