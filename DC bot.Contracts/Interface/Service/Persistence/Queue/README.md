# Queue

Defines repository operations for ordered queue entries, canonical batches, conditional start claims, and bounded playback history.

## Overview

- The bot and API consume these interfaces; concrete implementations and DI registration live in DC bot.Persistence unless noted below.
- Describe lookup absence, ordering, conditional mutation, and cancellation in the contract. Keep provider-specific SQL and EF types out of this boundary.
- The active canonical batch includes tracks appended before closure. Previous lookup excludes older batches, and repeat can restore already played canonical items. CloseBatchIfIdleAsync returning false means the caller must reconsider newly appended work.

## Interfaces

### IQueueRepository

**File:** `DC bot.Contracts/Interface/Service/Persistence/Queue/IQueueRepository.cs`

**Purpose:** Defines shared storage operations for queue repository, including caller cancellation and immutable result contracts.

**Methods:**

- `GetActiveBatchItemsAsync` — Reads the active batch's canonical items, including already played items required for full-list repetition.

```csharp
Task<IReadOnlyList<QueueItemToEnqueue>> GetActiveBatchItemsAsync(ulong guildId, CancellationToken cancellationToken = default);
```

- `GetByIdAsync` — Reads a queue item or playlist by its durable database identifier.

```csharp
Task<QueueItemRecord?> GetByIdAsync(long queueItemId, CancellationToken cancellationToken = default);
```

- `GetPendingStartsAsync` — Reads unresolved durable start claims, optionally restricted to one guild, for remote-state reconciliation.

```csharp
Task<IReadOnlyList<PendingPlaybackStart>> GetPendingStartsAsync(
    ulong? guildId = null,
    CancellationToken cancellationToken = default);
```

- `GetPreviousItemAsync` — Finds the preceding completed canonical item in the current active batch; earlier batches are excluded.

```csharp
Task<QueueItemRecord?> GetPreviousItemAsync(ulong guildId, CancellationToken cancellationToken = default);
```

- `GetQueuedItemsAsync` — Reads waiting and pending-start queue entries in durable queue position order.

```csharp
Task<IReadOnlyList<QueueItemRecord>> GetQueuedItemsAsync(ulong guildId, CancellationToken cancellationToken = default);
```

- `GetRecentBatchesAsync` — Reads bounded, recent batch history without changing playback state or the previous-track cursor.

```csharp
Task<IReadOnlyList<PlaybackBatchRecord>> GetRecentBatchesAsync(
    ulong guildId,
    int limit = 10,
    CancellationToken cancellationToken = default);
```

**Additional methods:** `TryEnableBatchRepeatAsync`, `RestartActiveBatchAsync`, `CloseBatchIfIdleAsync`, `CloseActiveBatchAsync`, `AnyQueuedItemsAsync`, `EnqueueManyAsync`, `ReorderQueuedItemsAsync`, `RemoveQueuedItemAtAsync`, `MarkAllQueuedAsSkippedAsync`, `ClaimForStartAsync`, `CompleteStartAsync`, `ReleaseStartAsync`, `CompleteAttemptAsync`, `TryMarkQueuedAsSkippedAsync`, `BindStartNodeAsync`, `ResolveDisconnectedStartAsync`, `ConfirmStartTransportAsync`. See the source and its XML contracts for the complete parameter lists.

**Usage:**

The caller supplies the component and argument values used in this example.

```csharp
var result = await queueRepository.GetActiveBatchItemsAsync(guildId, cancellationToken);
```

---

## Recovery Commit

**Purpose:** `CompleteStartAsync` commits optional `initialPosition` and `initiallyPaused` values in the same transaction as the matching queue start. Defaults retain ordinary start-at-zero, unpaused behavior. Idempotent repeats retain the committed result; a mismatched operation cannot overwrite another attempt.

**Methods:**

```csharp
Task<bool> CompleteStartAsync(
    ulong guildId,
    long queueItemId,
    Guid operationId,
    string trackIdentifier,
    string realtimeEventName,
    bool clearSourceMetadata = false,
    TimeSpan? initialPosition = null,
    bool initiallyPaused = false,
    CancellationToken cancellationToken = default);
```

---

## Related Components

- `DC bot.Contracts/Interface/Service/Persistence/README.md` — Parent feature overview and neighboring components.
- `DC bot.Contracts/README.md` — Project setup and architecture.
- `DC bot.Contracts/Interface/Service/Persistence/Models/Queue/` — Defines shared records and state values for ordered queue entries, canonical batches, conditional start claims, and bounded playback history.
- `DC bot.Persistence/` — Repository implementations and database mappings that consume these shared contracts.
