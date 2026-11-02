# Lifecycle

Defines the lifecycle boundary used by the bot music pipeline.

## Overview

- Implementations live under Service/Music/Lifecycle; commands and orchestrators exchange these contracts instead of constructing transport or persistence dependencies themselves.
- Result states distinguish completed, rejected, pending, and superseded work where applicable. Preserve ownership and cancellation semantics when adding an operation.

## Interfaces

### IPlayerConnectionService

**File:** `DC bot/Interface/Service/Music/Lifecycle/IPlayerConnectionService.cs`

**Purpose:** Defines the player connection service boundary so callers are independent of its concrete implementation.

**Methods:**

- `TryGetAndValidateExistingPlayerAsync` — Resolves and validates the existing player without creating a voice connection.

```csharp
Task<(
    IDiscordMessage message,
    IDiscordChannel? channel,
    CancellationToken cancellationToken = default);
```

- `TryJoinAndValidateAsync` — Joins or resolves the voice player and waits for readiness within the caller's join deadline.

```csharp
Task<(
    IDiscordMessage message,
    IDiscordChannel? channel,
    CancellationToken cancellationToken = default);
```

**Usage:**

The caller supplies the component and argument values used in this example.

```csharp
var result = await playerConnectionService.TryGetAndValidateExistingPlayerAsync(message, channel, cancellationToken);
```

---

## Related Components

- `DC bot/Interface/Service/Music/README.md` — Parent feature overview and neighboring components.
- `DC bot/README.md` — Project setup and architecture.
- `DC bot/Interface/Discord/` — Abstracts the Discord message, channel, guild, member, user, and voice-state values consumed by commands.
- `DC bot/Startup/DependencyInjection/` — Registers the runtime implementations used by these components.
