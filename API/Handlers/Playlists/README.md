# Playlists

Provides authorized playlist reads, creation, rename, deletion, and track operations.

## Overview

- Metadata operations use playlist repositories. Track resolution, URL imports, and loading into playback are queued as durable bot commands.
- Names use the shared validator. Uniqueness conflicts become HTTP Conflict responses; repository entities do not cross the API response boundary.

**Custom Covers**

`PATCH /api/playlists/{playlistId}/artwork` accepts `{ "artworkUri": "https://example.com/cover.jpg" }`. Pass `{ "artworkUri": null }` to restore automatic artwork. The handler requires guild access, trims URLs, and accepts only absolute HTTP/HTTPS URLs without embedded credentials, up to 2048 characters. Responses: 204 on success, 400 for invalid input, 401/403 for unavailable access, and 404 for an unknown playlist. The API stores the URL without fetching the image.

**Playlist Command Execution**

CreateAsync validates the shared PlaylistOptions limit before creating a playlist. A full guild receives 409 with errorCode PlaylistLimitReached. AddTrackAsync, ImportAsync, and LoadAsync enqueue supported worker commands; 202 confirms admission, and the persisted command result reports execution success or failure.

**Queue Snapshot Playlist Saving**

QueuePlaylistHandlers validates the captured ordered occurrence IDs and URL pairs within the authorized guild. Every ID must exist and its decoded source URL must match. The handler preserves duplicates and request order, validates configured limits and storage lengths, then calls CreateSnapshotAsync. Failures never leave an empty or partially populated playlist. Standard empty playlist creation also uses the atomic repository operation for its final limit check.

**Staging Integration**

Conflict resolution preserves one list entry per playlist and one atomic create path. Queue snapshot saving and custom artwork coexist with staging authentication and payload contracts.

## Classes

### PlaylistHandlers
Handles playlist create, list, view, rename, delete, save, and track mutation requests.

List and detail responses prefer the saved custom `artworkUri`, falling back to the first decodable track with artwork in playlist order. Individual detail tracks also expose their own nullable `artworkUri`. Without a custom cover, playlists without track artwork return null; the client renders default artwork. The list handler uses its existing batched track query to select covers.

`GET /api/playlists/{playlistId}` returns a complete playlist resource, including its tracks. The plural `playlists` names the collection; the ID identifies one resource in that collection. The route is unchanged.

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
