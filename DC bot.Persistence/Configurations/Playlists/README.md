# Playlists

Defines EF Core table mappings for saved playlist metadata and ordered, serialized playlist tracks.

## Overview

- These mappings connect the corresponding Entities feature to PostgreSQL columns, keys, indexes, and relationships used by repositories.
- Change entity shape and mapping together. Generate a new migration for schema changes rather than rewriting an already-applied migration.

## Classes

### PlaylistConfiguration

**File:** `DC bot.Persistence/Configurations/Playlists/PlaylistConfiguration.cs`

**Purpose:** Configures PostgreSQL storage, keys, and relationships for playlist.

**Implements / inherits:** `IEntityTypeConfiguration<PlaylistEntity>`.

**Methods:**

- `Configure` — Implements `Configure` using the contract shown below.

```csharp
public void Configure(Microsoft.EntityFrameworkCore.Metadata.Builders.EntityTypeBuilder<PlaylistEntity> builder);
```

**Usage:**

The caller supplies the component and argument values used in this example.

```csharp
playlistConfiguration.Configure(builder);
```

---

### PlaylistTrackConfiguration

**File:** `DC bot.Persistence/Configurations/Playlists/PlaylistTrackConfiguration.cs`

**Purpose:** Configures PostgreSQL storage, keys, and relationships for playlist track.

**Implements / inherits:** `IEntityTypeConfiguration<PlaylistTrackEntity>`.

**Methods:**

- `Configure` — Implements `Configure` using the contract shown below.

```csharp
public void Configure(EntityTypeBuilder<PlaylistTrackEntity> builder);
```

**Usage:**

The caller supplies the component and argument values used in this example.

```csharp
playlistTrackConfiguration.Configure(builder);
```

---

## Related Components

- `DC bot.Persistence/Configurations/README.md` — Parent feature overview and neighboring components.
- `DC bot.Persistence/README.md` — Project setup and architecture.
- `DC bot.Persistence/Configurations/Shared/` — Centralizes the Discord guild identifier conversion used by entity mappings.
- `DC bot.Persistence/Entities/Playlists/` — Represents the database rows used for saved playlist metadata and ordered, serialized playlist tracks.
- `DC bot.Contracts/Interface/Service/Persistence/` — Shared interfaces and immutable values implemented by persistence.
