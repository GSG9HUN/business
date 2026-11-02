# MobileApps

Defines repository operations for mobile identities, guild access, refresh sessions, and realtime subscription membership.

## Overview

- The bot and API consume these interfaces; concrete implementations and DI registration live in DC bot.Persistence unless noted below.
- Describe lookup absence, ordering, conditional mutation, and cancellation in the contract. Keep provider-specific SQL and EF types out of this boundary.

## Interfaces

### IMobileAppSessionRepository

**File:** `DC bot.Contracts/Interface/Service/Persistence/MobileApps/IMobileAppSessionRepository.cs`

**Purpose:** Defines shared storage operations for mobile app session repository, including caller cancellation and immutable result contracts.

**Methods:**

- `CreateAsync` — Persists a mobile refresh session with its user, hashed token, and expiry inside a transaction.

```csharp
Task CreateAsync(
    Guid sessionId,
    ulong discordUserId,
    string refreshTokenHash,
    DateTimeOffset expiresAtUtc,
    CancellationToken ct = default);
```

- `GetByRefreshTokenHashAsync` — Finds the persisted mobile session matching the hashed refresh token.

```csharp
Task<MobileAppSessionRecord?> GetByRefreshTokenHashAsync(string refreshTokenHash, CancellationToken ct = default);
```

- `RevokeAsync` — Marks the identified mobile session revoked inside a transaction.

```csharp
Task RevokeAsync(Guid sessionId, CancellationToken ct = default);
```

- `RotateRefreshTokenAsync` — Conditionally replaces a matching, unrevoked, unexpired refresh token so concurrent rotations cannot both succeed.

```csharp
Task<bool> RotateRefreshTokenAsync(
    Guid sessionId,
    string currentRefreshTokenHash,
    string newRefreshTokenHash,
    DateTimeOffset expiresAtUtc,
    CancellationToken ct = default);
```

**Usage:**

The caller supplies the component and argument values used in this example.

```csharp
var result = await mobileAppSessionRepository.GetByRefreshTokenHashAsync(refreshTokenHash, ct);
```

---

### IMobileAppUserRepository

**File:** `DC bot.Contracts/Interface/Service/Persistence/MobileApps/IMobileAppUserRepository.cs`

**Purpose:** Defines shared storage operations for mobile app user repository, including caller cancellation and immutable result contracts.

**Methods:**

- `GetGuildForUserAsync` — Reads one visible guild for the mobile user, returning null when membership is absent.

```csharp
Task<MobileAppUserGuildRecord?> GetGuildForUserAsync(ulong discordUserId, ulong guildId, CancellationToken ct = default);
```

- `GetGuildsForUserAsync` — Lists the guilds stored as visible to the requested mobile user.

```csharp
Task<IReadOnlyList<MobileAppUserGuildRecord>> GetGuildsForUserAsync(ulong discordUserId, CancellationToken ct = default);
```

- `GetUserAsync` — Reads the mobile user's stored profile, returning null when no profile exists.

```csharp
Task<MobileAppUserRecord?> GetUserAsync(ulong discordUserId, CancellationToken ct = default);
```

- `HasGuildAccessAsync` — Checks persisted user-to-guild membership before authorizing guild-scoped operations.

```csharp
Task<bool> HasGuildAccessAsync(ulong discordUserId, ulong guildId, CancellationToken ct = default);
```

- `SyncUserGuildsAsync` — Synchronizes the user's visible guild metadata using the shared PostgreSQL unique-conflict retry policy.

```csharp
Task SyncUserGuildsAsync(
    ulong discordUserId,
    IReadOnlyCollection<MobileAppUserGuildUpsertRecord> guilds,
    CancellationToken ct = default);
```

- `UpsertUserAsync` — Creates or updates the mobile user's Discord display metadata.

```csharp
Task UpsertUserAsync(MobileAppUserUpsertRecord user, CancellationToken ct = default);
```

**Usage:**

The caller supplies the component and argument values used in this example.

```csharp
var result = await mobileAppUserRepository.GetGuildForUserAsync(discordUserId, guildId, ct);
```

---

### IMobileRealtimeMembershipRepository

**File:** `DC bot.Contracts/Interface/Service/Persistence/MobileApps/IMobileRealtimeMembershipRepository.cs`

**Purpose:** Defines shared storage operations for mobile realtime membership repository, including caller cancellation and immutable result contracts.

**Methods:**

- `GetAuthorizedSubscribedUserIdsAsync` — Returns subscribed user identifiers that still have persisted access to the requested guild.

```csharp
Task<IReadOnlyList<ulong>> GetAuthorizedSubscribedUserIdsAsync(ulong guildId, CancellationToken cancellationToken = default);
```

- `RemoveConnectionMembershipsAsync` — Removes every persisted guild subscription for the disconnected realtime connection.

```csharp
Task RemoveConnectionMembershipsAsync(string connectionId, CancellationToken cancellationToken = default);
```

- `RemoveGuildMembershipAsync` — Removes the realtime connection's subscription to the selected guild.

```csharp
Task RemoveGuildMembershipAsync(
    string connectionId,
    ulong discordUserId,
    ulong guildId,
    CancellationToken cancellationToken = default);
```

- `UpsertGuildMembershipAsync` — Records a realtime connection's guild subscription and user identity.

```csharp
Task UpsertGuildMembershipAsync(
    string connectionId,
    ulong discordUserId,
    ulong guildId,
    CancellationToken cancellationToken = default);
```

**Usage:**

The caller supplies the component and argument values used in this example.

```csharp
var result = await mobileRealtimeMembershipRepository.GetAuthorizedSubscribedUserIdsAsync(guildId, cancellationToken);
```

---

## Related Components

- `DC bot.Contracts/Interface/Service/Persistence/README.md` — Parent feature overview and neighboring components.
- `DC bot.Contracts/README.md` — Project setup and architecture.
- `DC bot.Contracts/Interface/Service/Persistence/Models/MobileApps/` — Defines shared records and state values for mobile identities, guild access, refresh sessions, and realtime subscription membership.
- `DC bot.Persistence/` — Repository implementations and database mappings that consume these shared contracts.
