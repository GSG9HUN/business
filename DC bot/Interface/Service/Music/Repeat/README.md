# Repeat

Defines the repeat boundary used by the bot music pipeline.

## Overview

- Implementations live under Service/Music/Repeat; commands and orchestrators exchange these contracts instead of constructing transport or persistence dependencies themselves.
- Result states distinguish completed, rejected, pending, and superseded work where applicable. Preserve ownership and cancellation semantics when adding an operation.

## Interfaces

### IRepeatService

**File:** `DC bot/Interface/Service/Music/Repeat/IRepeatService.cs`

**Purpose:** Defines the repeat service boundary so callers are independent of its concrete implementation.

**Methods:**

- `InitAsync` — Ensures persisted playback and repeat state exists for the guild.

```csharp
Task InitAsync(ulong guildId, CancellationToken cancellationToken = default);
```

- `IsRepeatingAsync` — Reads the guild's persisted single-track repeat mode.

```csharp
Task<bool> IsRepeatingAsync(ulong guildId, CancellationToken cancellationToken = default);
```

- `IsRepeatingListAsync` — Reads the guild's persisted list-repeat mode.

```csharp
Task<bool> IsRepeatingListAsync(ulong guildId, CancellationToken cancellationToken = default);
```

- `SaveRepeatListSnapshotAsync` — Serializes the supplied current and waiting tracks into the legacy repeat-list snapshot.

```csharp
Task SaveRepeatListSnapshotAsync(
    ulong guildId,
    ILavaLinkTrack? currentTrack,
    IReadOnlyCollection<ILavaLinkTrack> queuedTracks,
    CancellationToken cancellationToken = default);
```

- `SetRepeatingAsync` — Persists single-track repeat while retaining list-repeat state and publishing the mode change.

```csharp
Task SetRepeatingAsync(ulong guildId, bool value, CancellationToken cancellationToken = default);
```

- `SetRepeatingListAsync` — Persists list-repeat mode and clears the legacy repeat snapshot when disabled.

```csharp
Task SetRepeatingListAsync(ulong guildId, bool value, CancellationToken cancellationToken = default);
```

**Usage:**

The caller supplies the component and argument values used in this example.

```csharp
await repeatService.InitAsync(guildId, cancellationToken);
```

---

## Related Components

- `DC bot/Interface/Service/Music/README.md` — Parent feature overview and neighboring components.
- `DC bot/README.md` — Project setup and architecture.
- `DC bot/Interface/Service/Music/Tracks/` — Defines the tracks boundary used by the bot music pipeline.
- `DC bot/Startup/DependencyInjection/` — Registers the runtime implementations used by these components.
