# Discovery

Implements personal favorite and discovery preference persistence using DbContext.

## Overview

- Enforce user ownership in favorite reads, removals and playback lookup.
- Use serializable retry for concurrent favorite saves and preference creation.
- Preserve unrelated guild, playlist and mobile data.

## Repositories

### FavoriteRepository

**Purpose:** `AddAsync` returns the existing user/source favorite or inserts one; `GetAsync` and `RemoveAsync` include both user ID and favorite ID; `ListAsync` returns ten entries per page ordered by stable ID. `CountAsync` returns the owner-scoped total without loading track records. No saved-favorite count cap is introduced.

**Validation Checks:** Favorite IDs alone never authorize another user's record. The unique source key and transaction retry handle concurrent duplicate saves.

---

### DiscoverySettingsRepository

**Purpose:** `GetPickerAsync`/`SetPickerAsync` access personal search preference; `GetAutoplayAsync`/`SetAutoplayAsync` access independent guild autoplay. Missing rows read false; tracked assignments and SaveChangesAsync persist updates.

## Related Components

- `DC bot.Contracts/Interface/Service/Persistence/Discovery/` - Shared storage interfaces.
- `DC bot.Persistence/Entities/Discovery/` - Rows and keys.
- `DC bot/Service/Music/Discovery/` - Runtime command behavior and process-local budgets.
