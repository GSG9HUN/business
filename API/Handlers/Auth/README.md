# Auth

Coordinates Discord OAuth, mobile login-ticket exchange, refresh-token rotation, and logout.

## Overview

- The browser callback consumes OAuth state, fetches Discord identity and guild memberships, persists the user, and redirects with a short-lived ticket. Exchange creates a mobile session; refresh conditionally rotates its stored token hash.
- DiscordOAuthOptions supplies the mobile redirect. HTTP cancellation reaches OAuth and repository calls. OAuth redirects and token responses have different HTTP shapes.

## Classes

### AuthHandler

**File:** `API/Handlers/Auth/AuthHandler.cs`

**Purpose:** Coordinates Discord OAuth callbacks, mobile login tickets, refresh rotation, and logout.

**Methods:**

- `DiscordCallback` — Consumes OAuth state, synchronizes the Discord user and guilds, and redirects with a mobile login ticket.

```csharp
public static async Task<IResult> DiscordCallback(
    string code,
    string state,
    DiscordOAuthService discordOAuthService,
    OAuthStateStore stateStore,
    AuthTicketStore ticketStore,
    IMobileAppUserRepository userRepository,
    IMobileAppUserSettingsRepository settingsRepository,
    IOptions<DiscordOAuthOptions> options,
    CancellationToken ct);
```

- `DiscordStart` — Creates a cached OAuth state and returns the Discord authorization URL without network I/O.

```csharp
public static Task<IResult> DiscordStart(DiscordOAuthService discordOAuthService, OAuthStateStore stateStore);
```

- `Exchange` — Consumes a mobile login ticket, persists a session, and returns access and refresh tokens.

```csharp
public static async Task<IResult> Exchange(
    ExchangeRequest request,
    AuthTicketStore ticketStore,
    AppTokenService tokenService,
    IMobileAppSessionRepository sessionRepository,
    CancellationToken ct);
```

- `Logout` — Revokes the session identified by the supplied refresh token when it exists.

```csharp
public static async Task<IResult> Logout(
    RefreshRequest request,
    AppTokenService tokenService,
    IMobileAppSessionRepository sessionRepository,
    CancellationToken ct);
```

- `Refresh` — Validates a stored refresh session and conditionally rotates its refresh-token hash before issuing tokens.

```csharp
public static async Task<IResult> Refresh(
    RefreshRequest request,
    AppTokenService tokenService,
    IMobileAppSessionRepository sessionRepository,
    CancellationToken ct);
```

**Usage:**

The caller supplies the component and argument values used in this example.

```csharp
var result = await AuthHandler.DiscordStart(discordOAuthService, stateStore);
```

---

## Related Components

- `API/Handlers/README.md` — Parent feature overview and neighboring components.
- `API/README.md` — Project setup and architecture.
- `API/Errors/` — Defines error categories understood by HTTP result mapping.
- `API/Mapping/` — Converts repository records and service outcomes into HTTP response contracts.
- `API/Results/` — Represents either a successful value or an expected error before conversion to HTTP.
- `API/Requests/Auth/` — Defines client-supplied input for application access/refresh tokens and short-lived login exchange input.
- `DC bot.Contracts/` — Shared storage and command protocol used by handlers and services.
