# Playlists

Defines client-facing output for saved playlist metadata and ordered, serialized playlist tracks.

## Overview

- Handlers, mappers, and snapshot providers construct these records from shared contract values and return them through HTTP or matching realtime events.
- Keep field meaning consistent for mobile consumers. An output model describes observable state and must not contain a tracked entity or a live SDK object.

## Models and Results

### PlaylistDetailResponse

**File:** `API/Responses/Playlists/PlaylistDetailResponse.cs`

**Purpose:** Carries the playlist detail payload returned to HTTP and mobile clients.

**Record declaration:**

```csharp
public sealed record PlaylistDetailResponse(
    string Id,
    string GuildId,
    string Name,
    int SongCount,
    int Duration,
    IReadOnlyList<PlaylistTrackResponse> Tracks);
```

---

### PlaylistSummaryResponse

**File:** `API/Responses/Playlists/PlaylistSummaryResponse.cs`

**Purpose:** Carries the playlist summary payload returned to HTTP and mobile clients.

**Record declaration:**

```csharp
public sealed record PlaylistSummaryResponse(string Id, string Name, int SongCount, int Duration);
```

---

### PlaylistTrackResponse

**File:** `API/Responses/Playlists/PlaylistTrackResponse.cs`

**Purpose:** Carries the playlist track payload returned to HTTP and mobile clients.

**Record declaration:**

```csharp
public sealed record PlaylistTrackResponse(int OrderNumber, string Title, string Author, int Duration, string TrackUri);
```

---

## Related Components

- `API/Responses/README.md` — Parent feature overview and neighboring components.
- `API/README.md` — Project setup and architecture.
- `DC bot.Contracts/` — Shared storage and command protocol used by handlers and services.
