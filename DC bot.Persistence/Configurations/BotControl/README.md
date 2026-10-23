# BotControl

Defines EF Core table mappings for durable bot commands, their claim/completion state, and cross-process change signals.

## Overview

- These mappings connect the corresponding Entities feature to PostgreSQL columns, keys, indexes, and relationships used by repositories.
- Change entity shape and mapping together. Generate a new migration for schema changes rather than rewriting an already-applied migration.

## Classes

### BotControlCommandsConfiguration

**File:** `DC bot.Persistence/Configurations/BotControl/BotControlCommandsConfiguration.cs`

**Purpose:** Configures PostgreSQL storage, keys, and relationships for bot control commands.

**Implements / inherits:** `IEntityTypeConfiguration<BotControlCommandEntity>`.

**Methods:**

- `Configure` — Implements `Configure` using the contract shown below.

```csharp
public void Configure(EntityTypeBuilder<BotControlCommandEntity> builder);
```

**Usage:**

The caller supplies the component and argument values used in this example.

```csharp
botControlCommandsConfiguration.Configure(builder);
```

---

## Related Components

- `DC bot.Persistence/Configurations/README.md` — Parent feature overview and neighboring components.
- `DC bot.Persistence/README.md` — Project setup and architecture.
- `DC bot.Persistence/Configurations/Shared/` — Centralizes the Discord guild identifier conversion used by entity mappings.
- `DC bot.Persistence/Entities/BotControl/` — Represents the database rows used for durable bot commands, their claim/completion state, and cross-process change signals.
- `DC bot.Contracts/Interface/Service/Persistence/` — Shared interfaces and immutable values implemented by persistence.
