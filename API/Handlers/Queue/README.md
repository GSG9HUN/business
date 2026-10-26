# Queue

Reads a guild queue and submits authorized queue mutation commands.

## Overview

- Enqueue validates the query and optional channel identifiers, resolves requester display metadata, and stores a bot play command. Clear, remove, shuffle, and move also use the command bridge.
- Queue display positions and zero-based move indices are separate inputs. The bot worker owns actual playback effects after command acceptance.

## Classes

### QueueHandlers

**File:** `API/Handlers/Queue/QueueHandlers.cs`

**Purpose:** Reads authorized guild queues and submits durable bot commands for queue mutations.

**Methods:**

- `GetAsync` — Returns the authorized guild's currently queued, decodable tracks.

```csharp
public static async Task<IResult> GetAsync(
    HttpContext httpContext,
    IMobileAppUserRepository userRepository,
    IQueueRepository queueRepository,
    CancellationToken cancellationToken);
```

- `ClearAsync` — Queues a durable clear command for an authorized guild.

```csharp
public static async Task<IResult> ClearAsync(
    HttpContext httpContext,
    IMobileAppUserRepository userRepository,
    IBotControlCommandsRepository commandsRepository,
    CancellationToken cancellationToken);
```

- `EnqueueAsync` — Validates the enqueue request and optional channel identifiers and persists a bot play command.

```csharp
public static async Task<IResult> EnqueueAsync(
    HttpContext httpContext,
    EnqueueRequest request,
    IMobileAppUserRepository userRepository,
    IBotControlCommandsRepository commandsRepository,
    CancellationToken cancellationToken);
```

- `MoveToIndexAsync` — Validates nonnegative source and target indices and queues a durable move command.

```csharp
public static async Task<IResult> MoveToIndexAsync(
    HttpContext httpContext,
    int trackIndex,
    int targetIndex,
    IMobileAppUserRepository userRepository,
    IBotControlCommandsRepository commandsRepository,
    CancellationToken cancellationToken);
```

- `RemoveAsync` — Validates a one-based track number and queues a durable remove command.

```csharp
public static async Task<IResult> RemoveAsync(
    HttpContext httpContext,
    int trackNumber,
    IMobileAppUserRepository userRepository,
    IBotControlCommandsRepository commandsRepository,
    CancellationToken cancellationToken);
```

- `ShuffleAsync` — Queues a durable shuffle command for an authorized guild.

```csharp
public static async Task<IResult> ShuffleAsync(
    HttpContext httpContext,
    IMobileAppUserRepository userRepository,
    IBotControlCommandsRepository commandsRepository,
    CancellationToken cancellationToken);
```

**Usage:**

The caller supplies the component and argument values used in this example.

```csharp
var result = await QueueHandlers.GetAsync(httpContext, userRepository, queueRepository, cancellationToken);
```

---

## Related Components

- `API/Handlers/README.md` — Parent feature overview and neighboring components.
- `API/README.md` — Project setup and architecture.
- `API/Mapping/` — Converts repository records and service outcomes into HTTP response contracts.
- `API/Requests/Queue/` — Defines client-supplied input for ordered queue entries, canonical batches, conditional start claims, and bounded playback history.
- `DC bot.Contracts/BotControl/` — Defines command type identifiers, realtime event names, and optional execution-channel context shared by the API and bot.
- `DC bot.Contracts/BotControl/Payloads/` — Defines the serialized arguments stored with durable commands submitted by the API and consumed by the bot worker.
- `DC bot.Contracts/` — Shared storage and command protocol used by handlers and services.
