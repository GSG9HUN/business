# Auth

Defines client-facing output for application access/refresh tokens and short-lived login exchange input.

## Overview

- Handlers, mappers, and snapshot providers construct these records from shared contract values and return them through HTTP or matching realtime events.
- Keep field meaning consistent for mobile consumers. An output model describes observable state and must not contain a tracked entity or a live SDK object.

## Models and Results

### AuthSessionResponse

**File:** `API/Responses/Auth/AuthSessionResponse.cs`

**Purpose:** Carries the auth session payload returned to HTTP and mobile clients.

**Record declaration:**

```csharp
public sealed record AuthSessionResponse(string AccessToken, string RefreshToken, int ExpiresInSeconds, long ExpiresAtMillis);
```

---

## Related Components

- `API/Responses/README.md` — Parent feature overview and neighboring components.
- `API/README.md` — Project setup and architecture.
- `DC bot.Contracts/` — Shared storage and command protocol used by handlers and services.
