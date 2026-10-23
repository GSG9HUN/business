# Endpoints

Registers minimal API routes, handler bindings, authorization, and endpoint filters.

## Overview

- Program.cs mounts these groups under /api. Route registration selects the handler; handlers own repository calls and response decisions.
- Keep route composition here and request processing in Handlers. Authentication alone does not replace a handler's guild-access check.

## Classes

### AuthEndpoints

**File:** `API/Endpoints/AuthEndpoints.cs`

**Purpose:** Registers the auth HTTP routes and their authorization and validation filters.

**Methods:**

- `MapAuthEndpoints` — Registers this feature's routes, handler bindings, authorization, and endpoint filters on the supplied route group.

```csharp
public static RouteGroupBuilder MapAuthEndpoints(this RouteGroupBuilder group);
```

**Usage:**

The caller supplies the component and argument values used in this example.

```csharp
var result = group.MapAuthEndpoints();
```

---

### CommandEndpoints

**File:** `API/Endpoints/CommandEndpoints.cs`

**Purpose:** Registers the command HTTP routes and their authorization and validation filters.

**Methods:**

- `MapCommandEndpoints` — Registers this feature's routes, handler bindings, authorization, and endpoint filters on the supplied route group.

```csharp
public static RouteGroupBuilder MapCommandEndpoints(this RouteGroupBuilder group);
```

**Usage:**

The caller supplies the component and argument values used in this example.

```csharp
var result = group.MapCommandEndpoints();
```

---

### GuildEndpoints

**File:** `API/Endpoints/GuildEndpoints.cs`

**Purpose:** Registers the guild HTTP routes and their authorization and validation filters.

**Methods:**

- `MapGuildEndpoints` — Registers this feature's routes, handler bindings, authorization, and endpoint filters on the supplied route group.

```csharp
public static RouteGroupBuilder MapGuildEndpoints(this RouteGroupBuilder group);
```

**Usage:**

The caller supplies the component and argument values used in this example.

```csharp
var result = group.MapGuildEndpoints();
```

---

### MusicSearchEndpoints

**File:** `API/Endpoints/MusicSearchEndpoints.cs`

**Purpose:** Registers the music search HTTP routes and their authorization and validation filters.

**Methods:**

- `MapMusicSearchEndpoints` — Registers this feature's routes, handler bindings, authorization, and endpoint filters on the supplied route group.

```csharp
public static RouteGroupBuilder MapMusicSearchEndpoints(this RouteGroupBuilder group);
```

**Usage:**

The caller supplies the component and argument values used in this example.

```csharp
var result = group.MapMusicSearchEndpoints();
```

---

### PlaybackEndpoints

**File:** `API/Endpoints/PlaybackEndpoints.cs`

**Purpose:** Registers the playback HTTP routes and their authorization and validation filters.

**Methods:**

- `MapPlaybackEndpoints` — Registers this feature's routes, handler bindings, authorization, and endpoint filters on the supplied route group.

```csharp
public static RouteGroupBuilder MapPlaybackEndpoints(this RouteGroupBuilder group);
```

**Usage:**

The caller supplies the component and argument values used in this example.

```csharp
var result = group.MapPlaybackEndpoints();
```

---

### PlayerEndpoints

**File:** `API/Endpoints/PlayerEndpoints.cs`

**Purpose:** Registers the player HTTP routes and their authorization and validation filters.

**Methods:**

- `MapPlayerEndpoints` — Registers this feature's routes, handler bindings, authorization, and endpoint filters on the supplied route group.

```csharp
public static RouteGroupBuilder MapPlayerEndpoints(this RouteGroupBuilder group);
```

**Usage:**

The caller supplies the component and argument values used in this example.

```csharp
var result = group.MapPlayerEndpoints();
```

---

### PlaylistEndpoints

**File:** `API/Endpoints/PlaylistEndpoints.cs`

**Purpose:** Registers the playlist HTTP routes and their authorization and validation filters.

**Methods:**

- `MapPlaylistEndpoints` — Registers this feature's routes, handler bindings, authorization, and endpoint filters on the supplied route group.

```csharp
public static RouteGroupBuilder MapPlaylistEndpoints(this RouteGroupBuilder group);
```

**Usage:**

The caller supplies the component and argument values used in this example.

```csharp
var result = group.MapPlaylistEndpoints();
```

---

### ProfileEndpoints

**File:** `API/Endpoints/ProfileEndpoints.cs`

**Purpose:** Registers the profile HTTP routes and their authorization and validation filters.

**Methods:**

- `MapProfileEndpoints` — Registers this feature's routes, handler bindings, authorization, and endpoint filters on the supplied route group.

```csharp
public static RouteGroupBuilder MapProfileEndpoints(this RouteGroupBuilder group);
```

**Usage:**

The caller supplies the component and argument values used in this example.

```csharp
var result = group.MapProfileEndpoints();
```

---

### QueueEndpoints

**File:** `API/Endpoints/QueueEndpoints.cs`

**Purpose:** Registers the queue HTTP routes and their authorization and validation filters.

**Methods:**

- `MapQueueEndpoints` — Registers this feature's routes, handler bindings, authorization, and endpoint filters on the supplied route group.

```csharp
public static RouteGroupBuilder MapQueueEndpoints(this RouteGroupBuilder group);
```

**Usage:**

The caller supplies the component and argument values used in this example.

```csharp
var result = group.MapQueueEndpoints();
```

---

### StatusEndpoints

**File:** `API/Endpoints/StatusEndpoints.cs`

**Purpose:** Registers the status HTTP routes and their authorization and validation filters.

**Methods:**

- `MapStatusEndpoints` — Registers this feature's routes, handler bindings, authorization, and endpoint filters on the supplied route group.

```csharp
public static RouteGroupBuilder MapStatusEndpoints(this RouteGroupBuilder group);
```

**Usage:**

The caller supplies the component and argument values used in this example.

```csharp
var result = group.MapStatusEndpoints();
```

---

## Related Components

- `API/README.md` — Parent feature overview and neighboring components.
- `API/Validation/` — Checks route identifiers, command aliases, and provider-aware enqueue input before handlers run.
- `API/Handlers/Auth/` — Coordinates Discord OAuth, mobile login-ticket exchange, refresh-token rotation, and logout.
- `API/Handlers/BotControl/` — Reads progress and results of durable bot-control commands.
- `API/Handlers/Guilds/` — Lists guilds visible to the authenticated mobile user and assembles their bot-presence metadata.
- `DC bot.Contracts/` — Shared storage and command protocol used by handlers and services.
