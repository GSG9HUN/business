# Playback

Represents the database rows used for current playback state, repeat flags, start ownership, and canonical batch metadata.

## Overview

- Feature repositories read and mutate these values in short-lived EF contexts, then map results into DC bot.Contracts records.
- Entity properties describe persisted state. Keep HTTP response formatting, Discord objects, and playback transport behavior outside these types.
- PlaybackBatchEntity and PlaybackBatchItemEntity retain the canonical playback sequence independently of waiting queue order. GuildPlaybackStateEntity also stores operation ownership and remote node binding needed for reconciliation.

## Classes

### GuildPlaybackStateEntity

**File:** `DC bot.Persistence/Entities/Playback/GuildPlaybackStateEntity.cs`

**Purpose:** Represents the persisted guild playback state row mapped by the EF Core model.

**Properties:**

```csharp
// The Discord guild identifier.
public ulong GuildId { get; set; }
public bool IsRepeating { get; set; }
public bool IsRepeatingList { get; set; }
public bool IsPaused { get; set; }
public int PositionSeconds { get; set; }
public DateTimeOffset? PositionUpdatedAtUtc { get; set; }
public string? CurrentTrackIdentifier { get; set; }
public long? QueueItemId { get; set; }
public long? ActiveBatchId { get; set; }
public DateTimeOffset UpdatedAtUtc { get; set; }
public GuildDataEntity Guild { get; set; }
```

---

### GuildRepeatListItemEntity

**File:** `DC bot.Persistence/Entities/Playback/GuildRepeatListItemEntity.cs`

**Purpose:** Represents the persisted guild repeat list item row mapped by the EF Core model.

**Properties:**

```csharp
// The identifier of this persisted record.
public long Id { get; set; }
public ulong GuildId { get; set; }
public int Position { get; set; }
public string TrackIdentifier { get; set; }
public DateTimeOffset AddedAtUtc { get; set; }
public GuildDataEntity Guild { get; set; }
```

---

### PlaybackBatchEntity

**File:** `DC bot.Persistence/Entities/Playback/PlaybackBatchEntity.cs`

**Purpose:** Represents the persisted playback batch row mapped by the EF Core model.

**Properties:**

```csharp
// The identifier of this persisted record.
public long Id { get; set; }
public ulong GuildId { get; set; }
public DateTimeOffset CreatedAtUtc { get; set; }
public DateTimeOffset? CompletedAtUtc { get; set; }
public GuildDataEntity Guild { get; set; }
```

---

### PlaybackBatchItemEntity

**File:** `DC bot.Persistence/Entities/Playback/PlaybackBatchItemEntity.cs`

**Purpose:** Represents the persisted playback batch item row mapped by the EF Core model.

**Properties:**

```csharp
// The identifier of this persisted record.
public long Id { get; set; }
public long BatchId { get; set; }
public int Position { get; set; }
public string TrackIdentifier { get; set; }
public string? SourceQuery { get; set; }
public string? SourceSearchMode { get; set; }
public string? RequestedBy { get; set; }
public DateTimeOffset AddedAtUtc { get; set; }
public PlaybackBatchEntity Batch { get; set; }
```

---

## Related Components

- `DC bot.Persistence/Entities/README.md` — Parent feature overview and neighboring components.
- `DC bot.Persistence/README.md` — Project setup and architecture.
- `DC bot.Persistence/Entities/Guilds/` — Represents the database rows used for guild defaults, premium state, and the mapped premium-audit schema.
- `DC bot.Contracts/Interface/Service/Persistence/` — Shared interfaces and immutable values implemented by persistence.
