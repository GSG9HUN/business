# Music

Exposes the top-level music operations used by commands and adapters.

## Overview

- ILavaLinkService delegates to specialized request, connection, control, queue, and playback services.
- Use the feature subcontracts for internal responsibilities. A caller must not infer session ownership from a stored player reference alone.

## Subfolders

### Events/

Defines the events boundary used by the bot music pipeline.

**Files:**

- `IPlaybackEventHandlerService.cs`
- `ITrackEndedHandlerService.cs`

**Purpose:** Implementations live under Service/Music/Events; commands and orchestrators exchange these contracts instead of constructing transport or persistence dependencies themselves.

---

### Lavalink/

Defines the lavalink boundary used by the bot music pipeline.

**Files:**

- `ILavalinkNodeConnectionService.cs`

**Purpose:** Implementations live under Service/Music/Lavalink; commands and orchestrators exchange these contracts instead of constructing transport or persistence dependencies themselves.

---

### Lifecycle/

Defines the lifecycle boundary used by the bot music pipeline.

**Files:**

- `IPlayerConnectionService.cs`

**Purpose:** Implementations live under Service/Music/Lifecycle; commands and orchestrators exchange these contracts instead of constructing transport or persistence dependencies themselves.

---

### Playback/

Defines the playback boundary used by the bot music pipeline.

**Files:**

- `ICurrentTrackService.cs`
- `IPlaybackControlService.cs`
- `IPlaybackRequestService.cs`
- `ITrackPlaybackService.cs`

**Purpose:** Implementations live under Service/Music/Playback; commands and orchestrators exchange these contracts instead of constructing transport or persistence dependencies themselves.

---

### PlaylistServiceInterface/

Defines the playlist service boundary used by the bot music pipeline.

**Files:**

- `IPlaylistService.cs`

**Purpose:** Implementations live under Service/Music/PlaylistService; commands and orchestrators exchange these contracts instead of constructing transport or persistence dependencies themselves.

---

### Presentation/

Defines the presentation boundary used by the bot music pipeline.

**Files:**

- `ITrackFormatterService.cs`
- `ITrackNotificationService.cs`

**Purpose:** Implementations live under Service/Music/Presentation; commands and orchestrators exchange these contracts instead of constructing transport or persistence dependencies themselves.

---

### Queue/

Defines the queue boundary used by the bot music pipeline.

**Files:**

- `IMusicQueueService.cs`
- `QueueMoveResult.cs`
- `QueueRemoveResult.cs`
- `QueueShuffleResult.cs`

**Purpose:** Implementations live under Service/Music/Queue; commands and orchestrators exchange these contracts instead of constructing transport or persistence dependencies themselves.

---

### Repeat/

Defines the repeat boundary used by the bot music pipeline.

**Files:**

- `IRepeatService.cs`

**Purpose:** Implementations live under Service/Music/Repeat; commands and orchestrators exchange these contracts instead of constructing transport or persistence dependencies themselves.

---

### Search/

Defines the search boundary used by the bot music pipeline.

**Files:**

- `ITrackSearchResolverService.cs`

**Purpose:** Implementations live under Service/Music/Search; commands and orchestrators exchange these contracts instead of constructing transport or persistence dependencies themselves.

---

### Tracks/

Defines the tracks boundary used by the bot music pipeline.

**Files:**

- `ILavaLinkTrack.cs`
- `ITrackSerializer.cs`

**Purpose:** Implementations live under Service/Music/Tracks; commands and orchestrators exchange these contracts instead of constructing transport or persistence dependencies themselves.

---

## Interfaces

### ILavaLinkService

**File:** `DC bot/Interface/Service/Music/ILavaLinkService.cs`

**Purpose:** Defines the lava link service boundary so callers are independent of its concrete implementation.

**Methods:**

- `StartPlayingQueue` — Requests a validated player connection and starts the next waiting queue item.

```csharp
Task<PlaybackStartResult> StartPlayingQueue(
    IDiscordMessage message,
    IDiscordChannel textChannel,
    IDiscordMember? member,
    CancellationToken cancellationToken = default);
```

- `ConnectAsync` — Connects the configured Discord or Lavalink runtime boundary and propagates connection failures.

```csharp
Task ConnectAsync(CancellationToken cancellationToken = default);
```

- `Init` — Initializes guild-specific persisted music state for subsequent commands.

```csharp
Task Init(ulong guildId, CancellationToken cancellationToken = default);
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

- `PlayAsyncQuery` — Requests query playback using the supplied Discord voice and response context.

```csharp
Task<PlaybackStartResult> PlayAsyncQuery(
    IDiscordChannel toDiscordChannel,
    string query,
    IDiscordMessage message,
    TrackSearchMode trackSearchMode,
    string? requestedBy = null,
    CancellationToken cancellationToken = default);
```

**Additional methods:** `PlayAsyncUrl`, `SkipAsync`, `PreviousAsync`, `ResumeAsync`. See the source and its XML contracts for the complete parameter lists.

**Usage:**

The caller supplies the component and argument values used in this example.

```csharp
await lavaLinkService.ConnectAsync(cancellationToken);
```

---

## Related Components

- `DC bot/Interface/Service/README.md` — Parent feature overview and neighboring components.
- `DC bot/README.md` — Project setup and architecture.
- `DC bot/Interface/Service/Music/Events/README.md` — Detailed component contracts and examples for this subfolder.
- `DC bot/Interface/Service/Music/Lavalink/README.md` — Detailed component contracts and examples for this subfolder.
- `DC bot/Interface/Service/Music/Lifecycle/README.md` — Detailed component contracts and examples for this subfolder.
- `DC bot/Interface/Service/Music/Playback/README.md` — Detailed component contracts and examples for this subfolder.
- `DC bot/Interface/Service/Music/PlaylistServiceInterface/README.md` — Detailed component contracts and examples for this subfolder.
- `DC bot/Interface/Service/Music/Presentation/README.md` — Detailed component contracts and examples for this subfolder.
- `DC bot/Interface/Service/Music/Queue/README.md` — Detailed component contracts and examples for this subfolder.
- `DC bot/Interface/Service/Music/Repeat/README.md` — Detailed component contracts and examples for this subfolder.
- `DC bot/Interface/Service/Music/Search/README.md` — Detailed component contracts and examples for this subfolder.
- `DC bot/Interface/Service/Music/Tracks/README.md` — Detailed component contracts and examples for this subfolder.
- `DC bot/Interface/Discord/` — Abstracts the Discord message, channel, guild, member, user, and voice-state values consumed by commands.
- `DC bot/Interface/Service/Music/Playback/Models/` — Carries inputs, statuses, and results for the playback use cases.
- `DC bot/Startup/DependencyInjection/` — Registers the runtime implementations used by these components.
