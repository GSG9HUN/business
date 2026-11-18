# Discovery

Defines SDK-independent persistence contracts for personal music favorites and opt-in preferences.

## Overview

- Carry immutable records and cancellation through repository boundaries.
- Keep authorization ownership explicit in user-scoped favorite method arguments.

## Interfaces

### IFavoriteRepository

**Purpose:** Provides AddAsync, CountAsync, ListAsync, GetAsync and RemoveAsync. CountAsync counts only the requesting user's records for pagination totals. Add deduplicates user/source identity; read/delete require the user ID as well as favorite ID. Listing uses ten-entry pages without limiting saved capacity.

---

### IDiscoverySettingsRepository

**Purpose:** Stores personal search-picker and guild autoplay booleans independently. Missing preferences default false; these methods do not contain playback behavior.

## Related Components

- `DC bot.Contracts/Interface/Service/Persistence/Models/Discovery/FavoriteTrackRecord.cs` - Immutable favorite result.
- `DC bot.Persistence/Repositories/Discovery/` - EF implementations.
