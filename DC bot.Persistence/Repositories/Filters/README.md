# Filters

Persists guild-local custom presets using EF contexts and existing transaction retry helpers.

## Overview

- Every read, list, save and delete is guild-scoped.
- New saves never overwrite an existing name implicitly.
- Persistence does not apply audio or decide Discord permissions.

## Repositories

### FilterPresetRepository

**Purpose:** SaveAsync uses a serializable transaction and unique/serialization conflict retry to insert or explicitly overwrite a definition. Existing names return false without overwrite. GetAsync and DeleteAsync include GuildId and Name; ListAsync orders ten-entry pages by name without imposing a saved-preset capacity cap. CountAsync obtains a guild-scoped total directly from the database.

**Validation Checks:** PlaybackFilterService enforces normalized names, schema compatibility, numeric bounds and management permissions. Integration against PostgreSQL is still required to validate transaction behavior; EF InMemory coverage verifies guild-scoped queries and deletes only.

## Related Components

- `DC bot.Contracts/Interface/Service/Persistence/Filters/` - Repository contract.
- `DC bot.Persistence/Entities/Filters/` - Tracked definition.
- `DC bot/Service/Music/Playback/PlaybackFilterService.cs` - Authorized command behavior.
