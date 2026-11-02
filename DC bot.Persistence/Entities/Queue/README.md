# Queue

Represents the database rows used for ordered queue entries, canonical batches, conditional start claims, and bounded playback history.

## Overview

- Feature repositories read and mutate these values in short-lived EF contexts, then map results into DC bot.Contracts records.
- Entity properties describe persisted state. Keep HTTP response formatting, Discord objects, and playback transport behavior outside these types.

## Classes

### GuildQueueItemEntity

**File:** `DC bot.Persistence/Entities/Queue/GuildQueueItemEntity.cs`

**Purpose:** Represents the persisted guild queue item row mapped by the EF Core model.

**Properties:**

```csharp
// The identifier of this persisted record.
public long Id { get; set; }
public ulong GuildId { get; set; }
public long? BatchId { get; set; }
public long? BatchItemId { get; set; }
public int Position { get; set; }
public string TrackIdentifier { get; set; }
public string? SourceQuery { get; set; }
public string? SourceSearchMode { get; set; }
public string? RequestedBy { get; set; }
public Guid? PlaybackOperationId { get; set; }
public QueueItemState? StartPreviousState { get; set; }
public Guid? StartPreviousOperationId { get; set; }
public string? PlaybackNodeAddress { get; set; }
public string? PlaybackNodeSessionId { get; set; }
public bool PlaybackTransportConfirmed { get; set; }
public QueueItemState State { get; set; }
public DateTimeOffset AddedAtUtc { get; set; }
public DateTimeOffset? PlayedAtUtc { get; set; }
public DateTimeOffset? SkippedAtUtc { get; set; }
public GuildDataEntity Guild { get; set; }
```

---

## Related Components

- `DC bot.Persistence/Entities/README.md` — Parent feature overview and neighboring components.
- `DC bot.Persistence/README.md` — Project setup and architecture.
- `DC bot.Persistence/Entities/Guilds/` — Represents the database rows used for guild defaults, premium state, and the mapped premium-audit schema.
- `DC bot.Contracts/Interface/Service/Persistence/Models/Queue/` — Defines shared records and state values for ordered queue entries, canonical batches, conditional start claims, and bounded playback history.
- `DC bot.Contracts/Interface/Service/Persistence/` — Shared interfaces and immutable values implemented by persistence.
