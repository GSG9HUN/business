# Repeat

Coordinates repeat-one and repeat-list behavior with playback state and canonical batches.

## Overview

- RepeatService uses repositories to toggle repeat state and retrieve the appropriate tracks for another playback cycle.
- Full-list repetition is based on the active batch, including already played items, rather than only the remaining waiting queue. Keep repeat flags and queue transitions consistent.

## Classes

### RepeatService

**File:** `DC bot/Service/Music/Repeat/RepeatService.cs`

**Purpose:** Persists repeat modes and maintains the legacy serialized repeat-list snapshot.

**Implements / inherits:** `IRepeatService`.

**Methods:**

- `InitAsync` — Ensures persisted playback and repeat state exists for the guild.

```csharp
public async Task InitAsync(ulong guildId, CancellationToken cancellationToken = default);
```

- `IsRepeatingAsync` — Reads the guild's persisted single-track repeat mode.

```csharp
public async Task<bool> IsRepeatingAsync(ulong guildId, CancellationToken cancellationToken = default);
```

- `IsRepeatingListAsync` — Reads the guild's persisted list-repeat mode.

```csharp
public async Task<bool> IsRepeatingListAsync(ulong guildId, CancellationToken cancellationToken = default);
```

- `SaveRepeatListSnapshotAsync` — Serializes the supplied current and waiting tracks into the legacy repeat-list snapshot.

```csharp
public async Task SaveRepeatListSnapshotAsync(
    ulong guildId,
    ILavaLinkTrack? currentTrack,
    IReadOnlyCollection<ILavaLinkTrack> queuedTracks,
    CancellationToken cancellationToken = default);
```

- `SetRepeatingAsync` — Persists single-track repeat while retaining list-repeat state and publishing the mode change.

```csharp
public async Task SetRepeatingAsync(ulong guildId, bool value, CancellationToken cancellationToken = default);
```

- `SetRepeatingListAsync` — Persists list-repeat mode and clears the legacy repeat snapshot when disabled.

```csharp
public async Task SetRepeatingListAsync(ulong guildId, bool value, CancellationToken cancellationToken = default);
```

**Usage:**

The caller supplies the component and argument values used in this example.

```csharp
await repeatService.InitAsync(guildId, cancellationToken);
```

---

## Related Components

- `DC bot/Service/Music/README.md` — Parent feature overview and neighboring components.
- `DC bot/README.md` — Project setup and architecture.
- `DC bot/Service/Music/Tracks/` — Adapts Lavalink tracks to the persisted identity format used by queue and playlist storage.
- `DC bot/Interface/Service/Music/Repeat/` — Defines the repeat boundary used by the bot music pipeline.
- `DC bot/Interface/Service/Music/Tracks/` — Defines the tracks boundary used by the bot music pipeline.
- `DC bot.Contracts/BotControl/` — Defines command type identifiers, realtime event names, and optional execution-channel context shared by the API and bot.
- `DC bot/Startup/DependencyInjection/` — Registers the runtime implementations used by these components.
