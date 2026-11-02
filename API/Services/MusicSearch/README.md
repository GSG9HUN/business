# MusicSearch

Validates provider queries and returns bounded track or playlist search results from Lavalink.

## Overview

- MusicSearchService checks deployment switches and cached source/plugin capabilities. LavalinkSearchClient applies authentication, response-size bounds, and caller-linked timeouts; the mapper validates metadata and provider URLs.
- MusicSearchRegistration owns options, the typed HTTP client, and rate-limit policies. Provider support depends on deployment configuration and the node; a listed provider is not a guarantee of availability.

## Classes

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
// The machine-readable error code returned to the API client.
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
// The base address of the Lavalink server used for music searches.
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
// Whether this search provider is enabled.
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

- `API/Services/README.md` — Parent feature overview and neighboring components.
- `API/README.md` — Project setup and architecture.
- `API/Responses/MusicSearch/` — Defines client-facing output for provider capabilities, search results, and safe provider error responses.
- `DC bot.Contracts/Music/Search/` — Defines canonical provider identities and validates supported provider content URLs.
- `DC bot.Contracts/` — Shared storage and command protocol used by handlers and services.
