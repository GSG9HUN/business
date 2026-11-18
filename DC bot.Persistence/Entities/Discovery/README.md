# Discovery

Contains tracked rows for personal favorites and independent discovery opt-in preferences.

## Overview

- These rows reference Discord identifiers directly and do not require mobile registration.
- Runtime player objects, pending menus, recent autoplay history and counters are not persisted here.

## Entities

### FavoriteTrackEntity

**Purpose:** Stores stable ID, Discord UserId, a source identity hash, serialized TrackIdentifier, Title, Author and AddedAtUtc. The unique user/source key makes repeated saves idempotent.

---

### UserMusicSettingsEntity

**Purpose:** Stores the user's SearchPickerEnabled preference, default false.

---

### GuildMusicSettingsEntity

**Purpose:** Stores the guild's independent AutoplayEnabled preference, default false.

## Related Components

- `DC bot.Persistence/Configurations/Discovery/` - Table/key/default mappings.
- `DC bot.Persistence/Repositories/Discovery/` - EF access and owner filtering.
- `DC bot.Persistence/Migrations/20261117135456_AddMusicDiscovery.cs` - Schema changes.
