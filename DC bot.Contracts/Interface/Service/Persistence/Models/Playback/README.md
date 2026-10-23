# Playback

Defines shared records and state values for current playback state, repeat flags, start ownership, and canonical batch metadata.

## Overview

- Persistence maps entities into these values, and application services or API mappers consume them without database-provider dependencies.
- Keep wire response DTOs in API and tracked entities in DC bot.Persistence. Changing a record affects both repository implementations and their consumers.

## Models and Results

### PlaybackStateRecord

**File:** `DC bot.Contracts/Interface/Service/Persistence/Models/Playback/PlaybackStateRecord.cs`

**Purpose:** Carries immutable playback state values across the shared storage or mobile-command boundary.

**Record declaration:**

```csharp
public record PlaybackStateRecord(
    ulong GuildId,
    bool IsRepeating,
    bool IsRepeatingList,
    string? CurrentTrackIdentifier,
    long? QueueItemId,
    DateTimeOffset UpdatedAtUtc,
    bool IsPaused = false,
    int PositionSeconds = 0,
    DateTimeOffset? PositionUpdatedAtUtc = null);
```

---

## Related Components

- `DC bot.Contracts/Interface/Service/Persistence/Models/README.md` — Parent feature overview and neighboring components.
- `DC bot.Contracts/README.md` — Project setup and architecture.
- `DC bot.Persistence/` — Repository implementations and database mappings that consume these shared contracts.
