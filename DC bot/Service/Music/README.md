# Music

Coordinates music requests through a facade and specialized playback components.

## Overview

- `Lifecycle/TwentyFourSevenService` changes saved preferences under a shared guild gate. `PersistentVoiceRecoveryService` runs periodic recovery; `PersistentVoiceLeaveService` disables the preference before disconnecting and closing the batch.
- LavaLinkService connects commands to request, lifecycle, control, queue, and track-playback services. TrackSearchResolverService interprets source prefixes and provider URLs.
- The concrete components are organized into Events, Lavalink, Lifecycle, Playback, PlaylistService, Presentation, Queue, Repeat, and Tracks. There is no MusicServices or ProgressiveTimer implementation folder in the current layout.

## Subfolders

### Events/

Processes correlated track events and coordinates queue advancement.

**Files:**

- `PlaybackEventHandlerService.cs`
- `TrackEndedHandlerService.cs`

**Purpose:** PlaybackEventHandlerService manages player subscriptions. TrackEndedHandlerService settles the accepted attempt and selects repeat, next-track, or queue-end behavior.

---

### Lavalink/

Owns connection operations for the Lavalink audio node.

**Files:**

- `LavalinkNodeConnectionService.cs`

**Purpose:** LavalinkNodeConnectionService isolates node-level connectivity from guild player lifecycle and individual track transport calls.

---

### Lifecycle/

Coordinates player connections, session ownership, cleanup, and recovery through dedicated subfolders.

**Files and folders:**

- `Connections/PlayerConnectionService.cs`
- `Connections/PlayerConnectionRetryPolicy.cs`
- `Sessions/GuildPlaybackSessions.cs`
- `Cleanup/`
- `Recovery/`
- `PersistentVoice/`
- `Models/`
- `README.md`

**Purpose:** Keep lifecycle responsibilities and implementation-owned models separate. Configuration deadlines live in `DC bot/Configuration/PlaybackLifecycleOptions.cs`; each lifecycle subfolder documents its components and ownership rules.

---

### Playback/

Coordinates requested tracks, start claims, playback controls, and current-track persistence.

**Files:**

- `CurrentTrackService.cs`
- `PlaybackControlService.cs`
- `PlaybackRequestService.cs`
- `TrackPlaybackService.cs`

**Purpose:** PlaybackRequestService resolves user input. TrackPlaybackService claims and starts a queue item; PlaybackControlService handles pause, resume, skip, previous, and leave under session ownership.

---

### PlaylistService/

Separates saved-playlist queries, metadata changes, track loading, and track mutations behind IPlaylistService.

**Files:**

- `PlaylistMutationService.cs`
- `PlaylistQueryService.cs`
- `PlaylistService.cs`
- `PlaylistTrackDisplayMapper.cs`
- `PlaylistTrackLoader.cs`
- `PlaylistTrackMutationService.cs`

**Purpose:** Query helpers map stored tracks; mutation helpers validate limits and uniqueness; the loader resolves Lavalink content before persistence.

---

### Presentation/

Formats playback metadata and sends now-playing notifications.

**Files:**

- `TrackFormatterService.cs`
- `TrackNotificationService.cs`

**Purpose:** TrackFormatterService builds track text and embeds; TrackNotificationService handles response delivery through the selected message channel.

---

### Queue/

Maps track requests to durable queue items and batch-aware queue operations.

**Files:**

- `MusicQueueService.cs`

**Purpose:** MusicQueueService uses ITrackSerializer and IQueueRepository for enqueue, reads, removal, shuffle, move, repetition, and history-related state.

---

### Repeat/

Coordinates repeat-one and repeat-list behavior with playback state and canonical batches.

**Files:**

- `RepeatService.cs`

**Purpose:** RepeatService uses repositories to toggle repeat state and retrieve the appropriate tracks for another playback cycle.

---

### Tracks/

Adapts Lavalink tracks to the persisted identity format used by queue and playlist storage.

**Files:**

- `LavalinkTrackSerializer.cs`

**Purpose:** LavalinkTrackSerializer implements ITrackSerializer so use cases do not duplicate parsing or encoding details.

---

## Classes

### LavaLinkService

**File:** `DC bot/Service/Music/LavaLinkService.cs`

**Purpose:** Exposes music commands through connection, request, queue, and playback-control services.

