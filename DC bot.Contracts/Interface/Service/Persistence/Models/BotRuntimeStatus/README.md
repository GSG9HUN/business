# BotRuntimeStatus

Defines shared records and state values for the retained bot-runtime heartbeat and liveness contract.

## Overview

- Persistence maps entities into these values, and application services or API mappers consume them without database-provider dependencies.
- Keep wire response DTOs in API and tracked entities in DC bot.Persistence. Changing a record affects both repository implementations and their consumers.
- BotRuntimeStatusRecord supports the retained heartbeat contract. There is no matching runtime repository registration in the current persistence composition root.

## Models and Results

### BotRuntimeStatusRecord

**File:** `DC bot.Contracts/Interface/Service/Persistence/Models/BotRuntimeStatus/BotRuntimeStatusRecord.cs`

**Purpose:** Carries immutable bot runtime status values across the shared storage or mobile-command boundary.

**Record declaration:**

```csharp
public record BotRuntimeStatusRecord(int Id, DateTimeOffset LastHeartbeatAtUtc, DateTimeOffset StartedAtUtc);
```

---

## Related Components

- `DC bot.Contracts/Interface/Service/Persistence/Models/README.md` — Parent feature overview and neighboring components.
- `DC bot.Contracts/README.md` — Project setup and architecture.
- `DC bot.Persistence/` — Repository implementations and database mappings that consume these shared contracts.
