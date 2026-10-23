# Guilds

Defines EF Core table mappings for guild defaults, premium state, and the mapped premium-audit schema.

## Overview

- These mappings connect the corresponding Entities feature to PostgreSQL columns, keys, indexes, and relationships used by repositories.
- Change entity shape and mapping together. Generate a new migration for schema changes rather than rewriting an already-applied migration.

## Classes

### GuildDataConfiguration

**File:** `DC bot.Persistence/Configurations/Guilds/GuildDataConfiguration.cs`

**Purpose:** Configures PostgreSQL storage, keys, and relationships for guild data.

**Implements / inherits:** `IEntityTypeConfiguration<GuildDataEntity>`.

**Methods:**

- `Configure` — Implements `Configure` using the contract shown below.

```csharp
public void Configure(EntityTypeBuilder<GuildDataEntity> builder);
```

**Mapping rules:** The twenty-four-seven flag maps to `is_twenty_four_seven_enabled` with a default of `false`. Preferred voice and text channel identifiers use nullable `numeric(20,0)` columns.

```csharp
builder.Property(entity => entity.PreferredVoiceChannelId)
    .HasColumnName("preferred_voice_channel_id")
    .HasColumnType("numeric(20,0)");
```

**Usage:**

The caller supplies the component and argument values used in this example.

```csharp
guildDataConfiguration.Configure(builder);
```

---

### GuildPremiumAuditConfiguration

**File:** `DC bot.Persistence/Configurations/Guilds/GuildPremiumAuditConfiguration.cs`

**Purpose:** Configures PostgreSQL storage, keys, and relationships for guild premium audit.

**Implements / inherits:** `IEntityTypeConfiguration<GuildPremiumAuditEntity>`.

**Methods:**

- `Configure` — Implements `Configure` using the contract shown below.

```csharp
public void Configure(EntityTypeBuilder<GuildPremiumAuditEntity> builder);
```

**Usage:**

The caller supplies the component and argument values used in this example.

```csharp
guildPremiumAuditConfiguration.Configure(builder);
```

---

## Related Components

- `DC bot.Persistence/Configurations/README.md` — Parent feature overview and neighboring components.
- `DC bot.Persistence/README.md` — Project setup and architecture.
- `DC bot.Persistence/Configurations/Shared/` — Centralizes the Discord guild identifier conversion used by entity mappings.
- `DC bot.Persistence/Entities/Guilds/` — Represents the database rows used for guild defaults, premium state, and the mapped premium-audit schema.
- `DC bot.Contracts/Interface/Service/Persistence/` — Shared interfaces and immutable values implemented by persistence.
