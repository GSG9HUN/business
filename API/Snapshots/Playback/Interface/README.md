# Interface

Defines the playback read model shared by player HTTP reads and realtime publication.

## Overview

- The optional user ID allows guild display metadata to be loaded for a particular user while the guild state remains shared.
- Keep REST and realtime consumers on the same snapshot semantics and forward cancellation through all underlying reads.

## Interfaces

### IPlaybackSnapshotService

**File:** `API/Snapshots/Playback/Interface/IPlaybackSnapshotService.cs`

**Purpose:** Defines the shared guild playback snapshot query.

**Methods:**

- `GetSnapshotAsync` — Returns a guild's assembled playback read model with bot presence and queue metadata.

```csharp
Task<PlaybackStatusResponse> GetSnapshotAsync(ulong guildId, ulong? userId = null, CancellationToken cancellationToken = default);
```

**Usage:**

The caller supplies the component and argument values used in this example.

```csharp
var result = await playbackSnapshotService.GetSnapshotAsync(guildId, userId, cancellationToken);
```

---

## Related Components

- `API/Snapshots/Playback/README.md` — Parent feature overview and neighboring components.
- `API/README.md` — Project setup and architecture.
- `API/Responses/Playback/` — Defines client-facing output for current playback state, repeat flags, start ownership, and canonical batch metadata.
- `DC bot.Contracts/` — Shared storage and command protocol used by handlers and services.
