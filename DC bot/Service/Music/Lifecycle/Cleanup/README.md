# Cleanup

Settles disconnected or invalid players through the existing ownership-aware cleanup workflow.

## Overview

- Separate local invalidation, subscriptions, stop, remote disconnect, and persistence outcomes.
- Retain enabled persistent-voice checkpoints and keep uncertain remote starts fenced.

## Cleanup Services

### PlaybackCleanupService

**File:** `DC bot/Service/Music/Lifecycle/Cleanup/PlaybackCleanupService.cs`

**Purpose:** Runs bounded cleanup phases and reports local, remote, and persistence outcomes separately.

**Methods:**

- `CleanupAsync` — Runs cleanup phases and returns their separate outcomes instead of treating partial cleanup as success.

```csharp
public async Task<LeaveResult> CleanupAsync(ulong guildId, ILavalinkPlayer? player, bool joinWasNeverInvoked);
```

**Usage:**

The caller supplies the component and argument values used in this example.

```csharp
var result = await playbackCleanupService.CleanupAsync(guildId, player, joinWasNeverInvoked);
```

**Behavior:** Cleanup owns independent phase deadlines. Caller cancellation must not abandon an uncertain remote player or incorrectly persist a confirmed disconnect. FullySucceeded uses the completion predicate declared on LeaveResult; the Stop phase is reported separately from that predicate.


**Checkpoint policy:** After confirmed remote disconnection and pending-start settlement, enabled persistent voice retains current-track identity, position, and pause state. Explicit leave disables that flag first, so its cleanup clears current playback. A disconnected voice observation and a retained recovery checkpoint describe different things.


---

### DisconnectedPlayerCleanupService

**File:** `DisconnectedPlayerCleanupService.cs`

**Purpose:** Reconciles a locally owned player that is no longer registered with the SDK before another join. Cleanup remains generation-guarded and phase results remain observable; failed cleanup does not silently discard ownership.


---

## Related Components

- `DC bot/Service/Music/Lifecycle/README.md` - Parent lifecycle overview and folder navigation.
- `DC bot/Configuration/PlaybackLifecycleOptions.cs` - Shared join and cleanup deadlines.
- `DC bot/Service/Music/Lifecycle/Models/` - Session, reservation, and recovery data contracts.
- `DC bot/Startup/DependencyInjection/MusicServiceCollectionExtensions.cs` - Runtime registration.
