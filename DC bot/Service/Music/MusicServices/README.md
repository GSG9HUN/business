# Music Services

This folder contains granular music component services.

## Overview

These services split music functionality into focused responsibilities. Each implements a corresponding interface from
`Interface/Service/Music/`.

## Services

### CurrentTrackService.cs

**Implements:** `ICurrentTrackService`

**Purpose:** Track current playing track state per guild — persisted to the database via `IPlaybackStateRepository`.

**Key Methods:**

- `GetCurrentTrackAsync()` - Load and parse the current track from DB for a guild
- `SetCurrentTrackAsync()` - Persist the current track identifier and its `QueueItemId` to DB (or clear both with `null`). If the passed `ILavaLinkTrack` is a `LavaLinkTrackWrapper`, the `QueueItemId` is extracted from it automatically.
- `GetCurrentTrackFormattedAsync()` - Return `"Author Title"` string, or empty if no track is set

**Persistence:**

- Uses `IPlaybackStateRepository` (`guild_playback_state.current_track_identifier`, `guild_playback_state.queue_item_id`)
- Track is stored as a Lavalink track identifier string and reconstructed through `ITrackSerializer`
- Invalid/unparsable identifiers are silently handled (logged as warning, returns `null`)

---

### MusicQueueService.cs

**Implements:** `IMusicQueueService`

**Purpose:** Manage music queue for each guild.

**Key Methods:**

- `Enqueue()` - Add track to queue
- `Dequeue()` - Atomically claim and return the next track via `ClaimNextQueuedItemAsync`; the returned `LavaLinkTrackWrapper` has `QueueItemId` set to the DB row ID
- `ViewQueue()` - View all queued tracks
- `HasTracks()` - Check if queue has tracks
- `GetQueue()` - Get queue snapshot
- `GetRepeatableQueue()` - Load the saved repeat-list snapshot from `IRepeatListRepository`, parse valid track identifiers, and return tracks that can be copied back into the queue
- `SetQueue()` - Persist reordered queue
- `ClearQueue()` - Mark queued tracks as skipped

**Persistence:**

- Uses `IQueueRepository` for database-backed queue operations
- Uses `IRepeatListRepository` to read repeat-list snapshots when queue repeat needs to restart playback
- Uses `ITrackSerializer` as the only queue/repeat track identity serialization boundary
- Queue state transitions are persisted through `QueueItemState` (`Queued`, `Playing`, `Played`, `Skipped`)
- Ordering updates are handled transactionally in repository layer
- `QueueItemId` on the dequeued `LavaLinkTrackWrapper` is later used by `TrackEndedHandlerService` to mark the item as played or skipped
- When repeat-list playback restarts, `TrackEndedHandlerService` loads tracks through `GetRepeatableQueue()` and copies them into queue storage through `EnqueueMany()`

---

### LavalinkNodeConnectionService.cs

**Implements:** `ILavalinkNodeConnectionService`

**Purpose:** Start and guard the Lavalink node connection lifecycle.

**Key Methods:**

- `ConnectAsync()` - Start `IAudioService` once and map startup failures to `LavalinkOperationException`

**Notes:**

- Owns the connection semaphore and idempotent startup state
- Keeps node lifecycle details out of `LavaLinkService`

---

### LavalinkTrackSerializer.cs

**Implements:** `ITrackSerializer`

**Purpose:** Centralize Lavalink track identifier serialization and parsing.

**Notes:**

- `Serialize()` currently delegates to the Lavalink track identifier returned by the wrapped track.
- `Deserialize()` parses the stored identifier and returns a `LavaLinkTrackWrapper`; when a queue item ID is supplied it is copied into `QueueItemId`.
- Queue, repeat-list, and current-track services depend on this contract instead of duplicating parse logic.

---

### PlaybackControlService.cs

**Implements:** `IPlaybackControlService`

**Purpose:** Handle playback transport controls.

**Key Methods:**

- `PauseAsync()` - Validate the existing connected player, pause the current track, and pause the progressive timer
- `ResumeAsync()` - Validate the existing connected player, resume playback, and resume the progressive timer for the same track
- `SkipAsync()` - Stop the progressive timer before stopping the current player so stale now-playing updates do not continue
- `LeaveVoiceChannel()` - Stop playback if needed, clean playback handlers, stop timers, and disconnect

**Notes:**

- Keeps pause/resume/skip/leave behavior out of the `LavaLinkService` facade
- Reuses `IPlayerConnectionService` for player lookup and validation

---

### PlaybackEventHandlerService.cs

**Implements:** `IPlaybackEventHandlerService`

**Purpose:** Register and manage playback event handlers.

**Key Methods:**

- `RegisterPlaybackFinishedHandler()` - Attach one track-ended handler per guild
- `CleanupGuildAsync()` - Remove the registered track-ended handler for a guild

---

### PlaybackRequestService.cs

**Implements:** `IPlaybackRequestService`

**Purpose:** Orchestrate `play` requests for URL and query input.

**Key Methods:**

