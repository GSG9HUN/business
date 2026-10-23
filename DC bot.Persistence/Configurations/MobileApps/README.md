# MobileApps

Defines EF Core table mappings for mobile identities, guild access, refresh sessions, and realtime subscription membership.

## Overview

- These mappings connect the corresponding Entities feature to PostgreSQL columns, keys, indexes, and relationships used by repositories.
- Change entity shape and mapping together. Generate a new migration for schema changes rather than rewriting an already-applied migration.

## Classes

### MobileAppSessionsConfiguration

**File:** `DC bot.Persistence/Configurations/MobileApps/MobileAppSessionsConfiguration.cs`

**Purpose:** Configures PostgreSQL storage, keys, and relationships for mobile app sessions.

**Implements / inherits:** `IEntityTypeConfiguration<MobileAppSessionEntity>`.

**Methods:**

- `Configure` — Implements `Configure` using the contract shown below.

```csharp
public void Configure(Microsoft.EntityFrameworkCore.Metadata.Builders.EntityTypeBuilder<MobileAppSessionEntity> builder);
```

**Usage:**

The caller supplies the component and argument values used in this example.

```csharp
mobileAppSessionsConfiguration.Configure(builder);
```

---

### MobileAppUsersConfiguration

**File:** `DC bot.Persistence/Configurations/MobileApps/MobileAppUsersConfiguration.cs`

**Purpose:** Configures PostgreSQL storage, keys, and relationships for mobile app users.

**Implements / inherits:** `IEntityTypeConfiguration<MobileAppUserEntity>`.

**Methods:**

- `Configure` — Implements `Configure` using the contract shown below.

```csharp
public void Configure(Microsoft.EntityFrameworkCore.Metadata.Builders.EntityTypeBuilder<MobileAppUserEntity> builder);
```

**Usage:**

The caller supplies the component and argument values used in this example.

```csharp
mobileAppUsersConfiguration.Configure(builder);
```

---

### MobileRealtimeGuildMembershipsConfiguration

**File:** `DC bot.Persistence/Configurations/MobileApps/MobileRealtimeGuildMembershipsConfiguration.cs`

**Purpose:** Configures PostgreSQL storage, keys, and relationships for mobile realtime guild memberships.

**Implements / inherits:** `IEntityTypeConfiguration<MobileRealtimeGuildMembershipEntity>`.

**Methods:**

- `Configure` — Implements `Configure` using the contract shown below.

```csharp
public void Configure(EntityTypeBuilder<MobileRealtimeGuildMembershipEntity> builder);
```

**Usage:**

The caller supplies the component and argument values used in this example.

```csharp
mobileRealtimeGuildMembershipsConfiguration.Configure(builder);
```

---

### UserGuildsConfiguration

**File:** `DC bot.Persistence/Configurations/MobileApps/UserGuildsConfiguration.cs`

**Purpose:** Configures PostgreSQL storage, keys, and relationships for user guilds.

**Implements / inherits:** `IEntityTypeConfiguration<UserGuildEntity>`.

**Methods:**

- `Configure` — Implements `Configure` using the contract shown below.

```csharp
public void Configure(Microsoft.EntityFrameworkCore.Metadata.Builders.EntityTypeBuilder<UserGuildEntity> builder);
```

**Usage:**

The caller supplies the component and argument values used in this example.

```csharp
userGuildsConfiguration.Configure(builder);
```

---

## Related Components

- `DC bot.Persistence/Configurations/README.md` — Parent feature overview and neighboring components.
- `DC bot.Persistence/README.md` — Project setup and architecture.
- `DC bot.Persistence/Configurations/Shared/` — Centralizes the Discord guild identifier conversion used by entity mappings.
- `DC bot.Persistence/Entities/MobileApps/` — Represents the database rows used for mobile identities, guild access, refresh sessions, and realtime subscription membership.
- `DC bot.Contracts/Interface/Service/Persistence/` — Shared interfaces and immutable values implemented by persistence.
