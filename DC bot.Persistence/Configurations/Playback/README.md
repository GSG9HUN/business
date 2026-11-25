# Playback

Defines EF Core table mappings for current playback state, repeat flags, start ownership, and canonical batch metadata.

## Overview

Canonical batch items map nullable `RequestedByDisplayName` to `requested_by_display_name` with a maximum length of 128. The `AddRequesterDisplayNames` migration introduces this metadata alongside the queue occurrence column.

`GuildPlaybackStateConfiguration` maps nullable `VolumePercent` to `guild_playback_state.volume_percent`. The `AddPlaybackVolume` migration introduces the column without inventing a confirmed default.

- These mappings connect the corresponding Entities feature to PostgreSQL columns, keys, indexes, and relationships used by repositories.
- Change entity shape and mapping together. Generate a new migration for schema changes rather than rewriting an already-applied migration.

**Staging Integration**

PlaybackBatchItemConfiguration maps is_removed and enqueue_group_id. Canonical batch keys and the unique batch/position index are retained.

## Classes

### GuildPlaybackStateConfiguration

**File:** `DC bot.Persistence/Configurations/Playback/GuildPlaybackStateConfiguration.cs`

**Purpose:** Configures PostgreSQL storage, keys, and relationships for guild playback state.

**Implements / inherits:** `IEntityTypeConfiguration<GuildPlaybackStateEntity>`.

**Methods:**

- `Configure` — Implements `Configure` using the contract shown below.

```csharp
public void Configure(EntityTypeBuilder<GuildPlaybackStateEntity> builder);
```

**Usage:**

The caller supplies the component and argument values used in this example.

```csharp
guildPlaybackStateConfiguration.Configure(builder);
```

---

### GuildRepeatListItemConfiguration

**File:** `DC bot.Persistence/Configurations/Playback/GuildRepeatListItemConfiguration.cs`

**Purpose:** Configures PostgreSQL storage, keys, and relationships for guild repeat list item.

**Implements / inherits:** `IEntityTypeConfiguration<GuildRepeatListItemEntity>`.

**Methods:**

- `Configure` — Implements `Configure` using the contract shown below.

```csharp
public void Configure(EntityTypeBuilder<GuildRepeatListItemEntity> builder);
```

**Usage:**

The caller supplies the component and argument values used in this example.

```csharp
guildRepeatListItemConfiguration.Configure(builder);
```

---

### PlaybackBatchConfiguration

**File:** `DC bot.Persistence/Configurations/Playback/PlaybackBatchConfiguration.cs`

**Purpose:** Configures PostgreSQL storage, keys, and relationships for playback batch.

**Implements / inherits:** `IEntityTypeConfiguration<PlaybackBatchEntity>`.

**Methods:**

- `Configure` — Implements `Configure` using the contract shown below.

```csharp
public void Configure(EntityTypeBuilder<PlaybackBatchEntity> builder);
```

**Usage:**

The caller supplies the component and argument values used in this example.

```csharp
playbackBatchConfiguration.Configure(builder);
```

---

### PlaybackBatchItemConfiguration

**File:** `DC bot.Persistence/Configurations/Playback/PlaybackBatchItemConfiguration.cs`

**Purpose:** Configures PostgreSQL storage, keys, and relationships for playback batch item.

**Implements / inherits:** `IEntityTypeConfiguration<PlaybackBatchItemEntity>`.

**Methods:**

- `Configure` — Implements `Configure` using the contract shown below.

```csharp
public void Configure(EntityTypeBuilder<PlaybackBatchItemEntity> builder);
```

**Usage:**

The caller supplies the component and argument values used in this example.

```csharp
playbackBatchItemConfiguration.Configure(builder);
```

---

## Related Components

- `DC bot.Persistence/Configurations/README.md` — Parent feature overview and neighboring components.
- `DC bot.Persistence/README.md` — Project setup and architecture.
- `DC bot.Persistence/Configurations/Shared/` — Centralizes the Discord guild identifier conversion used by entity mappings.
- `DC bot.Persistence/Entities/Playback/` — Represents the database rows used for current playback state, repeat flags, start ownership, and canonical batch metadata.
- `DC bot.Contracts/Interface/Service/Persistence/` — Shared interfaces and immutable values implemented by persistence.
