# Playlists

Defines shared records and state values for saved playlist metadata and ordered, serialized playlist tracks.

## Overview

- Persistence maps entities into these values, and application services or API mappers consume them without database-provider dependencies.
- Keep wire response DTOs in API and tracked entities in DC bot.Persistence. Changing a record affects both repository implementations and their consumers.

## Models and Results

### PlaylistRecord

**File:** `DC bot.Contracts/Interface/Service/Persistence/Models/Playlists/PlaylistRecord.cs`

**Purpose:** Carries immutable playlist values across the shared storage or mobile-command boundary.

**Record declaration:**

```csharp
public sealed record PlaylistRecord(long Id, ulong GuildId, string Name);
```

---

### PlaylistSummaryRecord

**File:** `DC bot.Contracts/Interface/Service/Persistence/Models/Playlists/PlaylistSummaryRecord.cs`

**Purpose:** Carries immutable playlist summary values across the shared storage or mobile-command boundary.

**Record declaration:**

```csharp
public sealed record PlaylistSummaryRecord(long Id, ulong GuildId, string Name, int TrackCount);
```

---

### PlaylistTrackCreateRecord

**File:** `DC bot.Contracts/Interface/Service/Persistence/Models/Playlists/PlaylistTrackCreateRecord.cs`

**Purpose:** Carries immutable playlist track create values across the shared storage or mobile-command boundary.

**Record declaration:**

```csharp
public sealed record PlaylistTrackCreateRecord(string Source, string TrackIdentifier, string TrackUri);
```

---

### PlaylistTrackRecord

**File:** `DC bot.Contracts/Interface/Service/Persistence/Models/Playlists/PlaylistTrackRecord.cs`

**Purpose:** Carries immutable playlist track values across the shared storage or mobile-command boundary.

**Record declaration:**

```csharp
public sealed record PlaylistTrackRecord(
    long Id,
    long PlaylistId,
    int OrderNumber,
    string Source,
    string TrackIdentifier,
    string TrackUri);
```

---

## Related Components

- `DC bot.Contracts/Interface/Service/Persistence/Models/README.md` — Parent feature overview and neighboring components.
- `DC bot.Contracts/README.md` — Project setup and architecture.
- `DC bot.Persistence/` — Repository implementations and database mappings that consume these shared contracts.
