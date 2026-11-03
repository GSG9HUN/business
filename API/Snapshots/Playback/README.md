# Playback

Combines persisted current-track, repeat flags, queue metadata, and bot presence into a playback response.

## Overview

- PlaybackSnapshotService decodes the current track and advances an unpaused saved position by elapsed time, clamping it to a known duration.
- The response is a read model. Stored presence and an estimated position do not independently prove a healthy voice transport.

## Subfolders

### Interface/

Defines the playback read model shared by player HTTP reads and realtime publication.

**Files:**

- `IPlaybackSnapshotService.cs`

**Purpose:** The optional user ID allows guild display metadata to be loaded for a particular user while the guild state remains shared.

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

- `API/Snapshots/README.md` — Parent feature overview and neighboring components.
- `API/README.md` — Project setup and architecture.
- `API/Snapshots/Playback/Interface/README.md` — Detailed component contracts and examples for this subfolder.
- `API/Mapping/` — Converts repository records and service outcomes into HTTP response contracts.
- `API/Responses/Playback/` — Defines client-facing output for current playback state, repeat flags, start ownership, and canonical batch metadata.
- `API/Snapshots/Playback/Interface/` — Defines the playback read model shared by player HTTP reads and realtime publication.
- `DC bot.Contracts/Interface/Service/Persistence/GuildBotStatus/` — Defines repository operations for persisted bot voice presence, channel display metadata, and voice-user counts.
- `DC bot.Contracts/` — Shared storage and command protocol used by handlers and services.
