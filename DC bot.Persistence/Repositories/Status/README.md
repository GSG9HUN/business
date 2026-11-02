# Status

Implements IDbStatusCheck without requiring a feature-specific data query.

## Overview

- DbStatusCheck creates a context through the factory and calls the provider's connectivity check with caller cancellation.
- Requested cancellation propagates rather than becoming false. A false connectivity result describes database reachability, not Discord or Lavalink readiness.

## Classes

### DbStatusCheck

**File:** `DC bot.Persistence/Repositories/Status/DbStatusCheck.cs`

**Purpose:** Probes database connectivity without converting requested cancellation into a connectivity failure.

**Implements / inherits:** `IDbStatusCheck`.

**Methods:**

- `CanConnectAsync` — Checks database connectivity; requested cancellation propagates while ordinary connectivity failures return false.

```csharp
public async Task<bool> CanConnectAsync(CancellationToken cancellationToken = default);
```

**Usage:**

The caller supplies the component and argument values used in this example.

```csharp
var result = await dbStatusCheck.CanConnectAsync(cancellationToken);
```

---

## Related Components

- `DC bot.Persistence/Repositories/README.md` — Parent feature overview and neighboring components.
- `DC bot.Persistence/README.md` — Project setup and architecture.
- `DC bot.Persistence/Db/` — Owns the EF Core context, design-time creation, and startup migration runner.
- `DC bot.Contracts/Interface/Service/Persistence/Status/` — Defines repository operations for database reachability checks.
- `DC bot.Contracts/Interface/Service/Persistence/` — Shared interfaces and immutable values implemented by persistence.
