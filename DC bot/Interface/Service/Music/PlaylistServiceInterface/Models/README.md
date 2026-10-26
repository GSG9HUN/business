# Models

Carries inputs, statuses, and results for the playlist service use cases.

## Overview

- Implementations live under Service/Music/PlaylistService; commands and orchestrators exchange these contracts instead of constructing transport or persistence dependencies themselves.
- Result states distinguish completed, rejected, pending, and superseded work where applicable. Preserve ownership and cancellation semantics when adding an operation.

## Models and Results

### AddSongResult

**File:** `DC bot/Interface/Service/Music/PlaylistServiceInterface/Models/AddSongResult.cs`

**Purpose:** Defines the typed result and status values returned by the add song use case, including expected rejection outcomes.

**Values:**

```csharp
public enum AddSongResult
{
    // The song was added to the saved playlist.
    Added,
    // The requested saved playlist does not exist.
    PlaylistDoesNotExist,
    // No playable tracks were resolved from the supplied input.
    NoTracksFound,
    // The supplied song URL is invalid.
    InvalidSongUrl,
    // The supplied playlist name is invalid.
    InvalidPlaylistName,
    // The playlist cannot accept more tracks because its track limit was reached.
    TrackLimitReached,
    // The operation failed for an unclassified reason.
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
    // The playlist was created.
    Created,
    // A playlist with the requested name already exists.
    PlaylistAlreadyExists,
    // The supplied playlist name is invalid.
    InvalidPlaylistName,
    // The guild cannot create another playlist because its playlist limit was reached.
    PlaylistLimitReached,
    // The operation failed for an unclassified reason.
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
    // The saved playlist was deleted.
    Deleted,
    // The requested saved playlist does not exist.
    DoesNotExist,
    // The supplied playlist name is invalid.
    InvalidPlaylistName,
    // The operation failed for an unclassified reason.
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
    // The requested track was removed from the saved playlist.
    Removed,
    // The requested saved playlist does not exist.
    PlaylistDoesNotExist,
    // The requested song was not found in the saved playlist.
    SongNotFound,
    // The supplied playlist name is invalid.
    InvalidPlaylistName,
    // The supplied playlist track index is invalid.
    InvalidTrackNumber,
    // The operation failed for an unclassified reason.
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
    // The saved playlist was renamed.
    Renamed,
    // The requested saved playlist does not exist.
    PlaylistDoesNotExist,
    // A playlist with the requested name already exists.
    PlaylistAlreadyExists,
    // The supplied playlist name is invalid.
    InvalidPlaylistName,
    // The operation failed for an unclassified reason.
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
    // The playlist was saved.
    Saved,
    // A saved playlist with this name already exists.
    AlreadyExists,
    // No playable tracks were resolved from the supplied input.
    NoTracksFound,
    // The supplied playlist name is invalid.
    InvalidPlaylistName,
    // The guild cannot create another playlist because its playlist limit was reached.
    PlaylistLimitReached,
    // The supplied tracks exceed the saved playlist track limit.
    TrackLimitExceeded,
    // The operation failed for an unclassified reason.
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

- `DC bot/Interface/Service/Music/PlaylistServiceInterface/README.md` — Parent feature overview and neighboring components.
- `DC bot/README.md` — Project setup and architecture.
- `DC bot/Startup/DependencyInjection/` — Registers the runtime implementations used by these components.
