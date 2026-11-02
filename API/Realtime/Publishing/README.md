# Publishing

Targets authorized mobile subscribers with command progress and fresh guild snapshots.

## Overview

- Command updates target the submitting user's guild group. Guild updates resolve currently authorized subscribed users before sending payloads.
- Requested cancellation propagates. Ordinary snapshot and transport errors are logged as best-effort publication failures; clients can still read REST state.

## Subfolders

### Interface/

Defines command-progress and guild-snapshot publication operations.

**Files:**

- `IBotControlRealtimePublisher.cs`
- `IGuildRealtimePublisher.cs`

**Purpose:** The PostgreSQL listener depends on these contracts; SignalR publisher implementations live in the parent folder.

---

## Classes

### BotControlRealtimePublisher

**File:** `API/Realtime/Publishing/BotControlRealtimePublisher.cs`

**Purpose:** Publishes command progress to the requesting user's guild group and its generic update event.

**Implements / inherits:** `IBotControlRealtimePublisher`.

**Methods:**

- `PublishCommandAsync` — Sends command progress to its owner's guild group and emits the generic command event when different.

```csharp
public async Task PublishCommandAsync(
    BotControlCommandRecord command,
    string eventName,
    CancellationToken cancellationToken = default);
```

**Usage:**

The caller supplies the component and argument values used in this example.

```csharp
await botControlRealtimePublisher.PublishCommandAsync(command, eventName, cancellationToken);
```

---

### GuildRealtimePublisher

**File:** `API/Realtime/Publishing/GuildRealtimePublisher.cs`

**Purpose:** Publishes current snapshots to persisted subscribers whose guild access is still authorized.

**Implements / inherits:** `IGuildRealtimePublisher`.

**Methods:**

- `PublishGuildBotStatusAsync` — Reads and publishes the current bot presence snapshot to authorized guild subscribers.

```csharp
public async Task PublishGuildBotStatusAsync(ulong guildId, string eventName, CancellationToken cancellationToken = default);
```

- `PublishPlaybackAsync` — Reads and publishes the current playback snapshot to authorized guild subscribers.

```csharp
public async Task PublishPlaybackAsync(ulong guildId, string eventName, CancellationToken cancellationToken = default);
```

- `PublishQueueAsync` — Reads and publishes the current queue snapshot to authorized guild subscribers.

```csharp
public async Task PublishQueueAsync(ulong guildId, string eventName, CancellationToken cancellationToken = default);
```

**Usage:**

The caller supplies the component and argument values used in this example.

```csharp
await guildRealtimePublisher.PublishGuildBotStatusAsync(guildId, eventName, cancellationToken);
```

---

## Related Components

- `API/Realtime/README.md` — Parent feature overview and neighboring components.
- `API/README.md` — Project setup and architecture.
- `API/Realtime/Publishing/Interface/README.md` — Detailed component contracts and examples for this subfolder.
- `API/Realtime/Events/` — Defines immutable command-progress and guild snapshot messages sent to mobile clients.
- `API/Realtime/Publishing/Interface/` — Defines command-progress and guild-snapshot publication operations.
- `API/Realtime/Snapshots/Interface/` — Defines the read models requested by realtime publishers.
- `DC bot.Contracts/BotControl/` — Defines command type identifiers, realtime event names, and optional execution-channel context shared by the API and bot.
- `DC bot.Contracts/` — Shared storage and command protocol used by handlers and services.
