# Sessions

Coordinates guild player generations, exclusive control, track attempts, and lifecycle ownership.

## Overview

- Own shared session synchronization and invalidate stale operations before cleanup.
- Keep transport draining, database admission, and attempt correlation under the same coordinator.

## Session Coordination

### GuildPlaybackSessions

**File:** `DC bot/Service/Music/Lifecycle/Sessions/GuildPlaybackSessions.cs`

**Purpose:** Coordinates per-guild player generations, exclusive reservations, playback attempts, and ordered cleanup.

**Methods:**

- `ReserveControl` — Reserves exclusive playback control immediately, rejecting competing controls and unresolved starts.

```csharp
public ControlReservation ReserveControl(PlaybackSession session, string operationName);
```

- `ReserveControlAsync` — Waits asynchronously for competing control ownership to be released, then attempts to reserve the current session.

```csharp
public async Task<ControlReservation> ReserveControlAsync(
    PlaybackSession session,
    string operationName,
    CancellationToken cancellationToken = default);
```

- `ValidateControl` — Checks that the session generation and reservation still belong to the caller.

```csharp
public void ValidateControl(PlaybackSession session, ControlReservation reservation);
```

- `AbandonAttempt` — Clears a matching preparation or attempt after an operation ends without a committed start.

```csharp
public void AbandonAttempt(PlaybackSession session, Guid operationId);
```

- `AbortJoinAsync` — Starts or shares leave cleanup while preserving whether the SDK join was invoked.

```csharp
public Task<LeaveResult> AbortJoinAsync(ulong guildId, Func<ILavalinkPlayer?, bool, Task<LeaveResult>> cleanup);
```

- `BeginAttempt` — Marks an attempt as preparing while preserving a ready attempt during an owned replacement.

```csharp
public void BeginAttempt(PlaybackSession session, Guid operationId, ControlReservation reservation, bool replace = false);
```

**Additional methods:** `Capture`, `IsCurrent`, `MutateAsync`, `RunTransportAsync`, `BindAttempt`, `CompleteAttempt`, `OnTrackEnded`, `JoinAsync`, `LeaveAsync`, `ReleaseControl`, `IsAttemptCurrent`, `RecoverWithoutPlayerAsync`, `LeaveIfCurrentAsync`, `CleanupUnownedPlayerAsync`. See the source and its XML contracts for the complete parameter lists.

**Usage:**

The caller supplies the component and argument values used in this example.

```csharp
guildPlaybackSessions.ValidateControl(session, reservation);
```

**Behavior:** Sessions are valid only while their player and generation remain current. Release control reservations in finally. Leave invalidates ownership before draining transports; failed cleanup remains quarantined for an explicit retry.


**Additional methods:**

```csharp
public long? GetReadyQueueItemId(PlaybackSession session);

public Task<LeaveResult>? RetryClosingCleanup(
    ulong guildId,
    Func<ILavalinkPlayer?, Task<LeaveResult>> cleanup);
```

**Purpose:** Correlate checkpoints to the ready attempt using the existing track identity comparison, and retry quarantined cleanup without interrupting an active join.

---

## Related Components

- `DC bot/Service/Music/Lifecycle/README.md` - Parent lifecycle overview and folder navigation.
- `DC bot/Configuration/PlaybackLifecycleOptions.cs` - Shared join and cleanup deadlines.
- `DC bot/Service/Music/Lifecycle/Models/` - Session, reservation, and recovery data contracts.
- `DC bot/Startup/DependencyInjection/MusicServiceCollectionExtensions.cs` - Runtime registration.
