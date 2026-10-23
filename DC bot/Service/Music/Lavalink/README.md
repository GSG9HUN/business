# Lavalink

Owns connection operations for the Lavalink audio node.

## Overview

- LavalinkNodeConnectionService isolates node-level connectivity from guild player lifecycle and individual track transport calls.
- A ready node and a ready Discord voice player are separate conditions; do not treat one as proof of the other.

## Classes

### LavalinkNodeConnectionService

**File:** `DC bot/Service/Music/Lavalink/LavalinkNodeConnectionService.cs`

**Purpose:** Connects the configured Lavalink node and records connection failures.

**Implements / inherits:** `ILavalinkNodeConnectionService`.

**Methods:**

- `ConnectAsync` — Connects the configured Discord or Lavalink runtime boundary and propagates connection failures.

```csharp
public async Task ConnectAsync(CancellationToken cancellationToken = default);
```

**Usage:**

The caller supplies the component and argument values used in this example.

```csharp
await lavalinkNodeConnectionService.ConnectAsync(cancellationToken);
```

---

## Related Components

- `DC bot/Service/Music/README.md` — Parent feature overview and neighboring components.
- `DC bot/README.md` — Project setup and architecture.
- `DC bot/Logging/` — Defines typed log messages and reusable scopes for bot operations.
- `DC bot/Exceptions/Music/` — Distinguishes Lavalink failures, track-load failures, expired sessions, and busy control operations.
- `DC bot/Interface/Service/Music/Lavalink/` — Defines the lavalink boundary used by the bot music pipeline.
- `DC bot/Startup/DependencyInjection/` — Registers the runtime implementations used by these components.
