# BotRuntimeStatus

Defines repository operations for the retained bot-runtime heartbeat and liveness contract.

## Overview

- The bot and API consume these interfaces; concrete implementations and DI registration live in DC bot.Persistence unless noted below.
- Describe lookup absence, ordering, conditional mutation, and cancellation in the contract. Keep provider-specific SQL and EF types out of this boundary.
- This is a retained contract, not an active readiness gate. The current persistence registrations do not bind IBotRuntimeStatusRepository, so its presence does not imply a running heartbeat writer.

## Interfaces

### IBotRuntimeStatusRepository

**File:** `DC bot.Contracts/Interface/Service/Persistence/BotRuntimeStatus/IBotRuntimeStatusRepository.cs`

**Purpose:** Retains the heartbeat upsert and current-status read contract. No implementation of this interface is registered by the current persistence composition root.

**Methods:**

- `GetCurrentAsync` — Reads the current persisted bot-runtime status when available.

```csharp
Task<BotRuntimeStatusRecord?> GetCurrentAsync(CancellationToken ct = default);
```

- `UpsertHeartbeatAsync` — Persists the bot instance's runtime heartbeat and liveness metadata.

```csharp
Task UpsertHeartbeatAsync(DateTimeOffset heartbeatAtUtc, CancellationToken ct = default);
```

**Usage:**

The caller supplies the component and argument values used in this example.

```csharp
var result = await botRuntimeStatusRepository.GetCurrentAsync(ct);
```

---

## Related Components

- `DC bot.Contracts/Interface/Service/Persistence/README.md` — Parent feature overview and neighboring components.
- `DC bot.Contracts/README.md` — Project setup and architecture.
- `DC bot.Contracts/Interface/Service/Persistence/Models/BotRuntimeStatus/` — Defines shared records and state values for the retained bot-runtime heartbeat and liveness contract.
- `DC bot.Persistence/` — Repository implementations and database mappings that consume these shared contracts.
