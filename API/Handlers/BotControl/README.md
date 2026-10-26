# BotControl

Reads progress and results of durable bot-control commands.

## Overview

- The handler loads the command, verifies access to its guild, and only returns progress to the user who submitted it.
- A command ID is not an authorization credential. Missing commands, forbidden access, and successful polling remain distinct responses.

## Classes

### BotControlCommandHandler

**File:** `API/Handlers/BotControl/BotControlCommandHandler.cs`

**Purpose:** Returns persisted command status only to the requesting user with guild access.

**Methods:**

- `GetStatusAsync` — Loads command progress and rejects access from users other than the command owner.

```csharp
public static async Task<IResult> GetStatusAsync(
    string commandId,
    HttpContext httpContext,
    IBotControlCommandsRepository commandsRepository,
    IMobileAppUserRepository userRepository,
    CancellationToken cancellationToken);
```

**Usage:**

The caller supplies the component and argument values used in this example.

```csharp
var result = await BotControlCommandHandler.GetStatusAsync(
    commandId,
    httpContext,
    commandsRepository,
    userRepository,
    cancellationToken);
```

---

## Related Components

- `API/Handlers/README.md` — Parent feature overview and neighboring components.
- `API/README.md` — Project setup and architecture.
- `API/Mapping/` — Converts repository records and service outcomes into HTTP response contracts.
- `DC bot.Contracts/Interface/Service/Persistence/BotControl/` — Defines repository operations for durable bot commands, their claim/completion state, and cross-process change signals.
- `DC bot.Contracts/Interface/Service/Persistence/MobileApps/` — Defines repository operations for mobile identities, guild access, refresh sessions, and realtime subscription membership.
- `DC bot.Contracts/` — Shared storage and command protocol used by handlers and services.