- `PlayAsyncUrl()` - Join/validate the voice channel, load tracks from a URL, register playback end handling, and delegate playback
- `PlayAsyncQuery()` - Join/validate the voice channel, load tracks from a search query, register playback end handling, and delegate playback

**Notes:**

- Keeps track loading and not-found handling out of `LavaLinkService`
- Delegates successful playback to `ITrackPlaybackService`

---

### PlayerConnectionService.cs

**Implements:** `IPlayerConnectionService`

**Purpose:** Manage player connections to voice channels.

**Key Methods:**

- `TryJoinAndValidateAsync()` - Allow an existing disconnected player a 10-second recovery window before stale cleanup, then join the voice channel and validate the connection
- `TryGetAndValidateExistingPlayerAsync()` - Validate an existing player and reject disconnected player state before playback controls run

**Join readiness behavior:**

- `TryJoinAndValidateAsync()` starts the Lavalink service once and checks node readiness on every request before attempting the voice join
- after `JoinAsync`, validation is delegated to `PlayerConnectionRetryPolicy`
- the retry path checks both the player returned by `JoinAsync` and the player manager lookup for the guild
- this protects first-call join/play flows where Discord voice state, Lavalink player state, and the player manager become consistent after the join call returns; readiness is checked every 500 ms for up to 10 seconds
- validation errors are sent only after the retry budget is exhausted
- join cleanup and validation are serialized per guild; playback-control validation waits for an in-progress join; leave holds the same lock through disconnect and status persistence

**Lifecycle:** Both methods accept optional `CancellationToken` values. Join cleanup, Lavalink join, and retry delay paths pass the token through so shutdown can interrupt waits instead of being swallowed as a validation error.

---

### PlayerConnectionRetryPolicy.cs

**Purpose:** Retry player readiness checks after a voice join.

**Key Methods:**

- `ValidateJoinedPlayerAsync()` - Retry validation for the returned join connection and the player manager's guild player until one becomes connected or the retry budget is exhausted

**Notes:**

- Handles the race where the bot has entered the voice channel, but Lavalink4NET does not yet report a connected player
- Preserves the manager validation error when no joined connection exists and no player appears during retry
- Uses the caller's `CancellationToken` for retry delays

---

### RepeatService.cs

**Implements:** `IRepeatService`

**Purpose:** Manage repeat mode flags and persist repeat-list snapshots.

**Key Methods:**

- `SetRepeatingAsync()` - Toggle current-track repeat
- `SetRepeatingListAsync()` - Toggle repeat-list mode and clear the saved snapshot when disabled
- `SaveRepeatListSnapshotAsync()` - Persist the current track plus queued tracks into `IRepeatListRepository`

**Persistence:** Repeat-list snapshots serialize tracks through `ITrackSerializer`.

---

### TrackEndedHandlerService.cs

**Implements:** `ITrackEndedHandlerService`

**Purpose:** Handle track end events and queue progression.

**Behavior:**

- marks the current queue item as played when the track finishes normally
- marks it as skipped for other end reasons
- stops the progressive timer for the completed guild before deciding the next playback path
- repeats the current track when single-track repeat is enabled and sends a fresh now-playing notification for the repeated track
- plays the next queued track when available
- rehydrates and plays the repeat-list snapshot when list repeat is enabled and the queue is empty
- sends the queue-empty notification when no playback path remains

---

### TrackFormatterService.cs

**Implements:** `ITrackFormatterService`

**Purpose:** Format track information for display.

---

### TrackNotificationService.cs

**Implements:** `ITrackNotificationService`

**Purpose:** Send notifications about track changes.

**Events:**

- `TrackStarted` - Fired when new track starts playing. It passes the target text channel and now-playing embed; it does not pass `DiscordClient`.

**Methods:**

- `NotifyNowPlayingAsync()` - Build and publish the now-playing embed
- `SendSafeAsync()` - Send notification with error handling

---

### TrackPlaybackService.cs

**Implements:** `ITrackPlaybackService`

**Purpose:** Play loaded tracks and queued tracks, send now-playing notifications, and update current-track state.

**Notes:**

- playlists are enqueued in bulk
- single tracks are enqueued and started immediately when the player has no current track
- when playback fails, the service sends the localized Lavalink validation error to the text channel

---

## Related Components

- **Interface/Service/Music/** - Service contracts
- **Service/Music/LavaLinkService.cs** - Orchestrates these services
- **DC bot.Contracts/Interface/Service/Persistence/** - Persistence contracts
- **DC bot.Persistence/Repositories/QueueRepository.cs** - Queue persistence implementation
- **DC bot.Persistence/Repositories/QueueClaimService.cs** - Internal atomic claim transaction used by `QueueRepository`

Voice status persistence failures are logged separately and do not turn a successful join or disconnect into a Lavalink error. Persisted status may remain stale until the next successful update.

Now-playing embeds display the track author/title, artwork when available, and total duration only. Timer start and resume calls are commented out, so these messages are not periodically edited.
