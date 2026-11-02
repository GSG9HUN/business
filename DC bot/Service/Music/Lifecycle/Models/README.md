# Models

Contains the session identities, control reservations, cleanup outcomes, and recovery values used by lifecycle services.

## Overview

- Keep implementation-owned data separate from lifecycle services.
- SDK-dependent models remain in the bot project rather than the shared Contracts project.
- PlaybackSession and ControlReservation identify ownership; neither creates or restores a player.

## Lifecycle Models

### ControlReservation

**File:** `DC bot/Service/Music/Lifecycle/Models/ControlReservation.cs`

**Purpose:** Identifies the operation that exclusively owns playback control in one guild generation.

**Record declaration:**

```csharp
public sealed record ControlReservation(ulong GuildId, long Generation, Guid OperationId, string OperationName);
```


---

### LeaveResult

**File:** `DC bot/Service/Music/Lifecycle/Models/LeaveResult.cs`

**Purpose:** Describes individual cleanup phase outcomes and whether the entire leave operation completed successfully; a partial failure remains observable.

**Properties:**

```csharp
// Whether local invalidation succeeded and timer, subscription, disconnect, and persistence phases are settled; the stop phase is reported separately.
public bool FullySucceeded { get; }
```
**Completion checks:** `FullySucceeded` requires successful local invalidation and settled timer, subscription, remote-disconnect, voice-persistence, and current-track-persistence phases. A settled phase is `Succeeded` or `NotApplicable`. The `Stop` result is recorded separately and is not part of this computed property.

**Record declaration:**

```csharp
public sealed record LeaveResult(
    LeavePhaseState LocalInvalidation,
    LeavePhaseState Timer,
    LeavePhaseState Subscription,
    LeavePhaseState Stop,
    LeavePhaseState RemoteDisconnected,
    LeavePhaseState VoicePersistence,
    LeavePhaseState CurrentTrackPersistence);
```

**Related enum:**

```csharp
public enum LeavePhaseState
{
    NotAttempted,
    Succeeded,
    Failed,
    Unknown,
    NotApplicable
}
```


---

### PlaybackResumeOptions

**File:** `PlaybackResumeOptions.cs`

**Purpose:** Carries source-track position and pause state through existing-item dispatch and confirmed-start persistence.

**Properties:**

```csharp
public TimeSpan Position { get; init; }
public bool IsPaused { get; init; }
```

The start pipeline applies a paused checkpoint with one remote track/position/pause update. The matching queue commit saves position and pause state in the same transaction as the confirmed start.


---

### PlaybackSession

**File:** `DC bot/Service/Music/Lifecycle/Models/PlaybackSession.cs`

**Purpose:** Identifies the guild player generation used to reject superseded playback work.

**Record declaration:**

```csharp
public sealed record PlaybackSession(ulong GuildId, long Generation, ILavalinkPlayer Player);
```


---

### VoiceRecoveryTarget

**File:** `DC bot/Service/Music/Lifecycle/Models/VoiceRecoveryTarget.cs`

**Purpose:** Captures the player generation, voice channel, and node session permitted for gateway recovery.

**Record declaration:**

```csharp
internal sealed record VoiceRecoveryTarget(PlaybackSession Session, ulong ChannelId, string NodeSessionId);
```


---

### VoiceRecoveryWindow

**File:** `DC bot/Service/Music/Lifecycle/Models/VoiceRecoveryWindow.cs`

**Purpose:** Limits automatic voice recovery to one player generation and expiry time.

**Record declaration:**

```csharp
internal sealed record VoiceRecoveryWindow(long Generation, DateTimeOffset Until);
```


---

## Related Components

- `DC bot/Service/Music/Lifecycle/README.md` - Parent lifecycle overview and folder navigation.
- `DC bot/Configuration/PlaybackLifecycleOptions.cs` - Shared join and cleanup deadlines.
- `DC bot/Startup/DependencyInjection/MusicServiceCollectionExtensions.cs` - Runtime registration.
