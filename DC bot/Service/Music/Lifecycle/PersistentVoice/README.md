# PersistentVoice

Coordinates saved twenty-four-seven preferences, periodic connection recovery, and explicit leave.

## Overview

- Coordinates saved 24/7 preferences, connection recovery and full leave cleanup.
- Serializes recovery, setting changes and leave admission through a guild gate.
- Restores confirmed playback checkpoints and prevents automatic recovery after explicit leave.

## Persistent Voice Services

### PersistentVoiceGate

**File:** `PersistentVoiceGate.cs`

**Purpose:** Shares one semaphore per guild between setting changes, automatic recovery, and explicit leave.

**Methods:**

```csharp
public SemaphoreSlim Get(ulong guildId);
```

Every successful acquisition has exactly one release. A setting change must not wait twice on the same non-reentrant semaphore.

---

### PersistentVoiceLeaveService

**File:** `PersistentVoiceLeaveService.cs`

**Purpose:** Disables persistent voice before owned leave cleanup and closes the active batch only after a fully successful result.

`LeaveIfEligibleAsync` rechecks persisted 24/7 mode, session ownership and human presence under `PersistentVoiceGate`. Admitted automatic leave uses full explicit-leave cleanup and batch closure. Temporary audio is cleared; saved custom presets remain stored.

**Methods:**

```csharp
public Task<LeaveResult> LeaveAsync(ulong guildId);
```

**Usage:**

```csharp
var leaving = persistentVoiceLeaveService.LeaveAsync(guildId);
var result = await leaving.WaitAsync(callerCancellationToken);
// The owner must also observe late failures if the caller stops waiting.
```

**Validation Checks:**

- Acquire the same guild gate as recovery and preference changes.
- Persist disabled mode before invalidating the player; a failed preference write prevents cleanup.
- Cleanup and batch persistence own independent deadlines rather than using the command caller's cancellation.
- Retain the gate until the admitted operation completes, even if the command stops waiting.
- Retry batch closure after a previously completed shared leave without requiring another remote disconnect.

---

### PersistentVoiceRecoveryService

**File:** `PersistentVoiceRecoveryService.cs`

**Purpose:** Checks available guilds every five seconds, preserves healthy players, and replaces disconnected players after confirmed cleanup.

The five-second worker runs music-control maintenance before recovery. Empty channels leave after five minutes only with 24/7 disabled. Enabling 24/7 cancels the countdown.

Without 24/7, recovery requires a playing occurrence, waiting track or nonempty repeated batch plus process-local remembered channels. Explicit leave clears that intent. Recovery restores a checkpoint or starts/refills an eligible queue; unknown starts remain fenced.

**Methods:**

```csharp
public Task RunAsync(CancellationToken cancellationToken);
```

**Usage:**

```csharp
await persistentVoiceRecovery.RunAsync(stoppingToken);
```

**Validation Checks:**

- Read saved enabled mode and voice/text targets; other human listeners are not required.
- Refresh a matching owned player and validate its connection and target channel.
- Allow ten seconds of failed health validation on a still-registered owned player before replacement.
- Defer new playback while a pending start requires reconciliation.
- Cleanup invalidates and drains the old generation; a failed cleanup remains quarantined.
- Restore the retained guild queue item still marked `Playing`; otherwise start queued music or atomically refill an enabled canonical repeat batch.
- Seekable tracks resume at the stored source position, while non-seekable tracks restart without seeking.
- Save position and pause state only for a ready, correlated queue attempt under a control reservation.
- Busy operations defer for five seconds; other failures defer for thirty seconds. Each pass uses the configured join plus leave deadlines.
- Cancelling the worker's wait does not cancel admitted lifecycle cleanup; late failures remain observed.

**Limits:** Checkpoints are periodic, so resumption is approximate. Missing permissions/channels and unresolved remote starts can defer recovery. A successful build does not prove live Discord or Lavalink connectivity.

---

### TwentyFourSevenService

**File:** `TwentyFourSevenService.cs`

**Purpose:** Serializes setting changes per guild and establishes or validates the initial player before saving enabled preferences.

**Methods:**

```csharp
public Task<TwentyFourSevenSettingsRecord> GetAsync(ulong guildId, CancellationToken cancellationToken = default);
public Task<TwentyFourSevenSettingsRecord?> SetAsync(
    IDiscordMessage message,
    bool enabled,
    IDiscordChannel? voiceChannel,
    CancellationToken cancellationToken = default);
```

**Usage:**

```csharp
var settings = await twentyFourSevenService.SetAsync(message, true, voiceChannel, cancellationToken);
if (settings is null)
{
    return; // Join validation already sent a response.
}
```

**Validation Checks:**

- Enabling uses `TryJoinAndValidateAsync` and stores the validated player's actual voice channel.
- Disabling saves the flag without joining, disconnecting, or requiring voice membership.
- Initial setup does not start or advance the queue.
- This service handles preference changes and initial setup under `PersistentVoiceGate`; the separate periodic worker performs rejoining and checkpoint restoration.

---

## Related Components

- `DC bot/Service/Music/Lifecycle/README.md` - Parent lifecycle overview and folder navigation.
- `DC bot/Configuration/PlaybackLifecycleOptions.cs` - Shared join and cleanup deadlines.
- `DC bot/Service/Music/Lifecycle/Models/` - Session, reservation, and recovery data contracts.
- `DC bot/Startup/DependencyInjection/MusicServiceCollectionExtensions.cs` - Runtime registration.
