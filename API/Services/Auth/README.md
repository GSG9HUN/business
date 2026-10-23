# Auth

Builds Discord OAuth requests and manages mobile access tokens, refresh tokens, cached callback state, and login tickets.

## Overview

- DiscordOAuthService uses a factory-managed HTTP client and DiscordOAuthOptions. AppTokenService signs 15-minute access tokens using AppAuthOptions; refresh sessions persist hashes instead of raw refresh tokens.
- OAuth state expires after five minutes and login tickets after two. Both stores are process-local memory caches; sharing login flow across multiple API instances requires shared state or routing affinity.

## Subfolders

### Discord/

Groups the upstream Discord REST payloads used by authentication services.

**Files:**

- `README.md` — Folder overview and navigation to its source components.

**Purpose:** DiscordOAuthService deserializes token, current-user, and guild responses from the Response subfolder.

---

## Classes

### AppAuthOptions

**File:** `API/Services/Auth/AppAuthOptions.cs`

**Purpose:** Configures the signing key, issuer, and audience of mobile application access tokens.

**Properties:**

```csharp
// The key used to sign application access tokens.
public string SigningKey { get; set; }
public string Issuer { get; set; }
public string Audience { get; set; }
```

---

### AppTokenService

**File:** `API/Services/Auth/AppTokenService.cs`

**Purpose:** Creates signed mobile access tokens and cryptographically random refresh tokens and their hashes.

**Methods:**

- `CreateAccessToken` — Signs a short-lived JWT containing the Discord subject, session identifier, and unique token identifier.

```csharp
public string CreateAccessToken(Guid sessionId, ulong discordUserId);
```

- `CreateRefreshToken` — Creates a Base64-encoded refresh token from 64 cryptographically random bytes.

```csharp
public string CreateRefreshToken();
```

- `HashRefreshToken` — Hashes a refresh token with SHA-256 for persistence without storing the raw token.

```csharp
public string HashRefreshToken(string refreshToken);
```

**Usage:**

The caller supplies the component and argument values used in this example.

```csharp
var result = appTokenService.CreateAccessToken(sessionId, discordUserId);
```

---

### AuthTicketStore

**File:** `API/Services/Auth/AuthTicketStore.cs`

**Purpose:** Stores short-lived mobile login tickets in the application memory cache.

**Methods:**

- `Create` — Caches a random login ticket for the Discord user for two minutes.

```csharp
public string Create(ulong discordUserId);
```

- `Consume` — Reads and removes a cached login ticket, returning its Discord user when present.

```csharp
public bool Consume(string ticket, out ulong discordUserId);
```

**Usage:**

The caller supplies the component and argument values used in this example.

```csharp
var result = authTicketStore.Consume(ticket, out var discordUserId);
```

---

### DiscordOAuthOptions

**File:** `API/Services/Auth/DiscordOAuthOptions.cs`

**Purpose:** Configures Discord OAuth credentials and the browser and mobile callback addresses.

**Properties:**

```csharp
// The Discord OAuth application identifier.
public string ClientId { get; set; }
public string ClientSecret { get; set; }
public string RedirectUri { get; set; }
public string AndroidRedirectUri { get; set; }
```

---

### DiscordOAuthService

**File:** `API/Services/Auth/DiscordOAuthService.cs`

**Purpose:** Uses a factory-managed HTTP client to exchange Discord OAuth codes and read user and guild data.

**Methods:**

- `CreateAuthorizeUrl` — Builds the Discord OAuth authorization URL with identify and guilds scopes and the supplied state.

```csharp
public string CreateAuthorizeUrl(string state);
```

- `GetCurrentUserAsync` — Reads the Discord user represented by the OAuth bearer token.

```csharp
public async Task<DiscordUserResponse> GetCurrentUserAsync(string accessToken, CancellationToken ct);
```

- `GetCurrentUserGuildsAsync` — Reads the guild memberships visible to the Discord OAuth bearer token.

```csharp
public async Task<IReadOnlyList<DiscordGuildResponse>> GetCurrentUserGuildsAsync(string accessToken, CancellationToken ct);
```

- `ExchangeCodeAsync` — Exchanges a Discord authorization code for OAuth tokens using the configured redirect URI.

```csharp
public async Task<DiscordTokenResponse> ExchangeCodeAsync(string code, CancellationToken ct);
```

**Usage:**

The caller supplies the component and argument values used in this example.

```csharp
var result = discordOAuthService.CreateAuthorizeUrl(state);
```

---

### OAuthStateStore

**File:** `API/Services/Auth/OAuthStateStore.cs`

**Purpose:** Stores short-lived OAuth state values for callback validation in the application memory cache.

**Methods:**

- `Create` — Caches a random OAuth callback state for five minutes.

```csharp
public string Create();
```

- `Consume` — Checks and removes a cached OAuth state when present.

```csharp
public bool Consume(string state);
```

**Usage:**

The caller supplies the component and argument values used in this example.

```csharp
var result = oAuthStateStore.Consume(state);
```

---

## Related Components

- `API/Services/README.md` — Parent feature overview and neighboring components.
- `API/README.md` — Project setup and architecture.
- `API/Services/Auth/Discord/README.md` — Detailed component contracts and examples for this subfolder.
- `API/Services/Auth/Discord/Response/` — Represents token, user, and guild data returned by Discord REST requests.
- `DC bot.Contracts/` — Shared storage and command protocol used by handlers and services.
