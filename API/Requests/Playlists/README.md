# Playlists

Defines client-supplied input for saved playlist metadata and ordered, serialized playlist tracks.

## Overview

- Minimal API binding constructs these values; feature filters and handlers normalize and validate them before repository or service calls.
- Optional input can mean leave unchanged rather than reset. Request fields are not authorization evidence; the authenticated principal and stored guild access determine permission.
- CreatePlaylistRequest accepts PlaylistName with a legacy Name fallback. Name normalization and duplicate handling occur in the handler and repository boundaries rather than in the DTO constructors.

## Classes

### AddSongRequest

**File:** `API/Requests/Playlists/AddSongRequest.cs`

**Purpose:** Carries client input for add song processing; handlers and endpoint filters validate its values.

**Properties:**

```csharp
// The URL of the song to add to the playlist.
public string SongUrl { get; init; }
```

**Methods:**

- `Deconstruct` — Exposes the request values through tuple-style deconstruction without validation or normalization.

```csharp
public void Deconstruct(out string songUrl);
```

**Usage:**

The caller supplies the component and argument values used in this example.

```csharp
addSongRequest.Deconstruct(out var songUrl);
```

---

### CreatePlaylistRequest

**File:** `API/Requests/Playlists/CreatePlaylistRequest.cs`

**Purpose:** Carries client input for create playlist processing; handlers and endpoint filters validate its values.

**Properties:**

```csharp
// The legacy playlist name used when PlaylistName is absent or blank.
public string? Name { get; init; }
public string? PlaylistName { get; init; }
```

**Methods:**

- `GetPlaylistName` — Returns the explicitly supplied playlist name, falling back to the legacy name field.

```csharp
public string? GetPlaylistName();
```

**Usage:**

The caller supplies the component and argument values used in this example.

```csharp
var result = createPlaylistRequest.GetPlaylistName();
```

---

### RenamePlaylistRequest

**File:** `API/Requests/Playlists/RenamePlaylistRequest.cs`

**Purpose:** Carries client input for rename playlist processing; handlers and endpoint filters validate its values.

**Properties:**

```csharp
// The new name requested for the saved playlist.
public string NewName { get; init; }
```

**Methods:**

- `Deconstruct` — Exposes the request values through tuple-style deconstruction without validation or normalization.

```csharp
public void Deconstruct(out string newName);
```

**Usage:**

The caller supplies the component and argument values used in this example.

```csharp
renamePlaylistRequest.Deconstruct(out var newName);
```

---

### SavePlaylistRequest

**File:** `API/Requests/Playlists/SavePlaylistRequest.cs`

**Purpose:** Carries client input for save playlist processing; handlers and endpoint filters validate its values.

**Properties:**

```csharp
// The saved playlist name.
public string Name { get; init; }
public string Url { get; init; }
```

**Methods:**

- `Deconstruct` — Exposes the request values through tuple-style deconstruction without validation or normalization.

```csharp
public void Deconstruct(out string name, out string url);
```

**Usage:**

The caller supplies the component and argument values used in this example.

```csharp
savePlaylistRequest.Deconstruct(out var name, out var url);
```

---

## Related Components

- `API/Requests/README.md` — Parent feature overview and neighboring components.
- `API/README.md` — Project setup and architecture.
- `DC bot.Contracts/` — Shared storage and command protocol used by handlers and services.
