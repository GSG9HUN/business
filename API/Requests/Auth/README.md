# Auth

Defines client-supplied input for application access/refresh tokens and short-lived login exchange input.

## Overview

- Minimal API binding constructs these values; feature filters and handlers normalize and validate them before repository or service calls.
- Optional input can mean leave unchanged rather than reset. Request fields are not authorization evidence; the authenticated principal and stored guild access determine permission.

## Models and Results

### ExchangeRequest

**File:** `API/Requests/Auth/ExchangeRequest.cs`

**Purpose:** Carries client input for exchange processing; handlers and endpoint filters validate its values.

**Record declaration:**

```csharp
public sealed record ExchangeRequest(string Ticket);
```

---

### RefreshRequest

**File:** `API/Requests/Auth/RefreshRequest.cs`

**Purpose:** Carries client input for refresh processing; handlers and endpoint filters validate its values.

**Record declaration:**

```csharp
public sealed record RefreshRequest(string RefreshToken);
```

---

## Related Components

- `API/Requests/README.md` — Parent feature overview and neighboring components.
- `API/README.md` — Project setup and architecture.
- `DC bot.Contracts/` — Shared storage and command protocol used by handlers and services.
