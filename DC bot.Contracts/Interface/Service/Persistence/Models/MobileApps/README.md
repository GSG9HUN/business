# MobileApps

Defines shared records and state values for mobile identities, guild access, refresh sessions, and realtime subscription membership.

## Overview

- Persistence maps entities into these values, and application services or API mappers consume them without database-provider dependencies.
- Keep wire response DTOs in API and tracked entities in DC bot.Persistence. Changing a record affects both repository implementations and their consumers.

## Models and Results

### MobileAppSessionRecord

**File:** `DC bot.Contracts/Interface/Service/Persistence/Models/MobileApps/MobileAppSessionRecord.cs`

**Purpose:** Carries immutable mobile app session values across the shared storage or mobile-command boundary.

**Record declaration:**

```csharp
public sealed record MobileAppSessionRecord(
    Guid SessionId,
    ulong DiscordUserId,
    string RefreshTokenHash,
    DateTimeOffset RefreshTokenExpiresAtUtc,
    DateTimeOffset? RevokedAtUtc);
```

---

### MobileAppUserGuildRecord

**File:** `DC bot.Contracts/Interface/Service/Persistence/Models/MobileApps/MobileAppUserGuildRecord.cs`

**Purpose:** Carries immutable mobile app user guild values across the shared storage or mobile-command boundary.

**Record declaration:**

```csharp
public sealed record MobileAppUserGuildRecord(
    ulong GuildId,
    string Name,
    string? IconHash,
    ulong Permissions,
    bool IsOwner,
    DateTimeOffset LastSeenAtUtc);
```

---

### MobileAppUserGuildUpsertRecord

**File:** `DC bot.Contracts/Interface/Service/Persistence/Models/MobileApps/MobileAppUserGuildUpsertRecord.cs`

**Purpose:** Carries immutable mobile app user guild upsert values across the shared storage or mobile-command boundary.

**Record declaration:**

```csharp
public sealed record MobileAppUserGuildUpsertRecord(ulong GuildId, string Name, string? IconHash, ulong Permissions, bool IsOwner);
```

---

### MobileAppUserRecord

**File:** `DC bot.Contracts/Interface/Service/Persistence/Models/MobileApps/MobileAppUserRecord.cs`

**Purpose:** Carries immutable mobile app user values across the shared storage or mobile-command boundary.

**Record declaration:**

```csharp
public sealed record MobileAppUserRecord(
    ulong DiscordUserId,
    string Username,
    string? GlobalName,
    string? AvatarHash,
    DateTimeOffset CreatedAtUtc,
    DateTimeOffset LastLoginAtUtc);
```

---

### MobileAppUserUpsertRecord

**File:** `DC bot.Contracts/Interface/Service/Persistence/Models/MobileApps/MobileAppUserUpsertRecord.cs`

**Purpose:** Carries immutable mobile app user upsert values across the shared storage or mobile-command boundary.

**Record declaration:**

```csharp
public sealed record MobileAppUserUpsertRecord(ulong DiscordUserId, string Username, string? GlobalName, string? AvatarHash);
```

---

## Related Components

- `DC bot.Contracts/Interface/Service/Persistence/Models/README.md` — Parent feature overview and neighboring components.
- `DC bot.Contracts/README.md` — Project setup and architecture.
- `DC bot.Persistence/` — Repository implementations and database mappings that consume these shared contracts.
