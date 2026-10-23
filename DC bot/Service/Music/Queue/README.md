# Queue

Maps track requests to durable queue items and batch-aware queue operations.

## Overview

- MusicQueueService uses ITrackSerializer and IQueueRepository for enqueue, reads, removal, shuffle, move, repetition, and history-related state.
- An active canonical batch includes tracks appended before it closes. Queue order and canonical batch order serve different purposes; changing waiting positions must not silently redefine history.

## Classes

### MusicQueueService

**File:** `DC bot/Service/Music/Queue/MusicQueueService.cs`

**Purpose:** Restores durable queue entries and applies queue edits and canonical batch-repeat operations.

**Implements / inherits:** `IMusicQueueService`.

**Methods:**

- `GetQueue` — Restores waiting tracks and marks unparseable persisted queue entries skipped.

```csharp
public async Task<Queue<ILavaLinkTrack>> GetQueue(ulong guildId, CancellationToken cancellationToken = default);
```

- `GetRepeatableQueue` — Restores all canonical items of the active batch, including played tracks, logging unparseable entries.

```csharp
public async Task<IReadOnlyCollection<ILavaLinkTrack>> GetRepeatableQueue(
    ulong guildId,
    CancellationToken cancellationToken = default);
```

- `TryEnableBatchRepeatAsync` — Atomically enables repetition of the live canonical batch when it contains active playback work.

```csharp
public Task<bool> TryEnableBatchRepeatAsync(ulong guildId, CancellationToken cancellationToken = default);
```

- `ClearQueue` — Marks waiting tracks skipped while preserving the playing track and canonical batch history.

```csharp
public async Task ClearQueue(ulong guildId, CancellationToken cancellationToken = default);
```

- `EnqueueMany` — Serializes incoming tracks and appends their source metadata to the active batch and waiting queue.

```csharp
public async Task EnqueueMany(
    ulong guildId,
    IReadOnlyCollection<QueueTrackToEnqueue> tracks,
    CancellationToken cancellationToken = default);
```

- `HasTracks` — Reports whether the guild has durable waiting queue entries.

```csharp
public async Task<bool> HasTracks(ulong guildId, CancellationToken cancellationToken = default);
```

**Additional methods:** `ViewQueue`, `SetQueue`, `ShuffleQueue`, `RemoveAt`, `MoveToIndex`. See the source and its XML contracts for the complete parameter lists.

**Usage:**

The caller supplies the component and argument values used in this example.

```csharp
var result = await musicQueueService.GetQueue(guildId, cancellationToken);
```

---

## Related Components

- `DC bot/Service/Music/README.md` — Parent feature overview and neighboring components.
- `DC bot/README.md` — Project setup and architecture.
- `DC bot/Service/Music/Tracks/` — Adapts Lavalink tracks to the persisted identity format used by queue and playlist storage.
- `DC bot/Interface/Service/Music/Queue/` — Defines the queue boundary used by the bot music pipeline.
- `DC bot/Interface/Service/Music/Tracks/` — Defines the tracks boundary used by the bot music pipeline.
- `DC bot/Interface/Service/Music/Queue/Models/` — Carries inputs, statuses, and results for the queue use cases.
- `DC bot/Startup/DependencyInjection/` — Registers the runtime implementations used by these components.
