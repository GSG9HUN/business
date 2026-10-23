# MobileApps

Implements shared storage operations for mobile identities, guild access, refresh sessions, and realtime subscription membership.

## Overview

- Callers resolve the contracts through AddPersistenceServices; implementations create contexts through IDbContextFactory and map database state into shared records.
- Forward caller cancellation through database I/O. Preserve transactional ownership and use the shared retry/rollback helpers where a multi-step mutation requires them.
- Refresh-token rotation is conditional on the previous hash. Realtime membership reads also recheck guild authorization before publishers target stored subscribers.

## Classes

### MobileAppSessionRepository

**File:** `DC bot.Persistence/Repositories/MobileApps/MobileAppSessionRepository.cs`

**Purpose:** Reads and mutates mobile app session state using short-lived contexts from the injected EF factory.

**Implements / inherits:** `IMobileAppSessionRepository`.

**Methods:**

- `CreateAsync` — Persists a mobile refresh session with its user, hashed token, and expiry inside a transaction.

```csharp
public async Task CreateAsync(
    Guid sessionId,
    ulong discordUserId,
    string refreshTokenHash,
    DateTimeOffset expiresAtUtc,
    CancellationToken ct = default);
```

- `GetByRefreshTokenHashAsync` — Finds the persisted mobile session matching the hashed refresh token.

```csharp
public async Task<MobileAppSessionRecord?> GetByRefreshTokenHashAsync(string refreshTokenHash, CancellationToken ct = default);
```

- `RevokeAsync` — Marks the identified mobile session revoked inside a transaction.

```csharp
public async Task RevokeAsync(Guid sessionId, CancellationToken ct = default);
```

- `RotateRefreshTokenAsync` — Conditionally replaces a matching, unrevoked, unexpired refresh token so concurrent rotations cannot both succeed.

```csharp
public async Task<bool> RotateRefreshTokenAsync(
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

### MobileAppUserRepository

**File:** `DC bot.Persistence/Repositories/MobileApps/MobileAppUserRepository.cs`

**Purpose:** Reads and mutates mobile app user state using short-lived contexts from the injected EF factory.

**Implements / inherits:** `IMobileAppUserRepository`.

**Methods:**

- `GetGuildForUserAsync` — Reads one visible guild for the mobile user, returning null when membership is absent.

```csharp
public async Task<MobileAppUserGuildRecord?> GetGuildForUserAsync(
    ulong discordUserId,
    ulong guildId,
    CancellationToken cancellationToken = default);
```

- `GetGuildsForUserAsync` — Lists the guilds stored as visible to the requested mobile user.

```csharp
public async Task<IReadOnlyList<MobileAppUserGuildRecord>> GetGuildsForUserAsync(
    ulong discordUserId,
    CancellationToken cancellationToken = default);
```

- `GetUserAsync` — Reads the mobile user's stored profile, returning null when no profile exists.

```csharp
public async Task<MobileAppUserRecord?> GetUserAsync(ulong discordUserId, CancellationToken cancellationToken = default);
```

- `HasGuildAccessAsync` — Checks persisted user-to-guild membership before authorizing guild-scoped operations.

```csharp
public async Task<bool> HasGuildAccessAsync(ulong discordUserId, ulong guildId, CancellationToken cancellationToken = default);
```

- `SyncUserGuildsAsync` — Synchronizes the user's visible guild metadata using the shared PostgreSQL unique-conflict retry policy.

```csharp
public async Task SyncUserGuildsAsync(
    ulong discordUserId,
    IReadOnlyCollection<MobileAppUserGuildUpsertRecord> guilds,
    CancellationToken cancellationToken = default);
```

- `UpsertUserAsync` — Creates or updates the mobile user's Discord display metadata.

```csharp
public async Task UpsertUserAsync(MobileAppUserUpsertRecord user, CancellationToken cancellationToken = default);
```

**Usage:**

The caller supplies the component and argument values used in this example.

```csharp
var result = await mobileAppUserRepository.GetGuildForUserAsync(discordUserId, guildId, cancellationToken);
```

---

### MobileRealtimeMembershipRepository

**File:** `DC bot.Persistence/Repositories/MobileApps/MobileRealtimeMembershipRepository.cs`

**Purpose:** Reads and mutates mobile realtime membership state using short-lived contexts from the injected EF factory.

**Implements / inherits:** `IMobileRealtimeMembershipRepository`.

**Methods:**

- `GetAuthorizedSubscribedUserIdsAsync` — Returns subscribed user identifiers that still have persisted access to the requested guild.

```csharp
public async Task<IReadOnlyList<ulong>> GetAuthorizedSubscribedUserIdsAsync(
    ulong guildId,
    CancellationToken cancellationToken = default);
```

- `RemoveConnectionMembershipsAsync` — Removes every persisted guild subscription for the disconnected realtime connection.

```csharp
public async Task RemoveConnectionMembershipsAsync(string connectionId, CancellationToken cancellationToken = default);
```

- `RemoveGuildMembershipAsync` — Removes the realtime connection's subscription to the selected guild.

```csharp
public async Task RemoveGuildMembershipAsync(
    string connectionId,
    ulong discordUserId,
    ulong guildId,
    CancellationToken cancellationToken = default);
```

- `UpsertGuildMembershipAsync` — Records a realtime connection's guild subscription and user identity.

```csharp
public async Task UpsertGuildMembershipAsync(
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

- `DC bot.Persistence/Repositories/README.md` — Parent feature overview and neighboring components.
- `DC bot.Persistence/README.md` — Project setup and architecture.
- `DC bot.Persistence/Db/` — Owns the EF Core context, design-time creation, and startup migration runner.
- `DC bot.Persistence/Entities/MobileApps/` — Represents the database rows used for mobile identities, guild access, refresh sessions, and realtime subscription membership.
- `DC bot.Persistence/Repositories/Shared/` — Provides PostgreSQL transaction retries and owned rollback handling shared by repositories.
- `DC bot.Contracts/Interface/Service/Persistence/MobileApps/` — Defines repository operations for mobile identities, guild access, refresh sessions, and realtime subscription membership.
- `DC bot.Contracts/Interface/Service/Persistence/` — Shared interfaces and immutable values implemented by persistence.
