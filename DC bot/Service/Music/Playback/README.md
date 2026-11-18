# Playback

Coordinates requested tracks, start claims, playback controls, and current-track persistence.

## Overview

- Resolves playback requests, starts queued occurrences and applies playback controls.
- Coordinates filter composition, transport confirmation and state persistence under guild session ownership.
- Distinguishes rejected, confirmed, pending and superseded operations.

## Subfolders

### Models/

Contains SDK-dependent playback data separated from behavior.

**Files:**

- `PlaybackFilterState.cs`
- `README.md`

**Purpose:** Retains complete immutable filter snapshots for confirmed, pending, and recovered state.

---

## Classes

### PlaybackFilterService

**Files:**

- `PlaybackFilterService.cs`

**Purpose:** Composes complete filter maps and owns capability checks, authoritative reconciliation, acknowledgement tracking, and restoration through existing guild sessions.

Reads node state, synchronizes the SDK cache, composes the complete filter map and reconciles uncertain updates under a control reservation. Individual controls retain unrelated built-in/plugin values; presets replace the map, and reset retains output volume.

Custom definitions are versioned, built-in-only and exclude output percentage; internal gain compensation remains part of effects. Save/overwrite/delete requires Manage Guild. Save/apply requires the bot voice channel; listing needs no player. Overwrite must be explicit.

**Methods:**

```csharp
Task<PlaybackControlResult> FiltersAsync(
    IDiscordMessage message, IDiscordMember? member, string? arguments, CancellationToken cancellationToken = default);
Task<PlaybackControlResult> BassBoostAsync(
    IDiscordMessage message, IDiscordMember? member, string? level, CancellationToken cancellationToken = default);
Task<PlaybackControlResult> SpeedAsync(
    IDiscordMessage message, IDiscordMember? member, string? multiplier, CancellationToken cancellationToken = default);
Task<PlaybackControlResult> PitchAsync(
    IDiscordMessage message, IDiscordMember? member, string? multiplier, CancellationToken cancellationToken = default);
Task RestoreAsync(ILavalinkPlayer player, CancellationToken cancellationToken = default);
```

**Validation Checks:** Mutations require the current player generation and the requesting member in the bot voice channel. Speed/pitch require finite invariant numbers in 0.5-2.0. Bass curves stay within 0-0.20 gain. Node capability data is cached per API client and node session, retaining the reported version, for one minute. Unknown outcomes are not retried blindly; the next inspection or mutation reads the authoritative player before composing new state.

**Usage:**

```text
<prefix>volume 65
<prefix>bassboost low
<prefix>speed 1.15
<prefix>filters show
<prefix>filters reset
```

The final state has neutral filters and retains output volume 65%. Individual bassboost changes only the equalizer; speed/pitch retain each other and rate. Presets replace the full filter map, including plugin effects. Low-frequency profiles are conservative starting values awaiting deployment listening verification. The bass-heavy preset applies 85% internal gain for headroom without changing player output volume.

After a timescale update the service reads the node source position, checks the same ready queue occurrence, and updates the existing position checkpoint. A checkpoint failure is reported separately from the acknowledged filter change.

**Advanced commands:**

| Effect | Values and application bounds |
| --- | --- |
| eq | band 0-14, gain -0.25..0.25; other bands are retained |
| rate | 0.5..2; speed and pitch are retained |
| karaoke | level/mono 0..1, band 20..20000 Hz, width 1..20000; on/off also supported |
| rotation | -1..1 Hz |
| channelmix | LL LR RL RR, each 0..1 |
| lowpass | smoothing 1.01..100 |
| tremolo | frequency 0.1..20, depth 0.01..1 |
| vibrato | frequency 0.1..14, depth 0.01..1 |
| distortion | sinOffset sinScale cosOffset cosScale tanOffset tanScale offset scale; offsets -1..1, scales 0..2 |

Every effect accepts off to clear just that component; rate off restores rate 1. Omitted values inspect configuration. These are application bounds, sometimes narrower than the [Lavalink protocol](https://lavalink.dev/api/rest/#filters). Invariant parsing rejects commas, exponent syntax, non-finite values, missing/excess parameters and values outside bounds before staging any SDK mutation.

**Custom preset usage:**

```text
<prefix>filters advanced eq 3 0.10
<prefix>filters advanced rate 1.10
<prefix>karaoke on
<prefix>filters custom save buli
<prefix>filters custom save buli overwrite
<prefix>filters custom list 1
<prefix>filters custom apply buli
<prefix>filters custom delete buli
```

Save captures authoritative node effects under the existing reservation, requires Manage Guild and the current voice channel, and records schema version 1. Names normalize to lowercase and accept 1-32 letters/digits, hyphen and underscore. Definitions are limited to validated built-in fields (including internal gain 0..1); arbitrary plugin definitions and incompatible versions are rejected. Apply replaces effects but retains output percentage. Delete requires Manage Guild and does not change current audio. List has ten-name pages without a total capacity limit. Save/overwrite is transactional; the active preset is not automatically selected on a fresh join. A leave resets temporary audio; saved definitions persist.

---

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

**Validation Checks:**

- Audio changes require the bot voice channel and an exclusive session reservation.
- Volume accepts integer 0-200%, with an amplification notice above 100%.
- Seek/restart retains occurrence identity and pause state; live/non-seekable tracks are rejected.
- Skip applies requester, DJ and vote rules; delayed completion cannot target another attempt.
- Skip-to validates the captured waiting ID/order snapshot. Earlier occurrences are skipped for the current pass while canonical repeat membership is preserved.
- Leave delegates to `PersistentVoiceLeaveService` and observes cleanup beyond the caller-facing deadline.

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

Loads provider URLs, playlists and searches for play or play-next. An enabled personal picker captures one result and stores its URI for reload; cancellation/expiry never enqueues a fallback result. Playlists and direct HTTP/HTTPS links bypass selection. Requests invalidate prior user menus and automatic decisions.

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

`PlayNextLoadedAsync` places occurrences first in waiting order while retaining canonical append order. `StartExistingAsync` restores an occurrence using confirmed position/pause; paused recovery applies track, position and pause together. `CommitKnownStartAsync` persists the confirmed start. Capacity rejection maps to `QueueFull`; uncertain starts require reconciliation before another remote play.

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

## Related Components

- `DC bot/Service/Music/README.md` — Parent feature overview and neighboring components.
- `DC bot/README.md` — Project setup and architecture.
- `DC bot/Constants/` — Centralizes identifiers used to select localized messages and validation errors.
- `DC bot/Logging/` — Defines typed log messages and reusable scopes for bot operations.
- `DC bot/Exceptions/Music/` — Distinguishes Lavalink failures, track-load failures, expired sessions, and busy control operations.
- `DC bot/Interface/Discord/` — Abstracts the Discord message, channel, guild, member, user, and voice-state values consumed by commands.
- `DC bot/Startup/DependencyInjection/` — Registers the runtime implementations used by these components.
