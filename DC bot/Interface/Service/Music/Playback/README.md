# Playback

Defines the playback boundary used by the bot music pipeline.

## Overview

- Defines playback inspection, control, filter, source-request and track-start boundaries.
- Preserves caller cancellation, session ownership and explicit operation outcomes.
- Uses shared result models for confirmed, rejected, pending and superseded work.

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

### IPlaybackFilterService

**Files:**

- `IPlaybackFilterService.cs`

**Purpose:** Exposes FiltersAsync, BassBoostAsync, SpeedAsync, PitchAsync, and RestoreAsync without adding an API/shared-contract dependency on SDK filter snapshots.

`FiltersAsync` supports inspection, preset selection/reset, advanced parameters and custom presets. Individual controls preserve unrelated effects/output; presets replace the effect map. `RestoreAsync` restores confirmed process-local effects during player replacement.

**Methods:** `FiltersAsync` inspects/lists/resets/selects presets; the three specific controls retain unrelated effects. `RestoreAsync` applies the last acknowledged process-local configuration during owned join validation.

---

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

Provides now-playing inspection, 0-200% volume, seek/forward/rewind/restart, pause/resume, skip/previous/leave and queue control. Position changes retain the occurrence and pause state. `SkipVotedAttemptAsync` binds background vote completion to its ready attempt.

**Methods:**

```csharp
Task<PlaybackControlResult> NowPlayingAsync(IDiscordMessage message, CancellationToken cancellationToken = default);
Task<PlaybackControlResult> VolumeAsync(
    IDiscordMessage message, IDiscordMember? member, string? value, CancellationToken cancellationToken = default);
Task<PlaybackControlResult> SeekAsync(
    IDiscordMessage message, IDiscordMember? member, string position, CancellationToken cancellationToken = default);
Task<PlaybackControlResult> ForwardAsync(
    IDiscordMessage message, IDiscordMember? member, string seconds, CancellationToken cancellationToken = default);
Task<PlaybackControlResult> RewindAsync(
    IDiscordMessage message, IDiscordMember? member, string seconds, CancellationToken cancellationToken = default);
Task<PlaybackControlResult> RestartAsync(
    IDiscordMessage message, IDiscordMember? member, CancellationToken cancellationToken = default);
```

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

`PlayNextLoadedAsync` places loaded tracks at the waiting front. `StartQueueSelectionAsync` starts a captured waiting occurrence. `StartExistingAsync` accepts `PlaybackResumeOptions` to restore confirmed position/pause without creating another canonical item.

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

## Related Components

- `DC bot/Interface/Service/Music/README.md` — Parent feature overview and neighboring components.
- `DC bot/README.md` — Project setup and architecture.
- `DC bot/Interface/Service/Music/Playback/Models/README.md` — Detailed component contracts and examples for this subfolder.
- `DC bot/Interface/Discord/` — Abstracts the Discord message, channel, guild, member, user, and voice-state values consumed by commands.
- `DC bot/Service/Music/Lifecycle/` — Coordinates per-guild player generations, control reservations, join validation, recovery, and ordered cleanup.
- `DC bot/Interface/Service/Music/Tracks/` — Defines the tracks boundary used by the bot music pipeline.
- `DC bot/Interface/Service/Music/Playback/Models/` — Carries inputs, statuses, and results for the playback use cases.
- `DC bot/Startup/DependencyInjection/` — Registers the runtime implementations used by these components.
