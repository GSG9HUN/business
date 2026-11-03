# GuildBotStatus

Defines EF Core table mappings for persisted bot voice presence, channel display metadata, and voice-user counts.

## Overview

- These mappings connect the corresponding Entities feature to PostgreSQL columns, keys, indexes, and relationships used by repositories.
- Change entity shape and mapping together. Generate a new migration for schema changes rather than rewriting an already-applied migration.

## Classes

### GuildBotStatusConfiguration

**File:** `DC bot.Persistence/Configurations/GuildBotStatus/GuildBotStatusConfiguration.cs`

**Purpose:** Configures PostgreSQL storage, keys, and relationships for guild bot status.

**Implements / inherits:** `IEntityTypeConfiguration<GuildBotStatusEntity>`.

**Methods:**

- `Configure` — Implements `Configure` using the contract shown below.

```csharp
public void Configure(Microsoft.EntityFrameworkCore.Metadata.Builders.EntityTypeBuilder<GuildBotStatusEntity> builder);
```

**Usage:**

The caller supplies the component and argument values used in this example.

```csharp
guildBotStatusConfiguration.Configure(builder);
```

---

## Related Components

- `DC bot.Persistence/Configurations/README.md` — Parent feature overview and neighboring components.
- `DC bot.Persistence/README.md` — Project setup and architecture.
- `DC bot.Persistence/Configurations/Shared/` — Centralizes the Discord guild identifier conversion used by entity mappings.
- `DC bot.Persistence/Entities/GuildBotStatus/` — Represents the database rows used for persisted bot voice presence, channel display metadata, and voice-user counts.
- `DC bot.Contracts/Interface/Service/Persistence/` — Shared interfaces and immutable values implemented by persistence.
