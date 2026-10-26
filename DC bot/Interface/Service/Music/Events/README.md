# Events

Defines the events boundary used by the bot music pipeline.

## Overview

- Implementations live under Service/Music/Events; commands and orchestrators exchange these contracts instead of constructing transport or persistence dependencies themselves.
- Result states distinguish completed, rejected, pending, and superseded work where applicable. Preserve ownership and cancellation semantics when adding an operation.

## Interfaces

### IPlaybackEventHandlerService

**File:** `DC bot/Interface/Service/Music/Events/IPlaybackEventHandlerService.cs`

**Purpose:** Defines the playback event handler service boundary so callers are independent of its concrete implementation.

**Methods:**

- `CleanupGuildAsync` — Disables and removes current and retired track-end subscriptions for the guild.

```csharp
Task CleanupGuildAsync(ulong guildId);
```

- `RegisterPlaybackFinishedHandler` — Subscribes the current player to correlated end events while retiring previous subscriptions.

```csharp
void RegisterPlaybackFinishedHandler(ulong guildId, ILavalinkPlayer connection, IDiscordChannel textChannel);
```

**Usage:**

The caller supplies the component and argument values used in this example.

```csharp
await playbackEventHandlerService.CleanupGuildAsync(guildId);
```

---

### ITrackEndedHandlerService

**File:** `DC bot/Interface/Service/Music/Events/ITrackEndedHandlerService.cs`

**Purpose:** Defines the track ended handler service boundary so callers are independent of its concrete implementation.

**Methods:**

- `HandleTrackEndedAsync` — Completes the owned queue attempt, then reloads, repeats, advances, or closes the batch for its end reason.

```csharp
Task HandleTrackEndedAsync(
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

- `DC bot/Interface/Service/Music/README.md` — Parent feature overview and neighboring components.
- `DC bot/README.md` — Project setup and architecture.
- `DC bot/Interface/Discord/` — Abstracts the Discord message, channel, guild, member, user, and voice-state values consumed by commands.
- `DC bot/Service/Music/Lifecycle/` — Coordinates per-guild player generations, control reservations, join validation, recovery, and ordered cleanup.
- `DC bot/Startup/DependencyInjection/` — Registers the runtime implementations used by these components.
