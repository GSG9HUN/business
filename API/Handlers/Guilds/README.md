# Guilds

Lists guilds visible to the authenticated mobile user and assembles their bot-presence metadata.

## Overview

- Memberships come from the mobile user repository; bot voice status is loaded separately and mapped into the guild response.
- A stored guild membership controls API visibility. Bot voice presence is display state and is not proof that Lavalink is ready to play.

## Classes

### GuildHandler

**File:** `API/Handlers/Guilds/GuildHandler.cs`

**Purpose:** Builds guild HTTP responses from the authenticated user's memberships and bot status.

**Methods:**

- `Guild` — Returns a single guild visible to the authenticated user or an HTTP access error.

```csharp
public static async Task<IResult> Guild(
    HttpContext httpContext,
    IMobileAppUserRepository repository,
    IGuildBotStatusRepository botStatusRepository,
    CancellationToken cancellationToken);
```

- `Guilds` — Returns the authenticated user's guilds with their persisted bot presence.

```csharp
public static async Task<IResult> Guilds(
    HttpContext httpContext,
    IMobileAppUserRepository repository,
    IGuildBotStatusRepository botStatusRepository,
    CancellationToken cancellationToken);
```

**Usage:**

The caller supplies the component and argument values used in this example.

```csharp
var result = await GuildHandler.Guild(httpContext, repository, botStatusRepository, cancellationToken);
```

---

## Related Components

- `API/Handlers/README.md` — Parent feature overview and neighboring components.
- `API/README.md` — Project setup and architecture.
- `API/Errors/` — Defines error categories understood by HTTP result mapping.
- `API/Mapping/` — Converts repository records and service outcomes into HTTP response contracts.
- `API/Results/` — Represents either a successful value or an expected error before conversion to HTTP.
- `DC bot.Contracts/Interface/Service/Persistence/GuildBotStatus/` — Defines repository operations for persisted bot voice presence, channel display metadata, and voice-user counts.
- `DC bot.Contracts/` — Shared storage and command protocol used by handlers and services.
