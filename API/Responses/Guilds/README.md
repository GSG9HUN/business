# Guilds

Defines client-facing output for guild defaults, premium state, and the mapped premium-audit schema.

## Overview

- Handlers, mappers, and snapshot providers construct these records from shared contract values and return them through HTTP or matching realtime events.
- Keep field meaning consistent for mobile consumers. An output model describes observable state and must not contain a tracked entity or a live SDK object.

## Models and Results

### GuildBotStatusResponse

**File:** `API/Responses/Guilds/GuildBotStatusResponse.cs`

**Purpose:** Carries the guild bot status payload returned to HTTP and mobile clients.

**Record declaration:**

```csharp
public sealed record GuildBotStatusResponse(bool IsOnline, string? ConnectedVoiceChannelName, int ConnectedVoiceUserCount);
```

---

### GuildSummaryResponse

**File:** `API/Responses/Guilds/GuildSummaryResponse.cs`

**Purpose:** Carries the guild summary payload returned to HTTP and mobile clients.

**Record declaration:**

```csharp
public sealed record GuildSummaryResponse(
    ulong GuildId,
    string Name,
    string? IconUrl,
    string AccessLevel,
    GuildBotStatusResponse BotStatus);
```

---

## Related Components

- `API/Responses/README.md` — Parent feature overview and neighboring components.
- `API/README.md` — Project setup and architecture.
- `DC bot.Contracts/` — Shared storage and command protocol used by handlers and services.
