# Lavalink

Defines the lavalink boundary used by the bot music pipeline.

## Overview

- Implementations live under Service/Music/Lavalink; commands and orchestrators exchange these contracts instead of constructing transport or persistence dependencies themselves.
- Result states distinguish completed, rejected, pending, and superseded work where applicable. Preserve ownership and cancellation semantics when adding an operation.

## Interfaces

### ILavalinkNodeConnectionService

**File:** `DC bot/Interface/Service/Music/Lavalink/ILavalinkNodeConnectionService.cs`

**Purpose:** Defines the lavalink node connection service boundary so callers are independent of its concrete implementation.

**Methods:**

- `ConnectAsync` — Connects the configured Discord or Lavalink runtime boundary and propagates connection failures.

```csharp
Task ConnectAsync(CancellationToken cancellationToken = default);
```

**Usage:**

The caller supplies the component and argument values used in this example.

```csharp
await lavalinkNodeConnectionService.ConnectAsync(cancellationToken);
```

---

## Related Components

- `DC bot/Interface/Service/Music/README.md` — Parent feature overview and neighboring components.
- `DC bot/README.md` — Project setup and architecture.
- `DC bot/Startup/DependencyInjection/` — Registers the runtime implementations used by these components.
