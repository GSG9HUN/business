# Playback

Defines the playback boundary used by the bot music pipeline.

## Overview

- Implementations live under Service/Music/Playback; commands and orchestrators exchange these contracts instead of constructing transport or persistence dependencies themselves.
- Result states distinguish completed, rejected, pending, and superseded work where applicable. Preserve ownership and cancellation semantics when adding an operation.

## Subfolders

### Models/

Carries inputs, statuses, and results for the playback use cases.

**Files:**

- `PlaybackControlResult.cs`
- `PlaybackStartReason.cs`
- `PlaybackStartResult.cs`
- `PlaybackStartStatus.cs`

**Purpose:** Implementations live under Service/Music/Playback; commands and orchestrators exchange these contracts instead of constructing transport or persistence dependencies themselves.

---

## Interfaces

### ICurrentTrackService

**File:** `DC bot/Interface/Service/Music/Playback/ICurrentTrackService.cs`

**Purpose:** Defines the current track service boundary so callers are independent of its concrete implementation.

**Methods:**

- `GetCurrentTrackAsync` — Restores the persisted current track, returning null when no current track can be resolved.

```csharp
Task<ILavaLinkTrack?> GetCurrentTrackAsync(ulong guildId, CancellationToken cancellationToken = default);
```

- `GetCurrentTrackFormattedAsync` — Formats the persisted current track for a command response.

```csharp
Task<string> GetCurrentTrackFormattedAsync(ulong guildId, CancellationToken cancellationToken = default);
```

- `SetCurrentTrackAsync` — Persists current-track identity and publishes the corresponding playback update.

```csharp
Task SetCurrentTrackAsync(
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

### IPlaybackControlService

**File:** `DC bot/Interface/Service/Music/Playback/IPlaybackControlService.cs`

**Purpose:** Defines the playback control service boundary so callers are independent of its concrete implementation.

**Methods:**

- `LeaveVoiceChannel` — Runs shared leave cleanup and closes the active batch after all applicable cleanup phases succeed.

```csharp
Task LeaveVoiceChannel(IDiscordMessage message, IDiscordMember? member, CancellationToken cancellationToken = default);
```

- `PauseAsync` — Pauses the validated player and persists playback position and pause state.

```csharp
Task<PlaybackControlResult> PauseAsync(
    IDiscordMessage message,
    IDiscordMember? member,
    CancellationToken cancellationToken = default);
```

- `PreviousAsync` — Starts the preceding canonical item in the active batch, reporting when no earlier item is available.

```csharp
Task<PlaybackControlResult> PreviousAsync(
    IDiscordMessage message,
    IDiscordMember? member,
    CancellationToken cancellationToken = default);
```

- `ResumeAsync` — Resumes the validated player and persists updated playback state.

```csharp
Task<PlaybackControlResult> ResumeAsync(
    IDiscordMessage message,
    IDiscordMember? member,
    CancellationToken cancellationToken = default);
```

- `SkipAsync` — Stops and completes the current attempt, then advances without relying on a later SDK Stopped event.

```csharp
Task<PlaybackControlResult> SkipAsync(
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

### IPlaybackRequestService

**File:** `DC bot/Interface/Service/Music/Playback/IPlaybackRequestService.cs`

**Purpose:** Defines the playback request service boundary so callers are independent of its concrete implementation.

**Methods:**

- `PlayAsyncQuery` — Requests source loading and session-bound playback for a search query.

```csharp
Task<PlaybackStartResult> PlayAsyncQuery(
    IDiscordChannel voiceStateChannel,
    string query,
    IDiscordMessage message,
    TrackSearchMode trackSearchMode,
    string? requestedBy = null,
    CancellationToken cancellationToken = default);
```

- `PlayAsyncUrl` — Requests source loading and session-bound playback for a URL.

```csharp
Task<PlaybackStartResult> PlayAsyncUrl(
    IDiscordChannel voiceStateChannel,
    Uri url,
    IDiscordMessage message,
    TrackSearchMode trackSearchMode,
    string? requestedBy = null,
    CancellationToken cancellationToken = default);
```

---

### ITrackPlaybackService

**File:** `DC bot/Interface/Service/Music/Playback/ITrackPlaybackService.cs`

**Purpose:** Defines the track playback service boundary so callers are independent of its concrete implementation.

**Methods:**

- `StartExistingAsync` — Starts the specified durable queue item under the supplied or newly acquired reservation.

```csharp
Task<PlaybackStartResult> StartExistingAsync(
    PlaybackSession session,
    IDiscordChannel textChannel,
    long queueItemId,
    PlaybackStartReason reason,
    ILavaLinkTrack? replacement = null,
    ControlReservation? reservation = null,
    CancellationToken cancellationToken = default);
```

- `TryPlayNextTrackAsync` — Claims and starts the next waiting track without blindly retrying uncertain remote dispatch.

```csharp
Task<PlaybackStartResult> TryPlayNextTrackAsync(
    PlaybackSession session,
    IDiscordChannel textChannel,
    PlaybackStartReason reason = PlaybackStartReason.QueueNext,
    ControlReservation? reservation = null,
    CancellationToken cancellationToken = default);
```

- `PlayTheFoundMusicAsync` — Adds loaded tracks to the active batch and starts playback when the player has no current track.

```csharp
Task<PlaybackStartResult> PlayTheFoundMusicAsync(
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

## Existing-Item Recovery

**Methods:**

```csharp
Task<PlaybackStartResult> StartExistingAsync(
    PlaybackSession session,
    IDiscordChannel textChannel,
    long queueItemId,
    PlaybackStartReason reason,
    ILavaLinkTrack? replacement = null,
    ControlReservation? reservation = null,
    PlaybackResumeOptions? resume = null,
    CancellationToken cancellationToken = default);
```

**Purpose:** Preserve the same queue occurrence while forwarding a confirmed position and pause state through start dispatch and commit. The optional checkpoint does not enqueue another canonical item.

---

## Related Components

- `DC bot/Interface/Service/Music/README.md` — Parent feature overview and neighboring components.
- `DC bot/README.md` — Project setup and architecture.
- `DC bot/Interface/Service/Music/Playback/Models/README.md` — Detailed component contracts and examples for this subfolder.
- `DC bot/Interface/Discord/` — Abstracts the Discord message, channel, guild, member, user, and voice-state values consumed by commands.
- `DC bot/Service/Music/Lifecycle/` — Coordinates per-guild player generations, control reservations, join validation, recovery, and ordered cleanup.
- `DC bot/Interface/Service/Music/Tracks/` — Defines the tracks boundary used by the bot music pipeline.
- `DC bot/Interface/Service/Music/Playback/Models/` — Carries inputs, statuses, and results for the playback use cases.
- `DC bot/Startup/DependencyInjection/` — Registers the runtime implementations used by these components.
