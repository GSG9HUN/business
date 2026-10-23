# Snapshots

Groups snapshot assembly used by both HTTP reads and realtime events.

## Overview

- The playback implementation combines repositories and mapping rather than reading a live Discord or Lavalink player.
- Keep snapshot composition reusable across transports. Snapshot assembly must not itself start or stop playback.

## Subfolders

### Playback/

Combines persisted current-track, repeat flags, queue metadata, and bot presence into a playback response.

**Files:**

- `PlaybackSnapshotService.cs`

**Purpose:** PlaybackSnapshotService decodes the current track and advances an unpaused saved position by elapsed time, clamping it to a known duration.

---

## Classes

### PlaybackSnapshotService

**File:** `API/Snapshots/Playback/PlaybackSnapshotService.cs`

**Purpose:** Combines persisted playback, bot presence, and queue data into a REST and realtime read model.

**Implements / inherits:** `IPlaybackSnapshotService`.

**Methods:**

- `GetSnapshotAsync` — Returns a guild's assembled playback read model with bot presence and queue metadata.

```csharp
public async Task<PlaybackStatusResponse> GetSnapshotAsync(
    ulong guildId,
    ulong? userId = null,
    CancellationToken cancellationToken = default);
```

**Usage:**

The caller supplies the component and argument values used in this example.

```csharp
var result = await playbackSnapshotService.GetSnapshotAsync(guildId, userId, cancellationToken);
```

---

## Related Components

- `API/README.md` — Parent feature overview and neighboring components.
- `API/Snapshots/Playback/README.md` — Detailed component contracts and examples for this subfolder.
- `DC bot.Contracts/` — Shared storage and command protocol used by handlers and services.
