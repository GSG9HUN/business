# Playlists

Defines repository operations for saved playlist metadata and ordered, serialized playlist tracks.

## Overview

- The bot and API consume these interfaces; concrete implementations and DI registration live in DC bot.Persistence unless noted below.
- Describe lookup absence, ordering, conditional mutation, and cancellation in the contract. Keep provider-specific SQL and EF types out of this boundary.

## Interfaces

### IPlaylistRepository

**File:** `DC bot.Contracts/Interface/Service/Persistence/Playlists/IPlaylistRepository.cs`

**Purpose:** Defines shared storage operations for playlist repository, including caller cancellation and immutable result contracts.

**Methods:**

- `CreatePlaylistAsync` — Creates a guild-owned empty playlist and returns its durable identifier, reporting duplicate-name conflicts.

```csharp
Task<long> CreatePlaylistAsync(ulong guildId, string playlistName, CancellationToken cancellationToken = default);
```

- `GetByGuildAndNameAsync` — Reads the guild-owned playlist with the requested stored name.

```csharp
Task<PlaylistRecord?> GetByGuildAndNameAsync(ulong guildId, string playlistName, CancellationToken cancellationToken = default);
```

- `GetByGuildAsync` — Lists the guild's saved playlists with their persisted track counts.

```csharp
Task<IReadOnlyList<PlaylistSummaryRecord>> GetByGuildAsync(ulong guildId, CancellationToken cancellationToken = default);
```

- `GetByIdAsync` — Reads a queue item or playlist by its durable database identifier.

```csharp
Task<PlaylistRecord?> GetByIdAsync(long playlistId, CancellationToken cancellationToken = default);
```

- `DeleteByGuildAndNameAsync` — Deletes only the playlist matching both guild identity and stored name.

```csharp
Task<bool> DeleteByGuildAndNameAsync(ulong guildId, string playlistName, CancellationToken cancellationToken = default);
```

- `DeleteByIdAsync` — Deletes the playlist with the specified durable identifier and reports whether a row was removed.

```csharp
Task<bool> DeleteByIdAsync(long playlistId, CancellationToken cancellationToken = default);
```

**Additional methods:** `ExistsAsync`, `RenameAsync`, `RenameByIdAsync`. See the source and its XML contracts for the complete parameter lists.

**Usage:**

The caller supplies the component and argument values used in this example.

```csharp
var result = await playlistRepository.GetByGuildAndNameAsync(guildId, playlistName, cancellationToken);
```

---

### IPlaylistTrackRepository

**File:** `DC bot.Contracts/Interface/Service/Persistence/Playlists/IPlaylistTrackRepository.cs`

**Purpose:** Defines shared storage operations for playlist track repository, including caller cancellation and immutable result contracts.

**Methods:**

- `GetByPlaylistIdOrderedAsync` — Reads one playlist's tracks in stored order without exposing tracked EF entities.

```csharp
Task<IReadOnlyList<PlaylistTrackRecord>> GetByPlaylistIdOrderedAsync(
    long playlistId,
    CancellationToken cancellationToken = default);
```

- `GetByPlaylistIdsOrderedAsync` — Reads tracks for the supplied playlists while preserving each playlist's stored order.

```csharp
Task<IReadOnlyList<PlaylistTrackRecord>> GetByPlaylistIdsOrderedAsync(
    IReadOnlyCollection<long> playlistIds,
    CancellationToken cancellationToken = default);
```

- `AddRangeAsync` — Appends an ordered track collection under serializable retry so concurrent appends receive distinct positions.

```csharp
Task AddRangeAsync(
    long playlistId,
    IReadOnlyCollection<PlaylistTrackCreateRecord> tracks,
    CancellationToken cancellationToken = default);
```

- `AddTrackAsync` — Appends one track with its source identity under serializable retry.

```csharp
Task AddTrackAsync(long playlistId, PlaylistTrackCreateRecord track, CancellationToken cancellationToken = default);
```

- `RemoveTrackAsync` — Removes the numbered playlist track and compacts remaining positions under serializable retry.

```csharp
Task RemoveTrackAsync(long playlistId, int orderNumber, CancellationToken cancellationToken = default);
```

**Usage:**

The caller supplies the component and argument values used in this example.

```csharp
var result = await playlistTrackRepository.GetByPlaylistIdOrderedAsync(playlistId, cancellationToken);
```

---

## Related Components

- `DC bot.Contracts/Interface/Service/Persistence/README.md` — Parent feature overview and neighboring components.
- `DC bot.Contracts/README.md` — Project setup and architecture.
- `DC bot.Contracts/Interface/Service/Persistence/Models/Playlists/` — Defines shared records and state values for saved playlist metadata and ordered, serialized playlist tracks.
- `DC bot.Persistence/` — Repository implementations and database mappings that consume these shared contracts.
