# Models

Carries inputs, statuses, and results for the queue use cases.

## Overview

- Implementations live under Service/Music/Queue; commands and orchestrators exchange these contracts instead of constructing transport or persistence dependencies themselves.
- Result states distinguish completed, rejected, pending, and superseded work where applicable. Preserve ownership and cancellation semantics when adding an operation.

## Models and Results

### QueueTrackToEnqueue

**File:** `DC bot/Interface/Service/Music/Queue/Models/QueueTrackToEnqueue.cs`

**Purpose:** Combines track, source, provider, and requester metadata for durable enqueueing.

**Record declaration:**

```csharp
public sealed record QueueTrackToEnqueue(
    ILavaLinkTrack Track,
    string? SourceQuery,
    TrackSearchMode? SourceSearchMode,
    string? RequestedBy);
```

---

## Related Components

- `DC bot/Interface/Service/Music/Queue/README.md` — Parent feature overview and neighboring components.
- `DC bot/README.md` — Project setup and architecture.
- `DC bot/Interface/Service/Music/Tracks/` — Defines the tracks boundary used by the bot music pipeline.
- `DC bot/Startup/DependencyInjection/` — Registers the runtime implementations used by these components.
