# Queue

Defines EF Core table mappings for ordered queue entries, canonical batches, conditional start claims, and bounded playback history.

## Overview

- These mappings connect the corresponding Entities feature to PostgreSQL columns, keys, indexes, and relationships used by repositories.
- Change entity shape and mapping together. Generate a new migration for schema changes rather than rewriting an already-applied migration.

## Classes

### GuildQueueItemConfiguration

**File:** `DC bot.Persistence/Configurations/Queue/GuildQueueItemConfiguration.cs`

**Purpose:** Configures PostgreSQL storage, keys, and relationships for guild queue item.

**Implements / inherits:** `IEntityTypeConfiguration<GuildQueueItemEntity>`.

**Methods:**

- `Configure` — Implements `Configure` using the contract shown below.

```csharp
public void Configure(EntityTypeBuilder<GuildQueueItemEntity> builder);
```

**Usage:**

The caller supplies the component and argument values used in this example.

```csharp
guildQueueItemConfiguration.Configure(builder);
```

---

## Related Components

- `DC bot.Persistence/Configurations/README.md` — Parent feature overview and neighboring components.
- `DC bot.Persistence/README.md` — Project setup and architecture.
- `DC bot.Persistence/Configurations/Shared/` — Centralizes the Discord guild identifier conversion used by entity mappings.
- `DC bot.Persistence/Entities/Queue/` — Represents the database rows used for ordered queue entries, canonical batches, conditional start claims, and bounded playback history.
- `DC bot.Contracts/Interface/Service/Persistence/Models/Queue/` — Defines shared records and state values for ordered queue entries, canonical batches, conditional start claims, and bounded playback history.
- `DC bot.Contracts/Interface/Service/Persistence/` — Shared interfaces and immutable values implemented by persistence.
