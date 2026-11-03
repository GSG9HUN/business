# Configuration

Defines typed options for bot identity, Lavalink connectivity, search resolution, saved-playlist limits, and playback lifecycle deadlines.

## Overview

- Startup builds the runtime settings and registers options. PlaybackLifecycleOptions defines the join and cleanup deadlines used by the music lifecycle services.
- Keep deployment configuration out of commands. Provider credentials and source plugins are configured on the Lavalink side rather than by these DTOs alone.

## Classes

### BotSettings

**File:** `DC bot/Configuration/BotSettings.cs`

**Purpose:** Defines Discord authentication and the text-command prefix.

**Properties:**

```csharp
// The Discord bot authentication token.
public string? Token { get; init; }
public string Prefix { get; init; }
```

---

### LavalinkSettings

**File:** `DC bot/Configuration/LavalinkSettings.cs`

**Purpose:** Defines the Lavalink endpoint and authentication used at startup.

**Properties:**

```csharp
// The hostname of the Lavalink server.
public string? Hostname { get; init; }
public int Port { get; init; }
public bool Secured { get; init; }
public string Password { get; init; }
```

---

### PlaylistOptions

**File:** `DC bot/Configuration/PlaylistOptions.cs`

**Purpose:** Defines playlist and track limits applied to saved-playlist operations.

**Properties:**

```csharp
// The maximum number of saved playlists allowed per guild.
public int MaxPlaylistsPerGuild { get; set; }
public int MaxTracksPerPlaylist { get; set; }
public int MaxImportedTracks { get; set; }
```

---

### SearchResolverOptions

**File:** `DC bot/Configuration/SearchResolverOptions.cs`

**Purpose:** Defines the provider used when a query does not identify a source.

**Properties:**

```csharp
// The search mode used when the caller does not specify a provider.
public string DefaultQueryMode { get; set; }
```

---

### PlaybackLifecycleOptions

**File:** `DC bot/Configuration/PlaybackLifecycleOptions.cs`

**Purpose:** Defines deadlines for join, leave, cleanup, and pending-start reconciliation.

**Properties:**

```csharp
// The maximum time allowed for the join lifecycle operation.
public TimeSpan JoinDeadline { get; set; }
public TimeSpan LeaveDeadline { get; set; }
```


---

## Related Components

- `DC bot/README.md` — Parent feature overview and neighboring components.
- `DC bot/Startup/DependencyInjection/` — Registers the runtime implementations used by these components.
