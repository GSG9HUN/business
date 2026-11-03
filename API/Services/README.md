# Services

Groups API-owned authentication and music-search behavior that is larger than a route declaration.

## Overview

- Auth services manage OAuth and token creation; music-search services validate provider queries, inspect node capabilities, and map results.
- Services use typed options, factory-managed HTTP clients, and shared persistence contracts where needed. Discord playback execution stays in the bot process.

## Subfolders

### Auth/

Builds Discord OAuth requests and manages mobile access tokens, refresh tokens, cached callback state, and login tickets.

**Files:**

- `AppAuthOptions.cs`
- `AppTokenService.cs`
- `AuthTicketStore.cs`
- `DiscordOAuthOptions.cs`
- `DiscordOAuthService.cs`
- `OAuthStateStore.cs`

**Purpose:** DiscordOAuthService uses a factory-managed HTTP client and DiscordOAuthOptions. AppTokenService signs 15-minute access tokens using AppAuthOptions; refresh sessions persist hashes instead of raw refresh tokens.

---

### MusicSearch/

Validates provider queries and returns bounded track or playlist search results from Lavalink.

**Files:**

- `IMusicSearchService.cs`
- `LavalinkSearchClient.cs`
- `MusicSearchException.cs`
- `MusicSearchOptions.cs`
- `MusicSearchProviderOptions.cs`
- `MusicSearchQuota.cs`
- `MusicSearchRegistration.cs`
- `MusicSearchResultMapper.cs`
- `MusicSearchService.cs`
- `NodeInfo.cs`

**Purpose:** MusicSearchService checks deployment switches and cached source/plugin capabilities. LavalinkSearchClient applies authentication, response-size bounds, and caller-linked timeouts; the mapper validates metadata and provider URLs.

---

## Classes

### AppAuthOptions

**File:** `API/Services/Auth/AppAuthOptions.cs`

**Purpose:** Configures the signing key, issuer, and audience of mobile application access tokens.

**Properties:**

```csharp
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

### LavalinkSearchClient

**File:** `API/Services/MusicSearch/LavalinkSearchClient.cs`

**Purpose:** Sends authenticated, size-limited Lavalink search requests with a caller-linked timeout.

**Methods:**

- `GetAsync` — Requests a bounded Lavalink JSON response and maps upstream failures while preserving caller cancellation.

```csharp
public async Task<JsonDocument?> GetAsync(string relativePath, CancellationToken cancellationToken);
```

**Usage:**

The caller supplies the component and argument values used in this example.

```csharp
var result = await lavalinkSearchClient.GetAsync(relativePath, cancellationToken);
```

---

### MusicSearchException

**File:** `API/Services/MusicSearch/MusicSearchException.cs`

**Purpose:** Describes a safe search error code, HTTP status, and optional retry delay.

**Implements / inherits:** `Exception`.

**Properties:**

```csharp
public string Code { get; }
public int StatusCode { get; }
public int? RetryAfterSeconds { get; }
```

---

### MusicSearchOptions

**File:** `API/Services/MusicSearch/MusicSearchOptions.cs`

**Purpose:** Configures the Lavalink search endpoint, timeout, result limit, and provider rollout settings.

**Properties:**

```csharp
public string BaseAddress { get; set; }
public string Password { get; set; }
public int TimeoutSeconds { get; set; }
public int MaxResults { get; set; }
public Dictionary<string, MusicSearchProviderOptions> Providers { get; set; }
```

---

### MusicSearchProviderOptions

**File:** `API/Services/MusicSearch/MusicSearchProviderOptions.cs`

**Purpose:** Controls search and enqueue availability for an individual deployed music provider.

**Properties:**

```csharp
public bool Enabled { get; set; }
public bool PlaylistSearch { get; set; }
public bool CanEnqueue { get; set; }
public string? UnavailableReason { get; set; }
```

---

### MusicSearchQuota

**File:** `API/Services/MusicSearch/MusicSearchQuota.cs`

**Purpose:** Owns per-provider fixed-window rate limiters for upstream search requests.

**Implements / inherits:** `IDisposable`.

**Methods:**

- `Acquire` — Acquires one provider request permit or throws a rate-limit error with a retry delay.

```csharp
public void Acquire(string provider);
```

- `Dispose` — Releases the owned provider rate-limiter partitions.

```csharp
public void Dispose();
```

**Usage:**

The caller supplies the component and argument values used in this example.

```csharp
musicSearchQuota.Acquire(provider);
```

---

### MusicSearchRegistration

**File:** `API/Services/MusicSearch/MusicSearchRegistration.cs`

**Purpose:** Registers typed search configuration, the HTTP client, provider quota, and endpoint rate limiting.

**Methods:**

- `AddMusicSearch` — Registers search options, factory-managed HTTP access, provider quotas, and the authenticated rate-limit policy.

```csharp
public static IServiceCollection AddMusicSearch(this IServiceCollection services, IConfiguration configuration);
```

**Usage:**

The caller supplies the component and argument values used in this example.

```csharp
var result = services.AddMusicSearch(configuration);
```

---

### MusicSearchResultMapper

**File:** `API/Services/MusicSearch/MusicSearchResultMapper.cs`

**Purpose:** Validates Lavalink search payloads and maps provider-safe URLs and metadata to music results.

**Methods:**

- `Map` — Validates provider track or playlist results and returns deduplicated, bounded search items.

```csharp
public static MusicSearchResponse Map(
    JsonElement root,
    MusicSearchProvider provider,
    string kind,
    bool canEnqueue,
    int maxResults);
