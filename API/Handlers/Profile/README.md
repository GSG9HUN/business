# Profile

Reads the authenticated user's profile and applies partial mobile preference updates.

## Overview

- GetMyProfile loads identity only; GetProfile also loads settings. UpdateSettings normalizes supplied language and theme values before patching preferences.
- Omitted patch fields remain unchanged. The subject claim determines the user; clients do not select another user's settings through request input.

## Classes

### ProfileHandler

**File:** `API/Handlers/Profile/ProfileHandler.cs`

**Purpose:** Reads the authenticated user's profile and validates partial mobile settings updates.

**Methods:**

- `GetMyProfile` — Returns the authenticated user's stored identity without loading settings.

```csharp
public static async Task<IResult> GetMyProfile(
    HttpContext httpContext,
    IMobileAppUserRepository userRepository,
    CancellationToken cancellationToken);
```

- `GetProfile` — Returns the authenticated user's identity and persisted or default mobile settings.

```csharp
public static async Task<IResult> GetProfile(
    HttpContext httpContext,
    IMobileAppUserRepository userRepository,
    IMobileAppUserSettingsRepository settingsRepository,
    CancellationToken cancellationToken);
```

- `UpdateSettings` — Normalizes supplied language and theme values and persists only the requested settings changes.

```csharp
public static async Task<IResult> UpdateSettings(
    HttpContext httpContext,
    UpdateProfileSettingsRequest request,
    IMobileAppUserSettingsRepository settingsRepository,
    CancellationToken cancellationToken);
```

**Usage:**

The caller supplies the component and argument values used in this example.

```csharp
var result = await ProfileHandler.GetMyProfile(httpContext, userRepository, cancellationToken);
```

---

## Related Components

- `API/Handlers/README.md` — Parent feature overview and neighboring components.
- `API/README.md` — Project setup and architecture.
- `API/Errors/` — Defines error categories understood by HTTP result mapping.
- `API/Mapping/` — Converts repository records and service outcomes into HTTP response contracts.
- `API/Results/` — Represents either a successful value or an expected error before conversion to HTTP.
- `API/Requests/Profile/` — Defines client-supplied input for mobile user identity and optional preference updates.
- `DC bot.Contracts/` — Shared storage and command protocol used by handlers and services.
