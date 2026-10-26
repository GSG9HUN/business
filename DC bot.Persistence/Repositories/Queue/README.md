# Queue

Implements shared storage operations for ordered queue entries, canonical batches, conditional start claims, and bounded playback history.

## Overview

- Callers resolve the contracts through AddPersistenceServices; implementations create contexts through IDbContextFactory and map database state into shared records.
- Forward caller cancellation through database I/O. Preserve transactional ownership and use the shared retry/rollback helpers where a multi-step mutation requires them.
- QueueRepository delegates atomic start ownership to QueueClaimService and canonical repeat/history to QueueBatchRepository. QueueBatchStore coordinates active batch state inside transactions; QueueRealtimeNotifier signals readers after relevant changes.

## Classes

### QueueBatchRepository

**File:** `DC bot.Persistence/Repositories/Queue/QueueBatchRepository.cs`

**Purpose:** Coordinates canonical batch repetition, closure, and history through short-lived EF contexts.

**Methods:**

- `GetItemsAsync` — Reads canonical items of the active batch in canonical playback order.

```csharp
internal Task<IReadOnlyList<QueueItemToEnqueue>> GetItemsAsync(ulong guildId, CancellationToken ct);
```

- `GetRecentAsync` — Reads recent playback batches with a bounded history limit and canonical track counts.

```csharp
internal async Task<IReadOnlyList<PlaybackBatchRecord>> GetRecentAsync(ulong guildId, int limit, CancellationToken ct);
```

- `TryEnableRepeatAsync` — Enables repeat for the live canonical batch only when active items exist and track repeat is disabled.

```csharp
internal Task<bool> TryEnableRepeatAsync(ulong guildId, CancellationToken ct);
```

- `CloseAsync` — Closes an idle or explicitly left batch according to the caller's ownership and active-item policy.

```csharp
internal Task<bool> CloseAsync(ulong guildId, bool explicitLeave, CancellationToken ct);
```

- `RestartAsync` — Re-enqueues the entire canonical batch only when list repeat is enabled and no active items remain.

```csharp
internal Task<bool> RestartAsync(ulong guildId, CancellationToken ct);
```

**Usage:**

The caller supplies the component and argument values used in this example.

```csharp
var result = await queueBatchRepository.GetItemsAsync(guildId, ct);
```

---

### QueueBatchStore

**File:** `DC bot.Persistence/Repositories/Queue/QueueBatchStore.cs`

**Purpose:** Maintains canonical batch membership and legacy adoption within the caller's current database context and transaction.

**Methods:**

- `GetActiveAsync` — Resolves or creates the active batch, adopting eligible legacy queue and repeat-snapshot entries when necessary.

```csharp
internal static async Task<PlaybackBatchEntity?> GetActiveAsync(
    BotDbContext db,
    ulong guildId,
    bool create,
    CancellationToken ct);
```

- `CloseAsync` — Closes an idle or explicitly left batch according to the caller's ownership and active-item policy.

```csharp
internal static async Task<bool> CloseAsync(BotDbContext db, ulong guildId, bool explicitLeave, CancellationToken ct);
```

- `HasActiveItemsAsync` — Checks for queued, starting, or playing items belonging to the selected batch.

```csharp
internal static Task<bool> HasActiveItemsAsync(BotDbContext db, long batchId, CancellationToken ct);
```

**Usage:**

The caller supplies the component and argument values used in this example.

```csharp
var result = await QueueBatchStore.GetActiveAsync(db, guildId, create, ct);
```

---

### QueueClaimService

**File:** `DC bot.Persistence/Repositories/Queue/QueueClaimService.cs`

**Purpose:** Applies operation-fenced queue start and completion transitions inside serializable database transactions.

**Methods:**

- `ClaimForStartAsync` — Atomically claims the requested or next eligible queue item; reuses the same pending operation and rejects a competing pending claim.

```csharp
public Task<QueueClaimResult> ClaimForStartAsync(
    ulong guildId,
    Guid operationId,
    long? queueItemId = null,
    CancellationToken cancellationToken = default);
```

- `CompleteAttemptAsync` — Completes only the matching playing attempt as played or skipped so late or duplicate events cannot consume a newer attempt.

```csharp
public Task<bool> CompleteAttemptAsync(
    ulong guildId,
    long queueItemId,
    Guid operationId,
    bool played,
    CancellationToken cancellationToken = default);
```

- `CompleteStartAsync` — Idempotently commits a matching start claim as playing, updates current-track state, and emits queue and playback events.

