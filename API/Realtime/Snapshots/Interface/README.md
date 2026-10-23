# Interface

Defines the read models requested by realtime publishers.

## Overview

- Implementations combine persistence contracts and response mapping without owning a SignalR connection.
- Pass caller cancellation through snapshot reads and keep transport-specific sending outside this contract.

## Interfaces

### IRealtimeSnapshotProvider

**File:** `API/Realtime/Snapshots/Interface/IRealtimeSnapshotProvider.cs`

**Purpose:** Defines snapshot reads shared by realtime publishers.

**Methods:**

- `GetGuildBotStatusSnapshotAsync` — Reads persisted bot presence and maps missing status to disconnected defaults.

```csharp
Task<GuildBotStatusResponse> GetGuildBotStatusSnapshotAsync(ulong guildId, CancellationToken cancellationToken = default);
```

- `GetPlaybackSnapshotAsync` — Reads the shared playback snapshot for a guild.

```csharp
Task<PlaybackStatusResponse> GetPlaybackSnapshotAsync(ulong guildId, CancellationToken cancellationToken = default);
```

- `GetQueueSnapshotAsync` — Reads persisted queued items and maps them to a guild queue snapshot.

```csharp
Task<QueueResponse> GetQueueSnapshotAsync(ulong guildId, CancellationToken cancellationToken = default);
```

**Usage:**

The caller supplies the component and argument values used in this example.

```csharp
var result = await realtimeSnapshotProvider.GetGuildBotStatusSnapshotAsync(guildId, cancellationToken);
```

---

## Related Components

- `API/Realtime/Snapshots/README.md` — Parent feature overview and neighboring components.
- `API/README.md` — Project setup and architecture.
- `API/Responses/Guilds/` — Defines client-facing output for guild defaults, premium state, and the mapped premium-audit schema.
- `API/Responses/Playback/` — Defines client-facing output for current playback state, repeat flags, start ownership, and canonical batch metadata.
- `API/Responses/Queue/` — Defines client-facing output for ordered queue entries, canonical batches, conditional start claims, and bounded playback history.
- `DC bot.Contracts/` — Shared storage and command protocol used by handlers and services.
