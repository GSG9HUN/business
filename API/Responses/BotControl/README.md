# BotControl

Defines client-facing output for durable bot commands, their claim/completion state, and cross-process change signals.

## Overview

- Handlers, mappers, and snapshot providers construct these records from shared contract values and return them through HTTP or matching realtime events.
- Keep field meaning consistent for mobile consumers. An output model describes observable state and must not contain a tracked entity or a live SDK object.

## Models and Results

### BotControlCommandResponse

**File:** `API/Responses/BotControl/BotControlCommandResponse.cs`

**Purpose:** Carries the bot control command payload returned to HTTP and mobile clients.

**Record declaration:**

```csharp
public sealed record BotControlCommandResponse(string CommandId, string Type, string State);
```

---

### BotControlCommandStatusResponse

**File:** `API/Responses/BotControl/BotControlCommandStatusResponse.cs`

**Purpose:** Carries the bot control command status payload returned to HTTP and mobile clients.

**Record declaration:**

```csharp
public sealed record BotControlCommandStatusResponse(
    string CommandId,
    string GuildId,
    string Type,
    string State,
    string? ErrorMessage,
    string? ResultJson,
    DateTimeOffset CreatedAtUtc,
    DateTimeOffset? ClaimedAtUtc,
    DateTimeOffset? CompletedAtUtc);
```

---

## Related Components

- `API/Responses/README.md` — Parent feature overview and neighboring components.
- `API/README.md` — Project setup and architecture.
- `DC bot.Contracts/` — Shared storage and command protocol used by handlers and services.
