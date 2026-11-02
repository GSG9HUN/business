# Status

Defines repository operations for database reachability checks.

## Overview

- The bot and API consume these interfaces; concrete implementations and DI registration live in DC bot.Persistence unless noted below.
- Describe lookup absence, ordering, conditional mutation, and cancellation in the contract. Keep provider-specific SQL and EF types out of this boundary.

## Interfaces

### IDbStatusCheck

**File:** `DC bot.Contracts/Interface/Service/Persistence/Status/IDbStatusCheck.cs`

**Purpose:** Defines shared storage operations for db status check, including caller cancellation and immutable result contracts.

**Methods:**

- `CanConnectAsync` — Checks database connectivity; requested cancellation propagates while ordinary connectivity failures return false.

```csharp
Task<bool> CanConnectAsync(CancellationToken cancellationToken = default);
```

**Usage:**

The caller supplies the component and argument values used in this example.

```csharp
var result = await dbStatusCheck.CanConnectAsync(cancellationToken);
```

---

## Related Components

- `DC bot.Contracts/Interface/Service/Persistence/README.md` — Parent feature overview and neighboring components.
- `DC bot.Contracts/README.md` — Project setup and architecture.
- `DC bot.Persistence/` — Repository implementations and database mappings that consume these shared contracts.
