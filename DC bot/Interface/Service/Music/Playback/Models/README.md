# Models

Carries inputs, statuses, and results for the playback use cases.

## Overview

- Implementations live under Service/Music/Playback; commands and orchestrators exchange these contracts instead of constructing transport or persistence dependencies themselves.
- Result states distinguish completed, rejected, pending, and superseded work where applicable. Preserve ownership and cancellation semantics when adding an operation.

## Models and Results

### PlaybackControlResult

**File:** `DC bot/Interface/Service/Music/Playback/Models/PlaybackControlResult.cs`

**Purpose:** Carries the success state and response details of a playback control.

**Record declaration:**

```csharp
public sealed record PlaybackControlResult(bool Success, string Message, string? ErrorCode = null);
```

**Methods:**

- `Failed` — Creates a failed playback-control result with response details.

```csharp
public static PlaybackControlResult Failed(string message, string errorCode);
```

- `Succeeded` — Creates a successful playback-control result.

```csharp
public static PlaybackControlResult Succeeded(string message);
```

**Usage:**

The caller supplies the component and argument values used in this example.

```csharp
var result = PlaybackControlResult.Failed(message, errorCode);
```

---

### PlaybackStartReason

**File:** `DC bot/Interface/Service/Music/Playback/Models/PlaybackStartReason.cs`

**Purpose:** Identifies why a start was requested so next-track, repeat, and previous transitions can retain their distinct queue behavior.

**Values:**

```csharp
public enum PlaybackStartReason
{
    // Playback starts for the initial requested track.
    First,
    // Playback advances to the next queued track.
    QueueNext,
    // Playback starts from the queue after joining a voice channel.
    JoinQueue,
    // The current track is started again in single-track repeat mode.
    RepeatCurrent,
    // Playback starts a track from the repeated playback batch.
    RepeatList,
    // Playback starts a replacement or reloaded track.
    Reload,
    // Playback moves to the previous track in the active batch.
    Previous,
    // Playback restores the same durable queue item after confirmed disconnection.
    Recovery
}
```

---

### PlaybackStartResult

**File:** `DC bot/Interface/Service/Music/Playback/Models/PlaybackStartResult.cs`

**Purpose:** Describes a start outcome without treating uncertain remote dispatch as confirmed failure.

**Record declaration:**

```csharp
public sealed record PlaybackStartResult(PlaybackStartStatus Status, Guid OperationId, long? QueueItemId = null);
```

---

### PlaybackStartStatus

**File:** `DC bot/Interface/Service/Music/Playback/Models/PlaybackStartStatus.cs`

**Purpose:** Names the possible start outcomes, including an accepted start whose durable state still needs reconciliation.

**Values:**

```csharp
public enum PlaybackStartStatus
{
    // No item is available for the requested operation.
    Empty,
    // The track was queued without starting playback immediately.
    Queued,
    // The operation has started successfully.
    Started,
    // The attempt did not start playback.
    NotStarted,
    // Playback started, but the corresponding state update is still pending.
    StartedStatePending,
    // The operation's outcome could not be determined.
    Unknown,
    // A newer operation replaced this playback attempt.
    Superseded,
    // Another operation currently owns the required playback state.
    Busy
}
```

---

## Related Components

- `DC bot/Interface/Service/Music/Playback/README.md` — Parent feature overview and neighboring components.
- `DC bot/README.md` — Project setup and architecture.
- `DC bot/Startup/DependencyInjection/` — Registers the runtime implementations used by these components.
