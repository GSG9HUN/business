# Playback

Coordinates requested tracks, start claims, playback controls, and current-track persistence.

## Overview

- PlaybackRequestService resolves user input. TrackPlaybackService claims and starts a queue item; PlaybackControlService handles pause, resume, skip, previous, and leave under session ownership.
- Previous selects within the active canonical batch. A remote start with an uncertain outcome is reconciled before retrying; do not blindly issue another PlayAsync and risk duplicate playback.

## Classes

### CurrentTrackService

**File:** `DC bot/Service/Music/Playback/CurrentTrackService.cs`

**Purpose:** Reads and persists the current track and formats it for command responses.

**Implements / inherits:** `ICurrentTrackService`.

**Methods:**

- `GetCurrentTrackAsync` — Restores the persisted current track, returning null when no current track can be resolved.

```csharp
public async Task<ILavaLinkTrack?> GetCurrentTrackAsync(ulong guildId, CancellationToken cancellationToken = default);
```

- `GetCurrentTrackFormattedAsync` — Formats the persisted current track for a command response.

```csharp
public async Task<string> GetCurrentTrackFormattedAsync(ulong guildId, CancellationToken cancellationToken = default);
```

- `SetCurrentTrackAsync` — Persists current-track identity and publishes the corresponding playback update.

```csharp
public async Task SetCurrentTrackAsync(
    ulong guildId,
    ILavaLinkTrack? track,
    string? realtimeEventName = null,
    CancellationToken cancellationToken = default);
```

**Usage:**

The caller supplies the component and argument values used in this example.

```csharp
var result = await currentTrackService.GetCurrentTrackAsync(guildId, cancellationToken);
```

---

### PlaybackControlService

**File:** `DC bot/Service/Music/Playback/PlaybackControlService.cs`

**Purpose:** Applies skip, previous, pause, resume, and leave under exclusive playback ownership.

**Implements / inherits:** `IPlaybackControlService`.

**Methods:**

- `LeaveVoiceChannel` — Runs shared leave cleanup and closes the active batch after all applicable cleanup phases succeed.

```csharp
public async Task LeaveVoiceChannel(
    IDiscordMessage message,
    IDiscordMember? member,
    CancellationToken cancellationToken = default);
```

- `PauseAsync` — Pauses the validated player and persists playback position and pause state.

```csharp
public async Task<PlaybackControlResult> PauseAsync(
    IDiscordMessage message,
    IDiscordMember? member,
    CancellationToken cancellationToken = default);
```

- `PreviousAsync` — Starts the preceding canonical item in the active batch, reporting when no earlier item is available.

```csharp
public async Task<PlaybackControlResult> PreviousAsync(
    IDiscordMessage message,
    IDiscordMember? member,
    CancellationToken cancellationToken = default);
```

- `ResumeAsync` — Resumes the validated player and persists updated playback state.

```csharp
public async Task<PlaybackControlResult> ResumeAsync(
    IDiscordMessage message,
    IDiscordMember? member,
    CancellationToken cancellationToken = default);
```

- `SkipAsync` — Stops and completes the current attempt, then advances without relying on a later SDK Stopped event.

```csharp
public async Task<PlaybackControlResult> SkipAsync(
    IDiscordMessage message,
    IDiscordMember? member,
    CancellationToken cancellationToken = default);
```

**Usage:**

The caller supplies the component and argument values used in this example.

```csharp
await playbackControlService.LeaveVoiceChannel(message, member, cancellationToken);
```

---

### PlaybackRequestService

**File:** `DC bot/Service/Music/Playback/PlaybackRequestService.cs`

**Purpose:** Loads requested tracks and routes them to a validated, session-bound queue.

**Implements / inherits:** `IPlaybackRequestService`.

**Methods:**

- `PlayAsyncQuery` — Loads a search query through the shared session-bound playback request pipeline.

