# Models

Carries formatted pages, read-only loaders and typed help state for Discord navigation.

## Overview

- Describes visible content, page bounds and optional item totals.
- Preserves an explicit initial page and optional already loaded content.
- Keeps playback mutation and Discord message ownership outside the models.
- Keeps help metadata and navigation separate from generic list loaders; no model holds a Discord renderer or encoded navigation string.

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

---

### HelpResponse

**Purpose:** Captures guild scope, a read-only command catalog, typed navigation, formatted page and localized footer for a help session.

---

### HelpCommandMetadata

**Purpose:** Stores the registered name and description key with category, prerequisites key and localized example-template key.

---

### HelpNavigationState

**Purpose:** Retains `View`, optional `Category`, one-based `CategoryPage` and optional `CommandName`. Details use their own page bounds while retaining the category return page.

---

### HelpCategory

**Purpose:** Identifies Playback, Queue, Playlists, Audio, Discovery, Settings and Utilities.

---

### HelpView

**Purpose:** Identifies Home, Category and Details views.

---

### HelpNavigationAction

**Purpose:** Identifies Category, Command, Previous, Next, Home and Back actions within versioned component IDs.

---

## Related Components

- `DC bot/Service/Presentation/DiscordPaginationService.cs` - Sends and owns expiring navigation panels.
- `DC bot/Interface/Service/Music/Playback/Models/PlaybackControlResult.cs` - Carries optional pagination alongside ordinary operation text.
