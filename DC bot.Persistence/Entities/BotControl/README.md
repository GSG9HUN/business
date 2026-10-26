# BotControl

Represents the database rows used for durable bot commands, their claim/completion state, and cross-process change signals.

## Overview

- Feature repositories read and mutate these values in short-lived EF contexts, then map results into DC bot.Contracts records.
- Entity properties describe persisted state. Keep HTTP response formatting, Discord objects, and playback transport behavior outside these types.

## Classes

### BotControlCommandEntity

**File:** `DC bot.Persistence/Entities/BotControl/BotControlCommandEntity.cs`

**Purpose:** Represents the persisted bot control command row mapped by the EF Core model.

**Properties:**

```csharp
// The identifier used to correlate the command and its execution result.
public string CommandId { get; set; }
public ulong GuildId { get; set; }
public string Type { get; set; }
public string? PayloadJson { get; set; }
public ulong UserId { get; set; }
public string? ResultJson { get; set; }
public BotControlCommandState Status { get; set; }
public string? ErrorMessage { get; set; }
public DateTimeOffset CreatedAtUtc { get; set; }
public DateTimeOffset? ClaimedAtUtc { get; set; }
public DateTimeOffset? CompletedAtUtc { get; set; }
```

---

## Related Components

- `DC bot.Persistence/Entities/README.md` — Parent feature overview and neighboring components.
- `DC bot.Persistence/README.md` — Project setup and architecture.
- `DC bot.Contracts/Interface/Service/Persistence/Models/BotControlCommand/` — Defines shared records and state values for bot control command operations.
- `DC bot.Contracts/Interface/Service/Persistence/` — Shared interfaces and immutable values implemented by persistence.