```

- `Property` — Reads a JSON object property or returns an undefined JSON value when it is absent.

```csharp
internal static JsonElement Property(JsonElement element, string name);
```

**Usage:**

The caller supplies the component and argument values used in this example.

```csharp
var result = MusicSearchResultMapper.Map(root, provider, kind, canEnqueue, maxResults);
```

---

### MusicSearchService

**File:** `API/Services/MusicSearch/MusicSearchService.cs`

**Purpose:** Validates music queries, discovers enabled node capabilities, and requests bounded provider results.

**Implements / inherits:** `IMusicSearchService`.

**Methods:**

- `GetCapabilitiesAsync` — Returns deployed provider search and enqueue capabilities based on the Lavalink node configuration.

```csharp
public async Task<IReadOnlyList<MusicSearchCapabilityResponse>> GetCapabilitiesAsync(CancellationToken cancellationToken);
```

- `SearchAsync` — Validates the music query and requests matching results from an enabled provider.

```csharp
public async Task<MusicSearchResponse> SearchAsync(
    string? provider,
    string? kind,
    string? query,
    string? pageToken,
    CancellationToken cancellationToken);
```

**Usage:**

The caller supplies the component and argument values used in this example.

```csharp
var result = await musicSearchService.GetCapabilitiesAsync(cancellationToken);
```

---

## Interfaces

### IMusicSearchService

**File:** `API/Services/MusicSearch/IMusicSearchService.cs`

**Purpose:** Defines provider capability discovery and validated track or playlist searches.

**Methods:**

- `GetCapabilitiesAsync` — Returns deployed provider search and enqueue capabilities based on the Lavalink node configuration.

```csharp
Task<IReadOnlyList<MusicSearchCapabilityResponse>> GetCapabilitiesAsync(CancellationToken cancellationToken);
```

- `SearchAsync` — Validates the music query and requests matching results from an enabled provider.

```csharp
Task<MusicSearchResponse> SearchAsync(
    string? provider,
    string? kind,
    string? query,
    string? pageToken,
    CancellationToken cancellationToken);
```

**Usage:**

The caller supplies the component and argument values used in this example.

```csharp
var result = await musicSearchService.GetCapabilitiesAsync(cancellationToken);
```

---

## Models and Results

### NodeInfo

**File:** `API/Services/MusicSearch/NodeInfo.cs`

**Purpose:** Caches the Lavalink source-manager names and availability of the LavaSearch plugin.

**Record declaration:**

```csharp
internal sealed record NodeInfo(HashSet<string> Sources, bool HasLavaSearch);
```

---

## Related Components

- `API/README.md` — Parent feature overview and neighboring components.
- `API/Services/Auth/README.md` — Detailed component contracts and examples for this subfolder.
- `API/Services/MusicSearch/README.md` — Detailed component contracts and examples for this subfolder.
- `DC bot.Contracts/` — Shared storage and command protocol used by handlers and services.
