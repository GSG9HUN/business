# Music Services

This folder contains granular music component services.

## Overview

These services split music functionality into focused responsibilities. Each implements a corresponding interface from
`Interface/Service/Music/MusicServiceInterface/`.

## Services

### CurrentTrackService.cs

**Implements:** `ICurrentTrackService`

**Purpose:** Track current playing track state per guild.

---

### MusicQueueService.cs

**Implements:** `IMusicQueueService`

**Purpose:** Manage music queue for each guild.

**Key Methods:**

- `Enqueue()` - Add track to queue
- `Dequeue()` - Remove and return next track
- `ViewQueue()` - View all queued tracks
- `HasTracks()` - Check if queue has tracks
- `GetQueue()` - Get queue snapshot
- `SetQueue()` - Persist reordered queue
- `ClearQueue()` - Mark queued tracks as skipped
- `GetRepeatableQueue()` - Get queue for repeat mode

**Persistence:**

- Uses `IQueueRepository` for database-backed queue operations
- Queue state transitions are persisted (`queued`, `playing`, `played`, `skipped`)
- Ordering updates are handled transactionally in repository layer

---

### PlaybackEventHandlerService.cs

**Implements:** `IPlaybackEventHandlerService`

**Purpose:** Register and manage playback event handlers.

---

### PlayerConnectionService.cs

**Implements:** `IPlayerConnectionService`

**Purpose:** Manage player connections to voice channels.

**Key Methods:**

- `TryJoinAndValidateAsync()` - Join voice channel and validate connection

---

### RepeatService.cs

**Implements:** `IRepeatService`

**Purpose:** Manage repeat modes (single track, queue).

---

### TrackEndedHandlerService.cs

**Implements:** `ITrackEndedHandlerService`

**Purpose:** Handle track end events and queue progression.

---

### TrackFormatterService.cs

**Implements:** `ITrackFormatterService`

**Purpose:** Format track information for display.

---

### TrackNotificationService.cs

**Implements:** `ITrackNotificationService`

**Purpose:** Send notifications about track changes.

**Events:**

- `TrackStarted` - Fired when new track starts playing

**Methods:**

- `SafeSendAsync()` - Send notification with error handling

---

### TrackPlaybackService.cs

**Implements:** `ITrackPlaybackService`

**Purpose:** Control track playback (play, pause, skip, resume).

---

## Related Components

- **Interface/Service/Music/MusicServiceInterface/** - Service contracts
- **Service/Music/LavaLinkService.cs** - Orchestrates these services
- **Interface/Service/Persistence/** - Persistence contracts
- **Persistence/Repositories/QueueRepository.cs** - Queue persistence implementation

