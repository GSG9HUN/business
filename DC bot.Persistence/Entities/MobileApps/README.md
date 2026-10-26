# MobileApps

Represents the database rows used for mobile identities, guild access, refresh sessions, and realtime subscription membership.

## Overview

- Feature repositories read and mutate these values in short-lived EF contexts, then map results into DC bot.Contracts records.
- Entity properties describe persisted state. Keep HTTP response formatting, Discord objects, and playback transport behavior outside these types.

## Classes

### MobileAppSessionEntity

**File:** `DC bot.Persistence/Entities/MobileApps/MobileAppSessionEntity.cs`

**Purpose:** Represents the persisted mobile app session row mapped by the EF Core model.

**Properties:**

```csharp
// The identifier of the mobile application session.
public Guid SessionId { get; set; }
public ulong DiscordUserId { get; set; }
public string RefreshTokenHash { get; set; }
public DateTimeOffset CreatedAtUtc { get; set; }
public DateTimeOffset LastRefreshedAtUtc { get; set; }
public DateTimeOffset RefreshTokenExpiresAtUtc { get; set; }
public DateTimeOffset? RevokedAtUtc { get; set; }
public MobileAppUserEntity User { get; set; }
```

---

### MobileAppUserEntity

**File:** `DC bot.Persistence/Entities/MobileApps/MobileAppUserEntity.cs`

**Purpose:** Represents the persisted mobile app user row mapped by the EF Core model.

**Properties:**

```csharp
// The Discord user identifier.
public ulong DiscordUserId { get; set; }
public string Username { get; set; }
public string? GlobalName { get; set; }
public string? AvatarHash { get; set; }
public DateTimeOffset CreatedAtUtc { get; set; }
public DateTimeOffset LastLoginAtUtc { get; set; }
public ICollection<UserGuildEntity> Guilds { get; set; }
public MobileAppUserSettingsEntity? Settings { get; set; }
```

---

### MobileRealtimeGuildMembershipEntity

**File:** `DC bot.Persistence/Entities/MobileApps/MobileRealtimeGuildMembershipEntity.cs`

**Purpose:** Represents the persisted mobile realtime guild membership row mapped by the EF Core model.

**Properties:**

```csharp
// The SignalR connection identifier associated with this membership.
public string ConnectionId { get; set; }
public ulong DiscordUserId { get; set; }
public ulong GuildId { get; set; }
public DateTimeOffset JoinedAtUtc { get; set; }
```

---

### UserGuildEntity

**File:** `DC bot.Persistence/Entities/MobileApps/UserGuildEntity.cs`

**Purpose:** Represents the persisted user guild row mapped by the EF Core model.

**Properties:**

```csharp
// The Discord user identifier.
public ulong DiscordUserId { get; set; }
public ulong GuildId { get; set; }
public ulong Permissions { get; set; }
public bool IsOwner { get; set; }
public string Name { get; set; }
public string? IconHash { get; set; }
public DateTimeOffset LastSeenAtUtc { get; set; }
public MobileAppUserEntity User { get; set; }
public GuildDataEntity Guild { get; set; }
```

---

## Related Components

- `DC bot.Persistence/Entities/README.md` — Parent feature overview and neighboring components.
- `DC bot.Persistence/README.md` — Project setup and architecture.
- `DC bot.Persistence/Entities/Guilds/` — Represents the database rows used for guild defaults, premium state, and the mapped premium-audit schema.
- `DC bot.Persistence/Entities/MobileAppUserSettings/` — Represents the database rows used for per-user mobile language, theme, feedback, sound, and telemetry preferences.
- `DC bot.Contracts/Interface/Service/Persistence/` — Shared interfaces and immutable values implemented by persistence.
