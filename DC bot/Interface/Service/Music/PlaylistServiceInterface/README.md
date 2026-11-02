# PlaylistServiceInterface

Defines the playlist service boundary used by the bot music pipeline.

## Overview

- Implementations live under Service/Music/PlaylistService; commands and orchestrators exchange these contracts instead of constructing transport or persistence dependencies themselves.
- Result states distinguish completed, rejected, pending, and superseded work where applicable. Preserve ownership and cancellation semantics when adding an operation.

## Subfolders

### Models/

Carries inputs, statuses, and results for the playlist service use cases.

**Files:**

- `AddSongResult.cs`
- `CreatePlaylistResult.cs`
- `DeletePlaylistResult.cs`
- `ListPlaylistsResult.cs`
- `LoadPlaylistResult.cs`
- `PlaylistDto.cs`
- `PlaylistSummaryDto.cs`
- `PlaylistTrackDto.cs`
- `PlaylistViewTrackDto.cs`
- `RemoveSongResult.cs`
- `RenamePlaylistResult.cs`
- `SavePlaylistResult.cs`
- `ViewPlaylistResult.cs`

**Purpose:** Implementations live under Service/Music/PlaylistService; commands and orchestrators exchange these contracts instead of constructing transport or persistence dependencies themselves.

---

## Interfaces

### IPlaylistService

**File:** `DC bot/Interface/Service/Music/PlaylistServiceInterface/IPlaylistService.cs`

**Purpose:** Defines the playlist service boundary so callers are independent of its concrete implementation.

**Methods:**

- `CreatePlaylistAsync` — Creates a named empty playlist when its name and the guild's playlist limit permit it.

```csharp
Task<CreatePlaylistResult> CreatePlaylistAsync(ulong guildId, string playlistName, CancellationToken cancellationToken = default);
```

- `AddSongToPlaylistAsync` — Loads a source track and appends it when playlist naming and track limits permit it.

```csharp
Task<AddSongResult> AddSongToPlaylistAsync(
    ulong guildId,
    string playlistName,
    string songUrl,
    CancellationToken cancellationToken = default);
```

- `DeletePlaylistAsync` — Deletes a guild-owned playlist after validating and normalizing its name.

```csharp
Task<DeletePlaylistResult> DeletePlaylistAsync(ulong guildId, string playlistName, CancellationToken cancellationToken = default);
```

- `ListPlaylistsAsync` — Reads the guild's saved playlists and track counts for a list response.

```csharp
Task<ListPlaylistsResult> ListPlaylistsAsync(ulong guildId, CancellationToken cancellationToken = default);
```

- `LoadPlaylistAsync` — Restores a saved playlist's tracks into a playback-ready load result.

```csharp
Task<LoadPlaylistResult> LoadPlaylistAsync(ulong guildId, string playlistName, CancellationToken cancellationToken = default);
```

- `RemoveSongFromPlaylistAsync` — Removes a numbered track from a guild-owned saved playlist.

