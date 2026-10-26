# Queue

Defines the queue boundary used by the bot music pipeline.

## Overview

- Implementations live under Service/Music/Queue; commands and orchestrators exchange these contracts instead of constructing transport or persistence dependencies themselves.
- Result states distinguish completed, rejected, pending, and superseded work where applicable. Preserve ownership and cancellation semantics when adding an operation.

## Subfolders

### Models/

Carries inputs, statuses, and results for the queue use cases.

**Files:**

- `QueueTrackToEnqueue.cs`

**Purpose:** Implementations live under Service/Music/Queue; commands and orchestrators exchange these contracts instead of constructing transport or persistence dependencies themselves.

---

## Interfaces

### IMusicQueueService

**File:** `DC bot/Interface/Service/Music/Queue/IMusicQueueService.cs`

**Purpose:** Defines the music queue service boundary so callers are independent of its concrete implementation.

**Methods:**

- `GetQueue` — Restores waiting tracks and marks unparseable persisted queue entries skipped.

```csharp
public Task<Queue<ILavaLinkTrack>> GetQueue(ulong guildId, CancellationToken cancellationToken = default);
```

- `GetRepeatableQueue` — Restores all canonical items of the active batch, including played tracks, logging unparseable entries.

```csharp
Task<IReadOnlyCollection<ILavaLinkTrack>> GetRepeatableQueue(ulong guildId, CancellationToken cancellationToken = default);
```

- `TryEnableBatchRepeatAsync` — Atomically enables repetition of the live canonical batch when it contains active playback work.

```csharp
Task<bool> TryEnableBatchRepeatAsync(ulong guildId, CancellationToken cancellationToken = default);
```

- `ClearQueue` — Marks waiting tracks skipped while preserving the playing track and canonical batch history.

```csharp
Task ClearQueue(ulong guildId, CancellationToken cancellationToken = default);
```

- `EnqueueMany` — Serializes incoming tracks and appends their source metadata to the active batch and waiting queue.

```csharp
public Task EnqueueMany(
    ulong guildId,
    IReadOnlyCollection<QueueTrackToEnqueue> tracks,
    CancellationToken cancellationToken = default);
```

- `HasTracks` — Reports whether the guild has durable waiting queue entries.

```csharp
public Task<bool> HasTracks(ulong guildId, CancellationToken cancellationToken = default);
```

**Additional methods:** `ViewQueue`, `SetQueue`, `ShuffleQueue`, `RemoveAt`, `MoveToIndex`. See the source and its XML contracts for the complete parameter lists.

**Usage:**

The caller supplies the component and argument values used in this example.

```csharp
var result = await musicQueueService.GetQueue(guildId, cancellationToken);
```

---

## Models and Results

### QueueMoveResult

**File:** `DC bot/Interface/Service/Music/Queue/QueueMoveResult.cs`

**Purpose:** Reports a waiting track's previous and requested positions and the queue size.

**Record declaration:**

```csharp
public sealed record QueueMoveResult(bool Success, int From, int To, int QueueSize);
```

---

### QueueRemoveResult

**File:** `DC bot/Interface/Service/Music/Queue/QueueRemoveResult.cs`

**Purpose:** Reports removal of a numbered waiting track and the remaining queue size.

**Record declaration:**

```csharp
public sealed record QueueRemoveResult(bool Success, int TrackNumber, int QueueSize, string? RemovedTrackTitle);
```

---

### QueueShuffleResult

**File:** `DC bot/Interface/Service/Music/Queue/QueueShuffleResult.cs`

**Purpose:** Reports shuffle success and the number of waiting tracks considered.

**Record declaration:**

```csharp
public sealed record QueueShuffleResult(bool Success, int TrackCount);
```

---

## Related Components

- `DC bot/Interface/Service/Music/README.md` — Parent feature overview and neighboring components.
- `DC bot/README.md` — Project setup and architecture.
- `DC bot/Interface/Service/Music/Queue/Models/README.md` — Detailed component contracts and examples for this subfolder.
- `DC bot/Interface/Service/Music/Tracks/` — Defines the tracks boundary used by the bot music pipeline.
- `DC bot/Interface/Service/Music/Queue/Models/` — Carries inputs, statuses, and results for the queue use cases.
- `DC bot/Startup/DependencyInjection/` — Registers the runtime implementations used by these components.
