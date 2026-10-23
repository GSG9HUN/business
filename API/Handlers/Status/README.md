# Status

Maps database connectivity checks to an operational HTTP response.

## Overview

- The handler calls IDbStatusCheck with the request token. Connectivity failures use the existing database-unavailable response; requested cancellation propagates.
- This endpoint checks database reachability. It does not certify Discord voice readiness, Lavalink health, or successful music playback.

## Classes

### StatusHandler

**File:** `API/Handlers/Status/StatusHandler.cs`

**Purpose:** Reports database connectivity through the API result-to-HTTP mapping.

**Methods:**

- `ExecuteAsync` — Checks database connectivity with caller cancellation and maps connectivity failures into HTTP results.

```csharp
public static async Task<IResult> ExecuteAsync(IDbStatusCheck statusCheckRepository, CancellationToken cancellationToken);
```

**Usage:**

The caller supplies the component and argument values used in this example.

```csharp
var result = await StatusHandler.ExecuteAsync(statusCheckRepository, cancellationToken);
```

---

## Related Components

- `API/Handlers/README.md` — Parent feature overview and neighboring components.
- `API/README.md` — Project setup and architecture.
- `API/Errors/` — Defines error categories understood by HTTP result mapping.
- `API/Mapping/` — Converts repository records and service outcomes into HTTP response contracts.
- `API/Results/` — Represents either a successful value or an expected error before conversion to HTTP.
- `DC bot.Contracts/Interface/Service/Persistence/Status/` — Defines repository operations for database reachability checks.
- `DC bot.Contracts/` — Shared storage and command protocol used by handlers and services.
