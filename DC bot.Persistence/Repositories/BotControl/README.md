# BotControl

Implements shared storage operations for durable bot commands, their claim/completion state, and cross-process change signals.

## Overview

- Callers resolve the contracts through AddPersistenceServices; implementations create contexts through IDbContextFactory and map database state into shared records.
- Forward caller cancellation through database I/O. Preserve transactional ownership and use the shared retry/rollback helpers where a multi-step mutation requires them.
- Command claims and completion metadata make the database the durable source of work. The notifier uses a dedicated PostgreSQL connection; its IAsyncDisposable signature is an SDK contract.

## Classes

### BotControlCommandsRepository

**File:** `DC bot.Persistence/Repositories/BotControl/BotControlCommandsRepository.cs`

**Purpose:** Reads and mutates bot control commands state using short-lived contexts from the injected EF factory.

**Implements / inherits:** `IBotControlCommandsRepository`.

**Methods:**

- `GetByCommandIdAsync` — Reads the durable command and its outcome by command identifier, returning null when it does not exist.

```csharp
public async Task<BotControlCommandRecord?> GetByCommandIdAsync(string commandId, CancellationToken cancellationToken);
```

- `GetStartedAsync` — Lists commands left in the started state for uncertain-outcome recovery rather than automatic replay.

```csharp
public async Task<IReadOnlyList<BotControlCommandRecord>> GetStartedAsync(CancellationToken cancellationToken);
```

- `ClaimNextPendingAsync` — Atomically claims the oldest eligible pending command so competing workers cannot execute the same claim.

```csharp
public Task<BotControlCommandRecord?> ClaimNextPendingAsync(CancellationToken cancellationToken);
```

- `EnqueueAsync` — Persists a pending mobile command and publishes its worker and realtime notifications within the same transaction.

```csharp
public async Task<BotControlCommandRecord> EnqueueAsync(
    ulong guildId,
    ulong userId,
    string type,
    CancellationToken cancellationToken);
```

- `EnqueueAsync` — Persists a pending mobile command and publishes its worker and realtime notifications within the same transaction.

```csharp
public async Task<BotControlCommandRecord> EnqueueAsync(
    ulong guildId,
    ulong userId,
    string type,
    string? payloadJson,
    CancellationToken cancellationToken);
```

- `MarkDoneAsync` — Persists a successful command result and emits its command-state notification.

```csharp
public async Task MarkDoneAsync(string commandId, string? resultJson, CancellationToken cancellationToken);
```

**Additional methods:** `MarkFailedAsync`. See the source and its XML contracts for the complete parameter lists.

**Usage:**

The caller supplies the component and argument values used in this example.

```csharp
var result = await botControlCommandsRepository.GetByCommandIdAsync(commandId, cancellationToken);
```

---

### PostgreSqlBotControlCommandNotifier

**File:** `DC bot.Persistence/Repositories/BotControl/PostgreSqlBotControlCommandNotifier.cs`

**Purpose:** Owns the long-lived PostgreSQL LISTEN connection used to wake the durable mobile-command worker.

**Implements / inherits:** `IBotControlCommandNotifier`, `IAsyncDisposable`.

**Methods:**

- `DisposeAsync` — Implements `DisposeAsync` using the contract shown below.

```csharp
public async ValueTask DisposeAsync();
```

- `EnsureListeningAsync` — Opens the dedicated PostgreSQL connection and installs LISTEN when needed.

```csharp
public async Task EnsureListeningAsync(CancellationToken cancellationToken);
```

- `WaitForCommandAsync` — Waits for a command-channel notification on the listening connection until caller cancellation.

```csharp
public async Task WaitForCommandAsync(CancellationToken cancellationToken);
```

**Usage:**

The caller supplies the component and argument values used in this example.

```csharp
await postgreSqlBotControlCommandNotifier.EnsureListeningAsync(cancellationToken);
```

---

## Related Components

- `DC bot.Persistence/Repositories/README.md` — Parent feature overview and neighboring components.
- `DC bot.Persistence/README.md` — Project setup and architecture.
- `DC bot.Persistence/Db/` — Owns the EF Core context, design-time creation, and startup migration runner.
- `DC bot.Persistence/Entities/BotControl/` — Represents the database rows used for durable bot commands, their claim/completion state, and cross-process change signals.
- `DC bot.Persistence/Repositories/Shared/` — Provides PostgreSQL transaction retries and owned rollback handling shared by repositories.
- `DC bot.Contracts/BotControl/` — Defines command type identifiers, realtime event names, and optional execution-channel context shared by the API and bot.
- `DC bot.Contracts/Interface/Service/Persistence/` — Shared interfaces and immutable values implemented by persistence.
