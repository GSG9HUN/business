# PlaylistService

Separates saved-playlist queries, metadata changes, track loading, and track mutations behind IPlaylistService.

## Overview

- Query helpers map stored tracks; mutation helpers validate limits and uniqueness; the loader resolves Lavalink content before persistence.
- Saved playlists are distinct from a live playback batch. Name validation is shared with the API, and owned rollback must still run when a request is cancelled.

## Classes

### PlaylistMutationService

**File:** `DC bot/Service/Music/PlaylistService/PlaylistMutationService.cs`

**Purpose:** Creates, deletes, and renames guild playlists while enforcing naming and ownership rules.

**Methods:**

- `CreatePlaylistAsync` — Creates a named empty playlist when its name and the guild's playlist limit permit it.

```csharp
internal async Task<CreatePlaylistResult> CreatePlaylistAsync(
    ulong guildId,
    string playlistName,
    CancellationToken cancellationToken = default);
```

- `DeletePlaylistAsync` — Deletes a guild-owned playlist after validating and normalizing its name.

```csharp
internal async Task<DeletePlaylistResult> DeletePlaylistAsync(
    ulong guildId,
    string playlistName,
    CancellationToken cancellationToken = default);
```

- `RenamePlaylistAsync` — Renames a guild-owned playlist after validating names and checking for conflicts.

```csharp
internal async Task<RenamePlaylistResult> RenamePlaylistAsync(
    ulong guildId,
    string currentName,
    string newName,
    CancellationToken cancellationToken = default);
```

**Usage:**

The caller supplies the component and argument values used in this example.

```csharp
var result = await playlistMutationService.CreatePlaylistAsync(guildId, playlistName, cancellationToken);
```

---

### PlaylistQueryService

**File:** `DC bot/Service/Music/PlaylistService/PlaylistQueryService.cs`

**Purpose:** Reads saved playlists and restores displayable tracks from persisted records.

**Methods:**

- `ListPlaylistsAsync` — Reads the guild's saved playlists and track counts for a list response.

```csharp
internal async Task<ListPlaylistsResult> ListPlaylistsAsync(ulong guildId, CancellationToken cancellationToken = default);
```

- `LoadPlaylistAsync` — Restores a saved playlist's tracks into a playback-ready load result.

```csharp
internal async Task<LoadPlaylistResult> LoadPlaylistAsync(
    ulong guildId,
    string playlistName,
    CancellationToken cancellationToken = default);
```

- `ViewPlaylistAsync` — Reads and restores display metadata for a named guild playlist.

```csharp
internal async Task<ViewPlaylistResult> ViewPlaylistAsync(
    ulong guildId,
    string playlistName,
    CancellationToken cancellationToken = default);
```

**Usage:**

The caller supplies the component and argument values used in this example.

```csharp
var result = await playlistQueryService.ListPlaylistsAsync(guildId, cancellationToken);
```

---

### PlaylistService

**File:** `DC bot/Service/Music/PlaylistService/PlaylistService.cs`

**Purpose:** Exposes saved-playlist queries and mutations through helpers with shared limits and logging.

**Implements / inherits:** `IPlaylistService`.

**Methods:**

- `CreatePlaylistAsync` — Creates a named empty playlist when its name and the guild's playlist limit permit it.

```csharp
public async Task<CreatePlaylistResult> CreatePlaylistAsync(
    ulong guildId,
    string playlistName,
    CancellationToken cancellationToken = default);
```

- `AddSongToPlaylistAsync` — Loads a source track and appends it when playlist naming and track limits permit it.

```csharp
public async Task<AddSongResult> AddSongToPlaylistAsync(
    ulong guildId,
    string playlistName,
    string songUrl,
    CancellationToken cancellationToken = default);
```

- `DeletePlaylistAsync` — Deletes a guild-owned playlist after validating and normalizing its name.

```csharp
public async Task<DeletePlaylistResult> DeletePlaylistAsync(
    ulong guildId,
    string playlistName,
    CancellationToken cancellationToken = default);
```

- `ListPlaylistsAsync` — Reads the guild's saved playlists and track counts for a list response.

```csharp
public async Task<ListPlaylistsResult> ListPlaylistsAsync(ulong guildId, CancellationToken cancellationToken = default);
```

- `LoadPlaylistAsync` — Restores a saved playlist's tracks into a playback-ready load result.

```csharp
public async Task<LoadPlaylistResult> LoadPlaylistAsync(
    ulong guildId,
    string playlistName,
    CancellationToken cancellationToken = default);
```

