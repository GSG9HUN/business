# Discovery

Defines bot-local discovery contracts used by Discord commands and playback orchestration.

## Overview

- Defines lyrics, favorites, recommendations and search-selection boundaries.
- Exchanges playback results and caller context while implementations own provider and interaction handling.

## Interfaces

### ILyricsService

**Purpose:** Executes read-only query/current-track lyrics lookup and pagination through `ExecuteAsync`.

---

### IFavoriteService

**Purpose:** Handles personal add/list/remove/play commands through `ExecuteAsync`, enforcing owner-scoped lookup and normal playback validation.

---

### IRecommendationService

**Purpose:** Exposes recommendations, autoplay settings, bounded conditional enqueue, manual-request invalidation and recent ended-track observation. `TryEnqueueAsync` returns whether the repository confirmed an automatic insert; it does not dispatch remote playback.

---

### ISearchPickerHandler

**Purpose:** Stores the personal preference, owns expiring choice completion and validates Discord component events. `CancelPending` supersedes older user requests; `SelectAsync` never uses a fallback after an active menu is cancelled or expires.

## Related Components

- `DC bot/Service/Music/Discovery/` - Implementations and command behavior.
- `DC bot.Contracts/Interface/Service/Persistence/Discovery/` - SDK-independent storage contracts.
