# Search

Defines canonical provider identities and validates supported provider content URLs.

## Overview

- MusicSearchProviders supplies shared lookup and URL rules so bot and API input handling agree. MusicSearchProvider carries provider metadata.
- Do not treat an arbitrary URL host or search prefix as an enabled deployment. Search and enqueue rollout are separate API options.

## Classes

### MusicSearchProviders

**File:** `DC bot.Contracts/Music/Search/MusicSearchProviders.cs`

**Purpose:** Defines shared provider prefixes and validates concrete content URLs without depending on a playback SDK.

**Properties:**

```csharp
// The complete set of supported music search provider identifiers.
public static IReadOnlyList<MusicSearchProvider> All { get; }
```

**Methods:**

- `Find` — Finds a supported music provider by its case-insensitive public prefix.

```csharp
public static MusicSearchProvider? Find(string? id);
```

- `IsProviderUrl` — Accepts only supported HTTP(S) provider content routes with allowed hosts, default ports, and no user credentials.

```csharp
public static bool IsProviderUrl(string providerId, string? value);
```

**Usage:**

The caller supplies the component and argument values used in this example.

```csharp
var result = MusicSearchProviders.Find(id);
```

---

## Models and Results

### MusicSearchProvider

**File:** `DC bot.Contracts/Music/Search/MusicSearchProvider.cs`

**Purpose:** Describes a shared search prefix, source name, and provider capability without binding callers to Lavalink.

**Record declaration:**

```csharp
public sealed record MusicSearchProvider(string Id, string Prefix, string SourceName, bool SupportsPlaylistSearch);
```

---

## Related Components

- `DC bot.Contracts/Music/README.md` — Parent feature overview and neighboring components.
- `DC bot.Contracts/README.md` — Project setup and architecture.
- `DC bot.Persistence/` — Repository implementations and database mappings that consume these shared contracts.
