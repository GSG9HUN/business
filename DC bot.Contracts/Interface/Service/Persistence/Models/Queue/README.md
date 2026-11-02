# Queue

Defines shared records and state values for ordered queue entries, canonical batches, conditional start claims, and bounded playback history.

## Overview

- Persistence maps entities into these values, and application services or API mappers consume them without database-provider dependencies.
- Keep wire response DTOs in API and tracked entities in DC bot.Persistence. Changing a record affects both repository implementations and their consumers.
- Queue position orders waiting playback; batch metadata preserves the canonical sequence. Start claims carry operation ownership so late completion cannot commit against a newer attempt.

## Models and Results

### PendingPlaybackStart

**File:** `DC bot.Contracts/Interface/Service/Persistence/Models/Queue/PendingPlaybackStart.cs`

**Purpose:** Carries immutable pending playback start values across the shared storage or mobile-command boundary.

**Record declaration:**

```csharp
public sealed record PendingPlaybackStart(
    ulong GuildId,
    long QueueItemId,
    Guid OperationId,
    string? NodeAddress,
    string? NodeSessionId,
    bool TransportConfirmed);
```

---

### PlaybackBatchRecord

**File:** `DC bot.Contracts/Interface/Service/Persistence/Models/Queue/PlaybackBatchRecord.cs`

**Purpose:** Carries immutable playback batch values across the shared storage or mobile-command boundary.

**Record declaration:**

```csharp
public sealed record PlaybackBatchRecord(long Id, DateTimeOffset CreatedAtUtc, DateTimeOffset? CompletedAtUtc, int TrackCount);
```

---

### QueueClaimResult

**File:** `DC bot.Contracts/Interface/Service/Persistence/Models/Queue/QueueClaimResult.cs`

**Purpose:** Carries immutable queue claim result values across the shared storage or mobile-command boundary.

**Record declaration:**

```csharp
public sealed record QueueClaimResult(QueueClaimStatus Status, QueueStartClaim? Claim = null);
```

---

### QueueClaimStatus

**File:** `DC bot.Contracts/Interface/Service/Persistence/Models/Queue/QueueClaimStatus.cs`

**Purpose:** Names the outcomes of an atomic start claim so callers can distinguish a successful reservation, an empty queue, and work owned by another operation.

**Values:**

```csharp
public enum QueueClaimStatus
{
    // A queue item was successfully reserved for playback.
    Claimed,
    // No item is available for the requested operation.
    Empty,
    // Another operation currently owns the required playback state.
    Busy
}
```

---

### QueueItemRecord

**File:** `DC bot.Contracts/Interface/Service/Persistence/Models/Queue/QueueItemRecord.cs`

**Purpose:** Carries immutable queue item values across the shared storage or mobile-command boundary.

**Record declaration:**

```csharp
public sealed record QueueItemRecord(
    long Id,
    ulong GuildId,
    int Position,
    string TrackIdentifier,
    string? SourceQuery,
    string? SourceSearchMode,
    string? RequestedBy,
    QueueItemState State,
    long? BatchId,
    long? BatchItemId,
    DateTimeOffset AddedAtUtc,
    DateTimeOffset? PlayedAtUtc,
    DateTimeOffset? SkippedAtUtc);
```

---

### QueueItemRemovalRecord

**File:** `DC bot.Contracts/Interface/Service/Persistence/Models/Queue/QueueItemRemovalRecord.cs`

**Purpose:** Carries immutable queue item removal values across the shared storage or mobile-command boundary.

**Record declaration:**

```csharp
public sealed record QueueItemRemovalRecord(bool Success, int QueueSize, QueueItemRecord? RemovedItem);
```

---

### QueueItemState

**File:** `DC bot.Contracts/Interface/Service/Persistence/Models/Queue/QueueItemState.cs`

**Purpose:** Names the persisted lifecycle states of a queue item rather than representing state transitions as arbitrary text.

**Values:**

```csharp
public enum QueueItemState : short
{
    // The item is waiting for playback.
    Queued = 0,
    // The item is currently playing.
    Playing = 1,
    // Playback of the item has completed.
    Played = 2,
    // The item was skipped instead of completing normally.
    Skipped = 3,
    // The item is reserved for a playback start whose outcome is not yet settled.
    Starting = 4
}
```

---

### QueueItemToEnqueue

**File:** `DC bot.Contracts/Interface/Service/Persistence/Models/Queue/QueueItemToEnqueue.cs`

**Purpose:** Carries immutable queue item to enqueue values across the shared storage or mobile-command boundary.

**Record declaration:**

```csharp
public sealed record QueueItemToEnqueue(
    string TrackIdentifier,
    string? SourceQuery,
    string? SourceSearchMode,
    string? RequestedBy);
```

---

### QueueStartClaim

**File:** `DC bot.Contracts/Interface/Service/Persistence/Models/Queue/QueueStartClaim.cs`

**Purpose:** Carries immutable queue start claim values across the shared storage or mobile-command boundary.

**Record declaration:**

```csharp
public sealed record QueueStartClaim(Guid OperationId, QueueItemRecord Item);
```

---

## Related Components

- `DC bot.Contracts/Interface/Service/Persistence/Models/README.md` — Parent feature overview and neighboring components.
- `DC bot.Contracts/README.md` — Project setup and architecture.
- `DC bot.Persistence/` — Repository implementations and database mappings that consume these shared contracts.
