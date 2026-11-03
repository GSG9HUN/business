# BotControl

Defines repository operations for durable bot commands, their claim/completion state, and cross-process change signals.

## Overview

- The bot and API consume these interfaces; concrete implementations and DI registration live in DC bot.Persistence unless noted below.
- Describe lookup absence, ordering, conditional mutation, and cancellation in the contract. Keep provider-specific SQL and EF types out of this boundary.

## Interfaces

### IBotControlCommandNotifier

**File:** `DC bot.Contracts/Interface/Service/Persistence/BotControl/IBotControlCommandNotifier.cs`

**Purpose:** Defines shared storage operations for bot control command notifier, including caller cancellation and immutable result contracts.

**Methods:**

- `EnsureListeningAsync` — Opens the dedicated PostgreSQL connection and installs LISTEN when needed.

```csharp
Task EnsureListeningAsync(CancellationToken cancellationToken);
```

- `WaitForCommandAsync` — Waits for a command-channel notification on the listening connection until caller cancellation.

```csharp
Task WaitForCommandAsync(CancellationToken cancellationToken);
```

**Usage:**

The caller supplies the component and argument values used in this example.

```csharp
await botControlCommandNotifier.EnsureListeningAsync(cancellationToken);
```

---

### IBotControlCommandsRepository

**File:** `DC bot.Contracts/Interface/Service/Persistence/BotControl/IBotControlCommandsRepository.cs`

**Purpose:** Defines shared storage operations for bot control commands repository, including caller cancellation and immutable result contracts.

**Methods:**

- `GetByCommandIdAsync` — Reads the durable command and its outcome by command identifier, returning null when it does not exist.

```csharp
Task<BotControlCommandRecord?> GetByCommandIdAsync(string commandId, CancellationToken cancellationToken);
```

- `GetStartedAsync` — Lists commands left in the started state for uncertain-outcome recovery rather than automatic replay.

```csharp
Task<IReadOnlyList<BotControlCommandRecord>> GetStartedAsync(CancellationToken ct);
```

- `ClaimNextPendingAsync` — Atomically claims the oldest eligible pending command so competing workers cannot execute the same claim.

```csharp
Task<BotControlCommandRecord?> ClaimNextPendingAsync(CancellationToken ct);
```

- `EnqueueAsync` — Persists a pending mobile command and publishes its worker and realtime notifications within the same transaction.

```csharp
Task<BotControlCommandRecord> EnqueueAsync(ulong guildId, ulong userId, string type, CancellationToken cancellationToken);
```

- `EnqueueAsync` — Persists a pending mobile command and publishes its worker and realtime notifications within the same transaction.

```csharp
Task<BotControlCommandRecord> EnqueueAsync(
    ulong guildId,
    ulong userId,
    string type,
    string? payloadJson,
    CancellationToken cancellationToken);
```

- `MarkDoneAsync` — Persists a successful command result and emits its command-state notification.

```csharp
Task MarkDoneAsync(string commandId, string? resultJson, CancellationToken ct);
```

**Additional methods:** `MarkFailedAsync`. See the source and its XML contracts for the complete parameter lists.

**Usage:**

The caller supplies the component and argument values used in this example.

```csharp
var result = await botControlCommandsRepository.GetByCommandIdAsync(commandId, cancellationToken);
```

---

## Related Components

- `DC bot.Contracts/Interface/Service/Persistence/README.md` — Parent feature overview and neighboring components.
- `DC bot.Contracts/README.md` — Project setup and architecture.
- `DC bot.Contracts/Interface/Service/Persistence/Models/BotControlCommand/` — Defines shared records and state values for bot control command operations.
- `DC bot.Persistence/` — Repository implementations and database mappings that consume these shared contracts.
