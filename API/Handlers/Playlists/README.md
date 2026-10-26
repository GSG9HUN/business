# Playlists

Provides authorized playlist reads, creation, rename, deletion, and track operations.

## Overview

- Metadata operations use playlist repositories. Track resolution, URL imports, and loading into playback are queued as durable bot commands.
- Names use the shared validator. Uniqueness conflicts become HTTP Conflict responses; repository entities do not cross the API response boundary.

## Classes

### PlaylistHandlers

**File:** `API/Handlers/Playlists/PlaylistHandlers.cs`

**Purpose:** Authorizes playlist reads and edits and queues bot commands for track loading and imports.

**Methods:**

- `CreateAsync` — Normalizes a playlist name and creates an empty guild playlist, returning conflicts for duplicates.

```csharp
public static async Task<IResult> CreateAsync(
    HttpContext httpContext,
    CreatePlaylistRequest request,
    IMobileAppUserRepository userRepository,
    IPlaylistRepository playlistRepository,
    CancellationToken cancellationToken);
```

- `GetAsync` — Returns the authorized playlist's ordered, decodable track details.

```csharp
public static async Task<IResult> GetAsync(
    long playlistId,
    HttpContext httpContext,
    IMobileAppUserRepository userRepository,
    IPlaylistRepository playlistRepository,
    IPlaylistTrackRepository playlistTrackRepository,
    CancellationToken cancellationToken);
```

- `AddTrackAsync` — Queues a durable bot command to resolve and add a supplied track URL to an authorized playlist.

```csharp
public static async Task<IResult> AddTrackAsync(
    long playlistId,
    AddSongRequest request,
    HttpContext httpContext,
    IMobileAppUserRepository userRepository,
    IPlaylistRepository playlistRepository,
    IBotControlCommandsRepository commandsRepository,
    CancellationToken cancellationToken);
```

- `DeleteAsync` — Deletes a playlist only after confirming guild access.

```csharp
public static async Task<IResult> DeleteAsync(
    long playlistId,
    HttpContext httpContext,
    IMobileAppUserRepository userRepository,
    IPlaylistRepository playlistRepository,
    CancellationToken cancellationToken);
```

- `ImportAsync` — Validates a playlist import request and queues a durable bot command to resolve its URL.

```csharp
public static async Task<IResult> ImportAsync(
    HttpContext httpContext,
    SavePlaylistRequest request,
    IMobileAppUserRepository userRepository,
    IBotControlCommandsRepository commandsRepository,
    CancellationToken cancellationToken);
```

- `ListAsync` — Lists authorized guild playlists with track counts and decoded total durations.

```csharp
public static async Task<IResult> ListAsync(
    HttpContext httpContext,
    IMobileAppUserRepository userRepository,
    IPlaylistRepository playlistRepository,
    IPlaylistTrackRepository playlistTrackRepository,
    CancellationToken cancellationToken);
```

**Additional methods:** `RenameAsync`, `RemoveTrackAsync`, `LoadAsync`. See the source and its XML contracts for the complete parameter lists.

**Usage:**

The caller supplies the component and argument values used in this example.

```csharp
var result = await PlaylistHandlers.CreateAsync(httpContext, request, userRepository, playlistRepository, cancellationToken);
```

---

## Related Components

- `API/Handlers/README.md` — Parent feature overview and neighboring components.
- `API/README.md` — Project setup and architecture.
- `API/Mapping/` — Converts repository records and service outcomes into HTTP response contracts.
- `API/Requests/Playlists/` — Defines client-supplied input for saved playlist metadata and ordered, serialized playlist tracks.
- `API/Responses/Playlists/` — Defines client-facing output for saved playlist metadata and ordered, serialized playlist tracks.
- `DC bot.Contracts/BotControl/` — Defines command type identifiers, realtime event names, and optional execution-channel context shared by the API and bot.
- `DC bot.Contracts/` — Shared storage and command protocol used by handlers and services.
