# MobileAppUserSettings

Defines EF Core table mappings for per-user mobile language, theme, feedback, sound, and telemetry preferences.

## Overview

- These mappings connect the corresponding Entities feature to PostgreSQL columns, keys, indexes, and relationships used by repositories.
- Change entity shape and mapping together. Generate a new migration for schema changes rather than rewriting an already-applied migration.

## Classes

### MobileAppUserSettingsConfiguration

**File:** `DC bot.Persistence/Configurations/MobileAppUserSettings/MobileAppUserSettingsConfiguration.cs`

**Purpose:** Configures PostgreSQL storage, keys, and relationships for mobile app user settings.

**Implements / inherits:** `IEntityTypeConfiguration<MobileAppUserSettingsEntity>`.

**Methods:**

- `Configure` — Implements `Configure` using the contract shown below.

```csharp
public void Configure(Microsoft.EntityFrameworkCore.Metadata.Builders.EntityTypeBuilder<MobileAppUserSettingsEntity> builder);
```

**Usage:**

The caller supplies the component and argument values used in this example.

```csharp
mobileAppUserSettingsConfiguration.Configure(builder);
```

---

## Related Components

- `DC bot.Persistence/Configurations/README.md` — Parent feature overview and neighboring components.
- `DC bot.Persistence/README.md` — Project setup and architecture.
- `DC bot.Persistence/Configurations/Shared/` — Centralizes the Discord guild identifier conversion used by entity mappings.
- `DC bot.Persistence/Entities/MobileAppUserSettings/` — Represents the database rows used for per-user mobile language, theme, feedback, sound, and telemetry preferences.
- `DC bot.Contracts/Interface/Service/Persistence/Models/MobileAppUserSettings/` — Defines shared records and state values for per-user mobile language, theme, feedback, sound, and telemetry preferences.
- `DC bot.Contracts/Interface/Service/Persistence/` — Shared interfaces and immutable values implemented by persistence.