**Implements / inherits:** `ILavaLinkService`.

**Methods:**

- `StartPlayingQueue` — Joins and validates the player's channel, registers completion handling, and attempts to start the next waiting item.

```csharp
public async Task<PlaybackStartResult> StartPlayingQueue(
    IDiscordMessage message,
    IDiscordChannel textChannel,
    IDiscordMember? member,
    CancellationToken cancellationToken = default);
```

- `ConnectAsync` — Connects the configured Discord or Lavalink runtime boundary and propagates connection failures.

```csharp
public async Task ConnectAsync(CancellationToken cancellationToken = default);
```

- `Init` — Initializes guild-specific persisted music state for subsequent commands.

```csharp
public async Task Init(ulong guildId, CancellationToken cancellationToken = default);
```

- `LeaveVoiceChannel` — Runs shared leave cleanup and closes the active batch after all applicable cleanup phases succeed.

```csharp
public async Task LeaveVoiceChannel(
    IDiscordMessage message,
    IDiscordMember? member,
    CancellationToken cancellationToken = default);
```

- `PauseAsync` — Pauses the validated player and persists playback position and pause state.

```csharp
public Task<PlaybackControlResult> PauseAsync(
    IDiscordMessage message,
    IDiscordMember? member,
    CancellationToken cancellationToken = default);
```

- `PlayAsyncQuery` — Delegates query playback to the request service using the supplied voice and response context.

```csharp
public Task<PlaybackStartResult> PlayAsyncQuery(
    IDiscordChannel voiceStateChannel,
    string query,
    IDiscordMessage message,
    TrackSearchMode trackSearchMode,
    string? requestedBy = null,
    CancellationToken cancellationToken = default);
```

**Additional methods:** `PlayAsyncUrl`, `ResumeAsync`, `SkipAsync`, `PreviousAsync`. See the source and its XML contracts for the complete parameter lists.

**Usage:**

The caller supplies the component and argument values used in this example.

```csharp
await lavaLinkService.ConnectAsync(cancellationToken);
```

---

### TrackSearchResolverService

**File:** `DC bot/Service/Music/TrackSearchResolverService.cs`

**Purpose:** Selects a search provider from a prefix, URL, or configured default.

**Implements / inherits:** `ITrackSearchResolverService`.

**Methods:**

- `ResolveSearchMode` — Resolves an explicit provider prefix or URL, falling back to the configured default.

```csharp
public TrackSearchMode ResolveSearchMode(string input);
```

**Usage:**

The caller supplies the component and argument values used in this example.

```csharp
var result = trackSearchResolverService.ResolveSearchMode(input);
```

---

## Related Components

- `DC bot/Service/README.md` — Parent feature overview and neighboring components.
- `DC bot/README.md` — Project setup and architecture.
- `DC bot/Service/Music/Events/README.md` — Detailed component contracts and examples for this subfolder.
- `DC bot/Service/Music/Lavalink/README.md` — Detailed component contracts and examples for this subfolder.
- `DC bot/Service/Music/Lifecycle/README.md` — Detailed component contracts and examples for this subfolder.
- `DC bot/Service/Music/Playback/README.md` — Detailed component contracts and examples for this subfolder.
- `DC bot/Service/Music/PlaylistService/README.md` — Detailed component contracts and examples for this subfolder.
- `DC bot/Service/Music/Presentation/README.md` — Detailed component contracts and examples for this subfolder.
- `DC bot/Service/Music/Queue/README.md` — Detailed component contracts and examples for this subfolder.
- `DC bot/Service/Music/Repeat/README.md` — Detailed component contracts and examples for this subfolder.
- `DC bot/Service/Music/Tracks/README.md` — Detailed component contracts and examples for this subfolder.
- `DC bot/Configuration/` — Defines typed options for bot identity, Lavalink connectivity, search resolution, and saved-playlist limits.
- `DC bot/Interface/Discord/` — Abstracts the Discord message, channel, guild, member, user, and voice-state values consumed by commands.
- `DC bot/Interface/Service/Music/` — Exposes the top-level music operations used by commands and adapters.
- `DC bot/Interface/Service/Presentation/` — Defines localized success, warning, error, and validation responses.
- `DC bot/Startup/DependencyInjection/` — Registers the runtime implementations used by these components.
