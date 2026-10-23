# Playback

Defines repository operations for current playback state, repeat flags, start ownership, and canonical batch metadata.

## Overview

- The bot and API consume these interfaces; concrete implementations and DI registration live in DC bot.Persistence unless noted below.
- Describe lookup absence, ordering, conditional mutation, and cancellation in the contract. Keep provider-specific SQL and EF types out of this boundary.

## Interfaces

### IPlaybackStateRepository

**File:** `DC bot.Contracts/Interface/Service/Persistence/Playback/IPlaybackStateRepository.cs`

**Purpose:** Defines shared storage operations for playback state repository, including caller cancellation and immutable result contracts.

**Methods:**

- `GetOrCreateAsync` — Reads the guild playback state or mobile-user settings, creating documented defaults when missing.

```csharp
Task<PlaybackStateRecord> GetOrCreateAsync(ulong guildId, CancellationToken cancellationToken = default);
```

- `SetCurrentTrackAsync` — Persists or clears current-track identity and its queue-item relationship, publishing the selected track event.

```csharp
Task SetCurrentTrackAsync(
    ulong guildId,
    string? trackIdentifier,
    long? queueItemId,
    CancellationToken cancellationToken = default);
```

- `SetCurrentTrackAsync` — Persists or clears current-track identity and its queue-item relationship, publishing the selected track event.

```csharp
Task SetCurrentTrackAsync(
    ulong guildId,
    string? trackIdentifier,
    long? queueItemId,
    string? realtimeEventName = null,
    CancellationToken cancellationToken = default);
```

- `SetPlaybackPositionAsync` — Persists playback position and pause state and publishes the selected playback event.

```csharp
Task SetPlaybackPositionAsync(ulong guildId, TimeSpan position, bool isPaused, CancellationToken cancellationToken = default);
```

- `SetPlaybackPositionAsync` — Persists playback position and pause state and publishes the selected playback event.

```csharp
Task SetPlaybackPositionAsync(
    ulong guildId,
    TimeSpan position,
    bool isPaused,
    string? realtimeEventName = null,
    CancellationToken cancellationToken = default);
```

- `SetRepeatStateAsync` — Persists repeat flags while retaining unrelated playback state and publishes the selected realtime event.

```csharp
Task SetRepeatStateAsync(
    ulong guildId,
    bool isRepeating,
    bool isRepeatingList,
    string? realtimeEventName = null,
    CancellationToken cancellationToken = default);
```

**Additional methods:** `SetRepeatStateAsync`. See the source and its XML contracts for the complete parameter lists.

**Usage:**

The caller supplies the component and argument values used in this example.

```csharp
var result = await playbackStateRepository.GetOrCreateAsync(guildId, cancellationToken);
```

---

### IRepeatListRepository

**File:** `DC bot.Contracts/Interface/Service/Persistence/Playback/IRepeatListRepository.cs`

**Purpose:** Defines shared storage operations for repeat list repository, including caller cancellation and immutable result contracts.

**Methods:**

- `GetTrackIdentifiersAsync` — Reads the legacy repeat-list snapshot in its persisted playback order.

```csharp
Task<IReadOnlyList<string>> GetTrackIdentifiersAsync(ulong guildId, CancellationToken cancellationToken = default);
```

- `ClearAsync` — Removes the guild's persisted legacy repeat-list snapshot.

```csharp
Task ClearAsync(ulong guildId, CancellationToken cancellationToken = default);
```

- `ReplaceAsync` — Replaces the guild's legacy repeat-list snapshot with the supplied ordered track identifiers.

```csharp
Task ReplaceAsync(ulong guildId, IReadOnlyList<string> trackIdentifiers, CancellationToken cancellationToken = default);
```

**Usage:**

The caller supplies the component and argument values used in this example.

```csharp
var result = await repeatListRepository.GetTrackIdentifiersAsync(guildId, cancellationToken);
```

---

## Related Components

- `DC bot.Contracts/Interface/Service/Persistence/README.md` — Parent feature overview and neighboring components.
- `DC bot.Contracts/README.md` — Project setup and architecture.
- `DC bot.Contracts/Interface/Service/Persistence/Models/Playback/` — Defines shared records and state values for current playback state, repeat flags, start ownership, and canonical batch metadata.
- `DC bot.Persistence/` — Repository implementations and database mappings that consume these shared contracts.