```csharp
public Task<bool> CompleteStartAsync(
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

- `ReleaseStartAsync` — Conditionally releases a matching pending start by restoring its previous state or marking it skipped.

```csharp
public Task<bool> ReleaseStartAsync(
    ulong guildId,
    long queueItemId,
    Guid operationId,
    bool skip,
    CancellationToken cancellationToken = default);
```

**Usage:**

The caller supplies the component and argument values used in this example.

```csharp
var result = await queueClaimService.ClaimForStartAsync(guildId, operationId, queueItemId, cancellationToken);
```

---

### QueueItemMapper

**File:** `DC bot.Persistence/Repositories/Queue/QueueItemMapper.cs`

**Purpose:** Maps queue entities to immutable contracts while preserving source, batch, and playback-operation metadata.

**Methods:**

- `ToRecord` — Maps a tracked queue entity into its immutable queue contract including batch, source, and operation identity.

```csharp
internal static QueueItemRecord ToRecord(GuildQueueItemEntity entity);
```

**Usage:**

The caller supplies the component and argument values used in this example.

```csharp
var result = QueueItemMapper.ToRecord(entity);
```

---

### QueueRealtimeNotifier

**File:** `DC bot.Persistence/Repositories/Queue/QueueRealtimeNotifier.cs`

**Purpose:** Publishes queue realtime notifications within the caller's database transaction.

**Methods:**

- `NotifyQueueUpdatedAsync` — Publishes a queue event within the caller's database transaction when relational notifications are supported.

```csharp
public static Task NotifyQueueUpdatedAsync(
    BotDbContext dbContext,
    ulong guildId,
    string eventName,
    CancellationToken cancellationToken);
```

**Usage:**

The caller supplies the component and argument values used in this example.

```csharp
await QueueRealtimeNotifier.NotifyQueueUpdatedAsync(dbContext, guildId, eventName, cancellationToken);
```

---

### QueueRepository

**File:** `DC bot.Persistence/Repositories/Queue/QueueRepository.cs`

**Purpose:** Exposes durable queue edits, canonical batch history, and fenced playback-start operations through injected persistence helpers.

**Implements / inherits:** `IQueueRepository`.

**Methods:**

- `GetActiveBatchItemsAsync` — Reads the active batch's canonical items, including already played items required for full-list repetition.

```csharp
public Task<IReadOnlyList<QueueItemToEnqueue>> GetActiveBatchItemsAsync(
    ulong guildId,
    CancellationToken cancellationToken = default);
```

- `GetByIdAsync` — Reads a queue item or playlist by its durable database identifier.

```csharp
public async Task<QueueItemRecord?> GetByIdAsync(long queueItemId, CancellationToken cancellationToken = default);
```

- `GetPendingStartsAsync` — Reads unresolved durable start claims, optionally restricted to one guild, for remote-state reconciliation.

```csharp
public async Task<IReadOnlyList<PendingPlaybackStart>> GetPendingStartsAsync(
    ulong? guildId = null,
    CancellationToken cancellationToken = default);
```

- `GetPreviousItemAsync` — Finds the preceding completed canonical item in the current active batch; earlier batches are excluded.

```csharp
public async Task<QueueItemRecord?> GetPreviousItemAsync(ulong guildId, CancellationToken cancellationToken = default);
```

- `GetQueuedItemsAsync` — Reads waiting and pending-start queue entries in durable queue position order.

```csharp
public async Task<IReadOnlyList<QueueItemRecord>> GetQueuedItemsAsync(
    ulong guildId,
    CancellationToken cancellationToken = default);
```

- `GetRecentBatchesAsync` — Reads bounded, recent batch history without changing playback state or the previous-track cursor.

```csharp
public Task<IReadOnlyList<PlaybackBatchRecord>> GetRecentBatchesAsync(
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

- `DC bot.Persistence/Repositories/README.md` — Parent feature overview and neighboring components.
- `DC bot.Persistence/README.md` — Project setup and architecture.
- `DC bot.Persistence/Db/` — Owns the EF Core context, design-time creation, and startup migration runner.
- `DC bot.Persistence/Entities/Playback/` — Represents the database rows used for current playback state, repeat flags, start ownership, and canonical batch metadata.
- `DC bot.Persistence/Entities/Queue/` — Represents the database rows used for ordered queue entries, canonical batches, conditional start claims, and bounded playback history.
- `DC bot.Persistence/Repositories/Guilds/` — Implements shared storage operations for guild defaults, premium state, and the mapped premium-audit schema.
- `DC bot.Contracts/Interface/Service/Persistence/` — Shared interfaces and immutable values implemented by persistence.
