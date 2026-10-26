# BotControlCommand

Defines shared records and state values for bot control command operations.

## Overview

- Persistence maps entities into these values, and application services or API mappers consume them without database-provider dependencies.
- Keep wire response DTOs in API and tracked entities in DC bot.Persistence. Changing a record affects both repository implementations and their consumers.

## Models and Results

### BotControlCommandRecord

**File:** `DC bot.Contracts/Interface/Service/Persistence/Models/BotControlCommand/BotControlCommandRecord.cs`

**Purpose:** Carries immutable bot control command values across the shared storage or mobile-command boundary.

**Record declaration:**

```csharp
public sealed record BotControlCommandRecord(
    string CommandId,
    ulong GuildId,
    ulong UserId,
    string Type,
    BotControlCommandState State,
    string? PayloadJson,
    string? ErrorMessage,
    DateTimeOffset CreatedAtUtc,
    DateTimeOffset? ClaimedAtUtc,
    DateTimeOffset? CompletedAtUtc,
    string? ResultJson);
```

---

### BotControlCommandState

**File:** `DC bot.Contracts/Interface/Service/Persistence/Models/BotControlCommand/BotControlCommandState.cs`

**Purpose:** Names the durable command lifecycle states shared by API polling, the bot worker, and realtime progress payloads.

**Values:**

```csharp
public enum BotControlCommandState : short
{
    // The command is waiting to be claimed by a worker.
    Pending = 0,
    // A worker has claimed the command and started executing it.
    Started = 1,
    // The command completed successfully.
    Done = 2,
    // The operation failed.
    Failed = 3
}
```

---

## Related Components

- `DC bot.Contracts/Interface/Service/Persistence/Models/README.md` — Parent feature overview and neighboring components.
- `DC bot.Contracts/README.md` — Project setup and architecture.
- `DC bot.Persistence/` — Repository implementations and database mappings that consume these shared contracts.