- `RemoveSongFromPlaylistAsync` — Removes a numbered track from a guild-owned saved playlist.

```csharp
public async Task<RemoveSongResult> RemoveSongFromPlaylistAsync(
    ulong guildId,
    string playlistName,
    int trackNumber,
    CancellationToken cancellationToken = default);
```

**Additional methods:** `SavePlaylistAsync`, `ViewPlaylistAsync`, `RenamePlaylistAsync`. See the source and its XML contracts for the complete parameter lists.

**Usage:**

The caller supplies the component and argument values used in this example.

```csharp
var result = await playlistService.AddSongToPlaylistAsync(guildId, playlistName, songUrl, cancellationToken);
```

---

### PlaylistTrackDisplayMapper

**File:** `DC bot/Service/Music/PlaylistService/PlaylistTrackDisplayMapper.cs`

**Purpose:** Maps stored tracks to display DTOs, logging entries whose encoded tracks cannot be restored.

**Methods:**

- `MapDisplayTracks` — Restores display metadata and logs stored tracks whose identifiers cannot be parsed.

```csharp
internal List<PlaylistViewTrackDto> MapDisplayTracks(
    IReadOnlyCollection<PlaylistTrackRecord> tracks,
    string playlistName,
    ulong guildId);
```

- `MapToDto` — Maps persisted playlist ordering and source identity into a response DTO.

```csharp
internal static PlaylistTrackDto MapToDto(PlaylistTrackRecord track);
```

**Usage:**

The caller supplies the component and argument values used in this example.

```csharp
var result = playlistTrackDisplayMapper.MapDisplayTracks(tracks, playlistName, guildId);
```

---

### PlaylistTrackLoader

**File:** `DC bot/Service/Music/PlaylistService/PlaylistTrackLoader.cs`

**Purpose:** Loads source tracks through Lavalink and translates source failures into playlist load errors.

**Methods:**

- `LoadTracksAsync` — Loads source tracks through Lavalink and translates source-loading failures to track-load errors.

```csharp
internal async Task<TrackLoadResult> LoadTracksAsync(
    string trackReference,
    TrackSearchMode searchMode,
    string operationName,
    CancellationToken cancellationToken = default);
```

**Usage:**

The caller supplies the component and argument values used in this example.

```csharp
var result = await playlistTrackLoader.LoadTracksAsync(trackReference, searchMode, operationName, cancellationToken);
```

---

### PlaylistTrackMutationService

**File:** `DC bot/Service/Music/PlaylistService/PlaylistTrackMutationService.cs`

**Purpose:** Loads and edits playlist tracks while enforcing configured playlist limits.

**Methods:**

- `AddSongToPlaylistAsync` — Loads a source track and appends it when playlist naming and track limits permit it.

```csharp
internal async Task<AddSongResult> AddSongToPlaylistAsync(
    ulong guildId,
    string playlistName,
    string songUrl,
    CancellationToken cancellationToken = default);
```

- `RemoveSongFromPlaylistAsync` — Removes a numbered track from a guild-owned saved playlist.

```csharp
internal async Task<RemoveSongResult> RemoveSongFromPlaylistAsync(
    ulong guildId,
    string playlistName,
    int trackNumber,
    CancellationToken cancellationToken = default);
```

- `SavePlaylistAsync` — Loads source tracks into a saved playlist while enforcing names, ownership, and configured limits.

```csharp
internal async Task<SavePlaylistResult> SavePlaylistAsync(
    ulong guildId,
    string playlistName,
    string playlistUrl,
    CancellationToken cancellationToken = default);
```

**Usage:**

The caller supplies the component and argument values used in this example.

```csharp
var result = await playlistTrackMutationService.AddSongToPlaylistAsync(guildId, playlistName, songUrl, cancellationToken);
```

---

## Related Components

- `DC bot/Service/Music/README.md` — Parent feature overview and neighboring components.
- `DC bot/README.md` — Project setup and architecture.
- `DC bot/Configuration/` — Defines typed options for bot identity, Lavalink connectivity, search resolution, and saved-playlist limits.
- `DC bot/Logging/` — Defines typed log messages and reusable scopes for bot operations.
- `DC bot/Exceptions/Music/` — Distinguishes Lavalink failures, track-load failures, expired sessions, and busy control operations.
- `DC bot/Wrapper/Lavalink/` — Exposes Lavalink track metadata through ILavaLinkTrack.
- `DC bot/Startup/DependencyInjection/` — Registers the runtime implementations used by these components.
