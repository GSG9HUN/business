# Interface

Defines command-progress and guild-snapshot publication operations.

## Overview

- The PostgreSQL listener depends on these contracts; SignalR publisher implementations live in the parent folder.
- Keep cancellation and group-targeting responsibilities explicit when adding a new event category.

## Interfaces

### IBotControlRealtimePublisher

**File:** `API/Realtime/Publishing/Interface/IBotControlRealtimePublisher.cs`

**Purpose:** Defines publication of durable command progress to mobile subscribers.

**Methods:**

- `PublishCommandAsync` — Sends command progress to its owner's guild group and emits the generic command event when different.

```csharp
Task PublishCommandAsync(BotControlCommandRecord command, string eventName, CancellationToken cancellationToken = default);
```

**Usage:**

The caller supplies the component and argument values used in this example.

```csharp
await botControlRealtimePublisher.PublishCommandAsync(command, eventName, cancellationToken);
```

---

### IGuildRealtimePublisher

**File:** `API/Realtime/Publishing/Interface/IGuildRealtimePublisher.cs`

**Purpose:** Defines guild playback, queue, and bot presence snapshot publication.

**Methods:**

- `PublishGuildBotStatusAsync` — Reads and publishes the current bot presence snapshot to authorized guild subscribers.

```csharp
Task PublishGuildBotStatusAsync(ulong guildId, string eventName, CancellationToken cancellationToken = default);
```

- `PublishPlaybackAsync` — Reads and publishes the current playback snapshot to authorized guild subscribers.

```csharp
Task PublishPlaybackAsync(ulong guildId, string eventName, CancellationToken cancellationToken = default);
```

- `PublishQueueAsync` — Reads and publishes the current queue snapshot to authorized guild subscribers.

```csharp
Task PublishQueueAsync(ulong guildId, string eventName, CancellationToken cancellationToken = default);
```

**Usage:**

The caller supplies the component and argument values used in this example.

```csharp
await guildRealtimePublisher.PublishGuildBotStatusAsync(guildId, eventName, cancellationToken);
```

---

## Related Components

- `API/Realtime/Publishing/README.md` — Parent feature overview and neighboring components.
- `API/README.md` — Project setup and architecture.
- `DC bot.Contracts/Interface/Service/Persistence/Models/BotControlCommand/` — Defines shared records and state values for bot control command operations.
- `DC bot.Contracts/` — Shared storage and command protocol used by handlers and services.
