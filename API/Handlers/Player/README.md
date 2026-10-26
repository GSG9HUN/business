# Player

Returns the playback read model for a guild visible to the authenticated user.

## Overview

- Guild access is checked before IPlaybackSnapshotService assembles track, queue, repeat, and bot-presence data.
- This is a persisted snapshot read, not a direct Lavalink player query. Keep its response aligned with realtime playback snapshots.

## Classes

### PlayerHandler

**File:** `API/Handlers/Player/PlayerHandler.cs`

**Purpose:** Returns an authorized guild playback snapshot using the shared snapshot service.

**Methods:**

- `GetSnapshotAsync` — Returns a guild's assembled playback read model with bot presence and queue metadata.

```csharp
public static async Task<IResult> GetSnapshotAsync(
    HttpContext httpContext,
    IMobileAppUserRepository userRepository,
    IPlaybackSnapshotService playbackSnapshotService,
    CancellationToken cancellationToken);
```

**Usage:**

The caller supplies the component and argument values used in this example.

```csharp
var result = await PlayerHandler.GetSnapshotAsync(httpContext, userRepository, playbackSnapshotService, cancellationToken);
```

---

## Related Components

- `API/Handlers/README.md` — Parent feature overview and neighboring components.
- `API/README.md` — Project setup and architecture.
- `API/Snapshots/Playback/Interface/` — Defines the playback read model shared by player HTTP reads and realtime publication.
- `DC bot.Contracts/Interface/Service/Persistence/MobileApps/` — Defines repository operations for mobile identities, guild access, refresh sessions, and realtime subscription membership.
- `DC bot.Contracts/` — Shared storage and command protocol used by handlers and services.
