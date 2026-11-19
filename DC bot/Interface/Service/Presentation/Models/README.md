# Models

Carries formatted pages and read-only loaders for Discord list navigation.

## Overview

- Describes visible content, page bounds and optional item totals.
- Preserves an explicit initial page and optional already loaded content.
- Keeps playback mutation and Discord message ownership outside the models.

## Models

### PaginationPage

**Purpose:** Carries the title, description, one-based number, page count and optional total item count.

**Properties:** `Title`, `Description`, `Number`, `PageCount`, `TotalItems`.

---

### PaginationRequest

**Purpose:** Carries a cancellable `LoadPageAsync` callback, `InitialPage` and optional `InitialContent`. A loader clamps requested pages within its captured user/guild scope; preloaded content avoids a duplicate initial query.

**Usage:**

```csharp
var request = new PaginationRequest(loader, InitialPage: 2, InitialContent: initialPage);
```

## Related Components

- `DC bot/Service/Presentation/DiscordPaginationService.cs` - Sends and owns expiring navigation panels.
- `DC bot/Interface/Service/Music/Playback/Models/PlaybackControlResult.cs` - Carries optional pagination alongside ordinary operation text.
