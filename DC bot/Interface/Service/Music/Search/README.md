# Search

Defines the search boundary used by the bot music pipeline.

## Overview

- Implementations live under Service/Music/Search; commands and orchestrators exchange these contracts instead of constructing transport or persistence dependencies themselves.
- Result states distinguish completed, rejected, pending, and superseded work where applicable. Preserve ownership and cancellation semantics when adding an operation.

## Interfaces

### ITrackSearchResolverService

**File:** `DC bot/Interface/Service/Music/Search/ITrackSearchResolverService.cs`

**Purpose:** Defines the track search resolver service boundary so callers are independent of its concrete implementation.

**Methods:**

- `ResolveSearchMode` — Resolves an explicit provider prefix or URL, falling back to the configured default.

```csharp
TrackSearchMode ResolveSearchMode(string input);
```

**Usage:**

The caller supplies the component and argument values used in this example.

```csharp
var result = trackSearchResolverService.ResolveSearchMode(input);
```

---

## Related Components

- `DC bot/Interface/Service/Music/README.md` — Parent feature overview and neighboring components.
- `DC bot/README.md` — Project setup and architecture.
- `DC bot/Startup/DependencyInjection/` — Registers the runtime implementations used by these components.
