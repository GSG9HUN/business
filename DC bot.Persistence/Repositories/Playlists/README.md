# Playlists

Implements shared storage operations for saved playlist metadata and ordered, serialized playlist tracks.

## Overview

- Callers resolve the contracts through AddPersistenceServices; implementations create contexts through IDbContextFactory and map database state into shared records.
- Forward caller cancellation through database I/O. Preserve transactional ownership and use the shared retry/rollback helpers where a multi-step mutation requires them.

## Classes

### PlaylistRepository

**File:** `DC bot.Persistence/Repositories/Playlists/PlaylistRepository.cs`

**Purpose:** Reads and mutates playlist state using short-lived contexts from the injected EF factory.

**Implements / inherits:** `IPlaylistRepository`.

**Methods:**

- `CreatePlaylistAsync` — Creates a guild-owned empty playlist and returns its durable identifier, reporting duplicate-name conflicts.

```csharp
public async Task<long> CreatePlaylistAsync(ulong guildId, string playlistName, CancellationToken cancellationToken = default);
```

- `GetByGuildAndNameAsync` — Reads the guild-owned playlist with the requested stored name.

```csharp
public async Task<PlaylistRecord?> GetByGuildAndNameAsync(
    ulong guildId,
    string playlistName,
    CancellationToken cancellationToken = default);
```

- `GetByGuildAsync` — Lists the guild's saved playlists with their persisted track counts.

```csharp
public async Task<IReadOnlyList<PlaylistSummaryRecord>> GetByGuildAsync(
    ulong guildId,
    CancellationToken cancellationToken = default);
```

- `GetByIdAsync` — Reads a queue item or playlist by its durable database identifier.

```csharp
public async Task<PlaylistRecord?> GetByIdAsync(long playlistId, CancellationToken cancellationToken = default);
```

- `DeleteByGuildAndNameAsync` — Deletes only the playlist matching both guild identity and stored name.

```csharp
public async Task<bool> DeleteByGuildAndNameAsync(
    ulong guildId,
    string playlistName,
    CancellationToken cancellationToken = default);
```

- `DeleteByIdAsync` — Deletes the playlist with the specified durable identifier and reports whether a row was removed.

```csharp
public async Task<bool> DeleteByIdAsync(long playlistId, CancellationToken cancellationToken = default);
```

**Additional methods:** `ExistsAsync`, `RenameAsync`, `RenameByIdAsync`. See the source and its XML contracts for the complete parameter lists.

**Usage:**

The caller supplies the component and argument values used in this example.

```csharp
var result = await playlistRepository.GetByGuildAndNameAsync(guildId, playlistName, cancellationToken);
```

---

### PlaylistTrackRepository

**File:** `DC bot.Persistence/Repositories/Playlists/PlaylistTrackRepository.cs`

**Purpose:** Reads and mutates playlist track state using short-lived contexts from the injected EF factory.

**Implements / inherits:** `IPlaylistTrackRepository`.

**Methods:**

- `GetByPlaylistIdOrderedAsync` — Reads one playlist's tracks in stored order without exposing tracked EF entities.

```csharp
public async Task<IReadOnlyList<PlaylistTrackRecord>> GetByPlaylistIdOrderedAsync(
    long playlistId,
    CancellationToken cancellationToken = default);
```

- `GetByPlaylistIdsOrderedAsync` — Reads tracks for the supplied playlists while preserving each playlist's stored order.

```csharp
public async Task<IReadOnlyList<PlaylistTrackRecord>> GetByPlaylistIdsOrderedAsync(
    IReadOnlyCollection<long> playlistIds,
    CancellationToken cancellationToken = default);
```

- `AddRangeAsync` — Appends an ordered track collection under serializable retry so concurrent appends receive distinct positions.

```csharp
public async Task AddRangeAsync(
    long playlistId,
    IReadOnlyCollection<PlaylistTrackCreateRecord> tracks,
    CancellationToken cancellationToken = default);
```

- `AddTrackAsync` — Appends one track with its source identity under serializable retry.

```csharp
public async Task AddTrackAsync(long playlistId, PlaylistTrackCreateRecord track, CancellationToken cancellationToken = default);
```

- `RemoveTrackAsync` — Removes the numbered playlist track and compacts remaining positions under serializable retry.

```csharp
public async Task RemoveTrackAsync(long playlistId, int orderNumber, CancellationToken cancellationToken = default);
```

**Usage:**

The caller supplies the component and argument values used in this example.

```csharp
var result = await playlistTrackRepository.GetByPlaylistIdOrderedAsync(playlistId, cancellationToken);
```

---

## Related Components

- `DC bot.Persistence/Repositories/README.md` — Parent feature overview and neighboring components.
- `DC bot.Persistence/README.md` — Project setup and architecture.
- `DC bot.Persistence/Db/` — Owns the EF Core context, design-time creation, and startup migration runner.
- `DC bot.Persistence/Entities/Playlists/` — Represents the database rows used for saved playlist metadata and ordered, serialized playlist tracks.
- `DC bot.Persistence/Repositories/Guilds/` — Implements shared storage operations for guild defaults, premium state, and the mapped premium-audit schema.
- `DC bot.Persistence/Repositories/Shared/` — Provides PostgreSQL transaction retries and owned rollback handling shared by repositories.
- `DC bot.Contracts/Interface/Service/Persistence/` — Shared interfaces and immutable values implemented by persistence.
