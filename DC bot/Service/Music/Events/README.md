# Events

Processes correlated track events and coordinates queue advancement.

## Overview

- PlaybackEventHandlerService manages player subscriptions. TrackEndedHandlerService settles the accepted attempt and selects repeat, next-track, or queue-end behavior.
- Natural finish events must match the active session and attempt. Skip uses explicit correlated completion so a delayed SDK Stopped event cannot advance the queue a second time.

## Classes

### PlaybackEventHandlerService

**File:** `DC bot/Service/Music/Events/PlaybackEventHandlerService.cs`

**Purpose:** Correlates SDK end events to current attempts and retires previous player subscriptions safely.

**Implements / inherits:** `IPlaybackEventHandlerService`.

**Methods:**

- `CleanupGuildAsync` — Disables and removes current and retired track-end subscriptions for the guild.

```csharp
public Task CleanupGuildAsync(ulong guildId);
```

- `RegisterPlaybackFinishedHandler` — Subscribes the current player to correlated end events while retiring previous subscriptions.

```csharp
public void RegisterPlaybackFinishedHandler(ulong guildId, ILavalinkPlayer connection, IDiscordChannel textChannel);
```

**Usage:**

The caller supplies the component and argument values used in this example.

```csharp
await playbackEventHandlerService.CleanupGuildAsync(guildId);
```

---

### TrackEndedHandlerService

**File:** `DC bot/Service/Music/Events/TrackEndedHandlerService.cs`

**Purpose:** Completes the correlated attempt, then advances, reloads, repeats, or closes its batch.

**Implements / inherits:** `ITrackEndedHandlerService`.

**Methods:**

- `HandleTrackEndedAsync` — Completes the owned queue attempt, then reloads, repeats, advances, or closes the batch for its end reason.

```csharp
public async Task HandleTrackEndedAsync(
    PlaybackSession session,
    Guid operationId,
    long queueItemId,
    TrackEndedEventArgs args,
    IDiscordChannel textChannel);
```

**Usage:**

The caller supplies the component and argument values used in this example.

```csharp
await trackEndedHandlerService.HandleTrackEndedAsync(session, operationId, queueItemId, args, textChannel);
```

---

## Related Components

- `DC bot/Service/Music/README.md` — Parent feature overview and neighboring components.
- `DC bot/README.md` — Project setup and architecture.
- `DC bot/Exceptions/Music/` — Distinguishes Lavalink failures, track-load failures, expired sessions, and busy control operations.
- `DC bot/Interface/Discord/` — Abstracts the Discord message, channel, guild, member, user, and voice-state values consumed by commands.
- `DC bot/Wrapper/Lavalink/` — Exposes Lavalink track metadata through ILavaLinkTrack.
- `DC bot/Service/Music/Lifecycle/` — Coordinates per-guild player generations, control reservations, join validation, recovery, and ordered cleanup.
- `DC bot/Startup/DependencyInjection/` — Registers the runtime implementations used by these components.
