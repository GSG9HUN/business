# Playlists

Represents the database rows used for saved playlist metadata and ordered, serialized playlist tracks.

## Overview

- Feature repositories read and mutate these values in short-lived EF contexts, then map results into DC bot.Contracts records.
- Entity properties describe persisted state. Keep HTTP response formatting, Discord objects, and playback transport behavior outside these types.

## Classes

### PlaylistEntity

**File:** `DC bot.Persistence/Entities/Playlists/PlaylistEntity.cs`

**Purpose:** Represents the persisted playlist row mapped by the EF Core model.

**Properties:**

```csharp
// The identifier of this persisted record.
public long Id { get; set; }
public ulong GuildId { get; set; }
public string Name { get; set; }
public GuildDataEntity Guild { get; set; }
public ICollection<PlaylistTrackEntity> Tracks { get; set; }
```

---

### PlaylistTrackEntity

**File:** `DC bot.Persistence/Entities/Playlists/PlaylistTrackEntity.cs`

**Purpose:** Represents the persisted playlist track row mapped by the EF Core model.

**Properties:**

```csharp
// The identifier of this persisted record.
public long Id { get; set; }
public long PlaylistId { get; set; }
public int OrderNumber { get; set; }
public string Source { get; set; }
public string TrackIdentifier { get; set; }
public string TrackUri { get; set; }
public PlaylistEntity Playlist { get; set; }
```

---

## Related Components

- `DC bot.Persistence/Entities/README.md` — Parent feature overview and neighboring components.
- `DC bot.Persistence/README.md` — Project setup and architecture.
- `DC bot.Persistence/Entities/Guilds/` — Represents the database rows used for guild defaults, premium state, and the mapped premium-audit schema.
- `DC bot.Contracts/Interface/Service/Persistence/` — Shared interfaces and immutable values implemented by persistence.
