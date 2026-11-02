# Snapshots

Builds queue, playback, and bot-presence read models for publishers.

## Overview

- RealtimeSnapshotProvider delegates playback to the shared playback snapshot service and maps queue and bot-status repository reads.
- A snapshot should have the same meaning as the corresponding REST response. Do not maintain a separate realtime-only playback state machine.

## Subfolders

### Interface/

Defines the read models requested by realtime publishers.

**Files:**

- `IRealtimeSnapshotProvider.cs`

**Purpose:** Implementations combine persistence contracts and response mapping without owning a SignalR connection.

---

## Classes

### RealtimeSnapshotProvider

**File:** `API/Realtime/Snapshots/RealtimeSnapshotProvider.cs`

**Purpose:** Assembles realtime queue, playback, and bot presence snapshots from persistence contracts.

**Implements / inherits:** `IRealtimeSnapshotProvider`.

**Methods:**

- `GetGuildBotStatusSnapshotAsync` — Reads persisted bot presence and maps missing status to disconnected defaults.

```csharp
public async Task<GuildBotStatusResponse> GetGuildBotStatusSnapshotAsync(
    ulong guildId,
    CancellationToken cancellationToken = default);
```

- `GetPlaybackSnapshotAsync` — Reads the shared playback snapshot for a guild.

```csharp
public async Task<PlaybackStatusResponse> GetPlaybackSnapshotAsync(ulong guildId, CancellationToken cancellationToken = default);
```

- `GetQueueSnapshotAsync` — Reads persisted queued items and maps them to a guild queue snapshot.

```csharp
public async Task<QueueResponse> GetQueueSnapshotAsync(ulong guildId, CancellationToken cancellationToken = default);
```

**Usage:**

The caller supplies the component and argument values used in this example.

```csharp
var result = await realtimeSnapshotProvider.GetGuildBotStatusSnapshotAsync(guildId, cancellationToken);
```

---

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

- `API/Realtime/README.md` — Parent feature overview and neighboring components.
- `API/README.md` — Project setup and architecture.
- `API/Realtime/Snapshots/Interface/README.md` — Detailed component contracts and examples for this subfolder.
- `API/Mapping/` — Converts repository records and service outcomes into HTTP response contracts.
- `API/Responses/Guilds/` — Defines client-facing output for guild defaults, premium state, and the mapped premium-audit schema.
- `API/Responses/Playback/` — Defines client-facing output for current playback state, repeat flags, start ownership, and canonical batch metadata.
- `API/Responses/Queue/` — Defines client-facing output for ordered queue entries, canonical batches, conditional start claims, and bounded playback history.
- `DC bot.Contracts/` — Shared storage and command protocol used by handlers and services.
