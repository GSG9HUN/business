# Playback

Implements shared storage operations for current playback state, repeat flags, start ownership, and canonical batch metadata.

## Overview

- Callers resolve the contracts through AddPersistenceServices; implementations create contexts through IDbContextFactory and map database state into shared records.
- Forward caller cancellation through database I/O. Preserve transactional ownership and use the shared retry/rollback helpers where a multi-step mutation requires them.
- Playback start confirmation checks operation ownership before committing the selected queue item. Notification helpers publish invalidation signals; consumers reload snapshots instead of interpreting the signal as complete playback state.

## Classes

### PlaybackRealtimeNotifier

**File:** `DC bot.Persistence/Repositories/Playback/PlaybackRealtimeNotifier.cs`

**Purpose:** Publishes playback realtime notifications within the caller's database transaction.

**Methods:**

- `NotifyPlaybackUpdatedAsync` — Publishes a playback event through the current database transaction; non-relational test providers are skipped.

```csharp
public static Task NotifyPlaybackUpdatedAsync(
    BotDbContext dbContext,
    ulong guildId,
    string eventName,
    CancellationToken cancellationToken);
```

**Usage:**

The caller supplies the component and argument values used in this example.

```csharp
await PlaybackRealtimeNotifier.NotifyPlaybackUpdatedAsync(dbContext, guildId, eventName, cancellationToken);
```

---

### PlaybackStateRepository

**File:** `DC bot.Persistence/Repositories/Playback/PlaybackStateRepository.cs`

**Purpose:** Reads and mutates playback state state using short-lived contexts from the injected EF factory.

**Implements / inherits:** `IPlaybackStateRepository`.

**Methods:**

- `GetOrCreateAsync` — Reads the guild playback state or mobile-user settings, creating documented defaults when missing.

```csharp
public async Task<PlaybackStateRecord> GetOrCreateAsync(ulong guildId, CancellationToken cancellationToken = default);
```

- `SetCurrentTrackAsync` — Persists or clears current-track identity and its queue-item relationship, publishing the selected track event.

```csharp
public async Task SetCurrentTrackAsync(
    ulong guildId,
    string? trackIdentifier,
    long? queueItemId,
    string? realtimeEventName = null,
    CancellationToken cancellationToken = default);
```

- `SetPlaybackPositionAsync` — Persists playback position and pause state and publishes the selected playback event.

```csharp
public async Task SetPlaybackPositionAsync(
    ulong guildId,
    TimeSpan position,
    bool isPaused,
    string? realtimeEventName = null,
    CancellationToken cancellationToken = default);
```

- `SetRepeatStateAsync` — Persists repeat flags while retaining unrelated playback state and publishes the selected realtime event.

```csharp
public async Task SetRepeatStateAsync(
    ulong guildId,
    bool isRepeating,
    bool isRepeatingList,
    string? realtimeEventName = null,
    CancellationToken cancellationToken = default);
```

**Usage:**

The caller supplies the component and argument values used in this example.

```csharp
var result = await playbackStateRepository.GetOrCreateAsync(guildId, cancellationToken);
```

---

### RepeatListRepository

**File:** `DC bot.Persistence/Repositories/Playback/RepeatListRepository.cs`

**Purpose:** Reads and mutates repeat list state using short-lived contexts from the injected EF factory.

**Implements / inherits:** `IRepeatListRepository`.

**Methods:**

- `GetTrackIdentifiersAsync` — Reads the legacy repeat-list snapshot in its persisted playback order.

```csharp
public async Task<IReadOnlyList<string>> GetTrackIdentifiersAsync(ulong guildId, CancellationToken cancellationToken = default);
```

- `ClearAsync` — Removes the guild's persisted legacy repeat-list snapshot.

```csharp
public async Task ClearAsync(ulong guildId, CancellationToken cancellationToken = default);
```

- `ReplaceAsync` — Replaces the guild's legacy repeat-list snapshot with the supplied ordered track identifiers.

```csharp
public async Task ReplaceAsync(
    ulong guildId,
    IReadOnlyList<string> trackIdentifiers,
    CancellationToken cancellationToken = default);
```

**Usage:**

The caller supplies the component and argument values used in this example.

```csharp
var result = await repeatListRepository.GetTrackIdentifiersAsync(guildId, cancellationToken);
```

---

## Related Components

- `DC bot.Persistence/Repositories/README.md` — Parent feature overview and neighboring components.
- `DC bot.Persistence/README.md` — Project setup and architecture.
- `DC bot.Persistence/Db/` — Owns the EF Core context, design-time creation, and startup migration runner.
- `DC bot.Persistence/Entities/Playback/` — Represents the database rows used for current playback state, repeat flags, start ownership, and canonical batch metadata.
- `DC bot.Persistence/Repositories/Guilds/` — Implements shared storage operations for guild defaults, premium state, and the mapped premium-audit schema.
- `DC bot.Persistence/Repositories/Shared/` — Provides PostgreSQL transaction retries and owned rollback handling shared by repositories.
- `DC bot.Contracts/Interface/Service/Persistence/` — Shared interfaces and immutable values implemented by persistence.