```csharp
Task<RemoveSongResult> RemoveSongFromPlaylistAsync(
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

## Models and Results

### AddSongResult

**File:** `DC bot/Interface/Service/Music/PlaylistServiceInterface/Models/AddSongResult.cs`

**Purpose:** Defines the typed result and status values returned by the add song use case, including expected rejection outcomes.

**Values:**

```csharp
public enum AddSongResult
{
    Added,
    PlaylistDoesNotExist,
    NoTracksFound,
    InvalidSongUrl,
    InvalidPlaylistName,
    TrackLimitReached,
    UnknownError
}
```

---

### CreatePlaylistResult

**File:** `DC bot/Interface/Service/Music/PlaylistServiceInterface/Models/CreatePlaylistResult.cs`

**Purpose:** Defines the typed result and status values returned by the create playlist use case, including expected rejection outcomes.

**Values:**

```csharp
public enum CreatePlaylistResult
{
    Created,
    PlaylistAlreadyExists,
    InvalidPlaylistName,
    PlaylistLimitReached,
    UnknownError
}
```

---

### DeletePlaylistResult

**File:** `DC bot/Interface/Service/Music/PlaylistServiceInterface/Models/DeletePlaylistResult.cs`

**Purpose:** Defines the typed result and status values returned by the delete playlist use case, including expected rejection outcomes.

**Values:**

```csharp
public enum DeletePlaylistResult
{
    Deleted,
    DoesNotExist,
    InvalidPlaylistName,
    UnknownError
}
```

---

### ListPlaylistsResult

**File:** `DC bot/Interface/Service/Music/PlaylistServiceInterface/Models/ListPlaylistsResult.cs`

**Purpose:** Carries outcome and response data for list playlists operations.

**Record declaration:**

```csharp
public sealed record ListPlaylistsResult(ListPlaylistsStatus Status, IReadOnlyList<PlaylistSummaryDto> Playlists);
```

**Related enum:**

```csharp
public enum ListPlaylistsStatus
{
    Listed,
    NoPlaylists,
    UnknownError
}
```

---

### LoadPlaylistResult

**File:** `DC bot/Interface/Service/Music/PlaylistServiceInterface/Models/LoadPlaylistResult.cs`

**Purpose:** Carries outcome and response data for load playlist operations.

**Record declaration:**

```csharp
public sealed record LoadPlaylistResult(LoadPlaylistStatus Status, PlaylistDto? Playlist);
```

**Related enum:**

```csharp
public enum LoadPlaylistStatus
{
    Loaded,
    NotFound,
    EmptyPlaylist,
    InvalidPlaylistName,
    UnknownError
}
```

---

### PlaylistDto

**File:** `DC bot/Interface/Service/Music/PlaylistServiceInterface/Models/PlaylistDto.cs`

**Purpose:** Carries playlist values across the playlist presentation boundary.

**Record declaration:**

```csharp
public sealed record PlaylistDto(string Name, IReadOnlyList<PlaylistTrackDto> Tracks);
```

---

### PlaylistSummaryDto

**File:** `DC bot/Interface/Service/Music/PlaylistServiceInterface/Models/PlaylistSummaryDto.cs`

**Purpose:** Contains a saved playlist's name and track count for list responses.

**Record declaration:**

```csharp
public sealed record PlaylistSummaryDto(string Name, int TrackCount);
```

---

### PlaylistTrackDto

**File:** `DC bot/Interface/Service/Music/PlaylistServiceInterface/Models/PlaylistTrackDto.cs`

**Purpose:** Carries playlist track values across the playlist presentation boundary.

**Record declaration:**

```csharp
public sealed record PlaylistTrackDto(int OrderNumber, string Source, string TrackIdentifier, string TrackUri);
```

---

### PlaylistViewTrackDto

**File:** `DC bot/Interface/Service/Music/PlaylistServiceInterface/Models/PlaylistViewTrackDto.cs`

**Purpose:** Contains a saved track's display metadata and playlist position.

**Record declaration:**

```csharp
public sealed record PlaylistViewTrackDto(int OrderNumber, string Title, string Author, TimeSpan Duration, string TrackUri);
```

---

### RemoveSongResult

**File:** `DC bot/Interface/Service/Music/PlaylistServiceInterface/Models/RemoveSongResult.cs`

**Purpose:** Defines the typed result and status values returned by the remove song use case, including expected rejection outcomes.

**Values:**

```csharp
public enum RemoveSongResult
{
    Removed,
    PlaylistDoesNotExist,
    SongNotFound,
    InvalidPlaylistName,
    InvalidTrackNumber,
    UnknownError
}
```

---

### RenamePlaylistResult

**File:** `DC bot/Interface/Service/Music/PlaylistServiceInterface/Models/RenamePlaylistResult.cs`

**Purpose:** Defines the typed result and status values returned by the rename playlist use case, including expected rejection outcomes.

**Values:**

```csharp
public enum RenamePlaylistResult
{
    Renamed,
    PlaylistDoesNotExist,
    PlaylistAlreadyExists,
    InvalidPlaylistName,
    UnknownError
}
```

---

### SavePlaylistResult

**File:** `DC bot/Interface/Service/Music/PlaylistServiceInterface/Models/SavePlaylistResult.cs`

**Purpose:** Defines the typed result and status values returned by the save playlist use case, including expected rejection outcomes.

**Values:**

```csharp
public enum SavePlaylistResult
{
    Saved,
    AlreadyExists,
    NoTracksFound,
    InvalidPlaylistName,
    PlaylistLimitReached,
    TrackLimitExceeded,
    UnknownError
}
```

---

### ViewPlaylistResult

**File:** `DC bot/Interface/Service/Music/PlaylistServiceInterface/Models/ViewPlaylistResult.cs`

**Purpose:** Carries outcome and response data for view playlist operations.

**Record declaration:**

```csharp
public sealed record ViewPlaylistResult(
    ViewPlaylistStatus Status,
    string PlaylistName,
    IReadOnlyList<PlaylistViewTrackDto> Tracks);
```

**Related enum:**

```csharp
public enum ViewPlaylistStatus
{
    Viewed,
    PlaylistDoesNotExist,
    EmptyPlaylist,
    InvalidPlaylistName,
    UnknownError
}
```

---

## Related Components

- `DC bot/Interface/Service/Music/README.md` — Parent feature overview and neighboring components.
- `DC bot/README.md` — Project setup and architecture.
- `DC bot/Interface/Service/Music/PlaylistServiceInterface/Models/README.md` — Detailed component contracts and examples for this subfolder.
- `DC bot/Interface/Service/Music/PlaylistServiceInterface/Models/` — Carries inputs, statuses, and results for the playlist service use cases.
- `DC bot/Startup/DependencyInjection/` — Registers the runtime implementations used by these components.