```csharp
public Task<PlaybackStartResult> PlayAsyncQuery(
    IDiscordChannel voiceStateChannel,
    string query,
    IDiscordMessage message,
    TrackSearchMode trackSearchMode,
    string? requestedBy = null,
    CancellationToken cancellationToken = default);
```

- `PlayAsyncUrl` — Loads a source URL through the shared session-bound playback request pipeline.

```csharp
public Task<PlaybackStartResult> PlayAsyncUrl(
    IDiscordChannel voiceStateChannel,
    Uri url,
    IDiscordMessage message,
    TrackSearchMode trackSearchMode,
    string? requestedBy = null,
    CancellationToken cancellationToken = default);
```

---

### TrackPlaybackService

**File:** `DC bot/Service/Music/Playback/TrackPlaybackService.cs`

**Purpose:** Claims queue items, dispatches playback, and commits or reconciles results under session ownership.

**Implements / inherits:** `ITrackPlaybackService`.

**Methods:**

- `StartExistingAsync` — Starts the specified durable queue item under the supplied or newly acquired reservation.

```csharp
public Task<PlaybackStartResult> StartExistingAsync(
    PlaybackSession session,
    IDiscordChannel textChannel,
    long queueItemId,
    PlaybackStartReason reason,
    ILavaLinkTrack? replacement = null,
    ControlReservation? reservation = null,
    PlaybackResumeOptions? resume = null,
    CancellationToken cancellationToken = default);
```

- `TryPlayNextTrackAsync` — Claims and starts the next waiting track without blindly retrying uncertain remote dispatch.

```csharp
public Task<PlaybackStartResult> TryPlayNextTrackAsync(
    PlaybackSession session,
    IDiscordChannel textChannel,
    PlaybackStartReason reason = PlaybackStartReason.QueueNext,
    ControlReservation? reservation = null,
    CancellationToken cancellationToken = default);
```

- `PlayTheFoundMusicAsync` — Adds loaded tracks to the active batch and starts playback when the player has no current track.

```csharp
public async Task<PlaybackStartResult> PlayTheFoundMusicAsync(
    TrackLoadResult searchQuery,
    PlaybackSession session,
    IDiscordChannel textChannel,
    string? requestedBy,
    string? sourceQuery,
    TrackSearchMode? sourceSearchMode,
    CancellationToken cancellationToken = default);
```

**Usage:**

The caller supplies the component and argument values used in this example.

```csharp
var result = await trackPlaybackService.TryPlayNextTrackAsync(session, textChannel, reason, reservation, cancellationToken);
```

---

## Recovery Start and Explicit Leave

**Purpose:** Keep replacement playback inside the existing queue-claim, node-binding, attempt-correlation, and commit workflow.

- `StartExistingAsync` accepts optional `PlaybackResumeOptions`; normal next-track starts pass no recovery checkpoint.
- Recovery uses `PlaybackStartReason.Recovery` and the existing queue-item ID instead of adding a new batch item.
- Paused recovery sends the track, position, and pause in one remote update. Ordinary recovery uses `TrackPlayProperties.StartPosition`.
- `CommitKnownStartAsync` forwards position and pause state to the transactional queue commit.
- Now-playing notifications use the restored starting position.
- `LeaveVoiceChannel` delegates to `PersistentVoiceLeaveService` and continues observing late cleanup faults after its caller-facing deadline.

---

## Related Components

- `DC bot/Service/Music/README.md` — Parent feature overview and neighboring components.
- `DC bot/README.md` — Project setup and architecture.
- `DC bot/Constants/` — Centralizes identifiers used to select localized messages and validation errors.
- `DC bot/Logging/` — Defines typed log messages and reusable scopes for bot operations.
- `DC bot/Exceptions/Music/` — Distinguishes Lavalink failures, track-load failures, expired sessions, and busy control operations.
- `DC bot/Interface/Discord/` — Abstracts the Discord message, channel, guild, member, user, and voice-state values consumed by commands.
- `DC bot/Startup/DependencyInjection/` — Registers the runtime implementations used by these components.
