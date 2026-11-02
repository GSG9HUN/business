# MusicSearch

Defines client-facing output for provider capabilities, search results, and safe provider error responses.

## Overview

- Handlers, mappers, and snapshot providers construct these records from shared contract values and return them through HTTP or matching realtime events.
- Keep field meaning consistent for mobile consumers. An output model describes observable state and must not contain a tracked entity or a live SDK object.

## Models and Results

### MusicSearchCapabilityResponse

**File:** `API/Responses/MusicSearch/MusicSearchCapabilityResponse.cs`

**Purpose:** Carries the music search capability payload returned to HTTP and mobile clients.

**Record declaration:**

```csharp
public sealed record MusicSearchCapabilityResponse(
    string ProviderId,
    bool SupportsTrackSearch,
    bool SupportsPlaylistSearch,
    bool CanEnqueue,
    string? UnavailableReason,
    bool SupportsPagination = false);
```

---

### MusicSearchErrorResponse

**File:** `API/Responses/MusicSearch/MusicSearchErrorResponse.cs`

**Purpose:** Carries the music search error payload returned to HTTP and mobile clients.

**Record declaration:**

```csharp
public sealed record MusicSearchErrorResponse(string ErrorCode, string ErrorMessage);
```

---

### MusicSearchResponse

**File:** `API/Responses/MusicSearch/MusicSearchResponse.cs`

**Purpose:** Carries the music search payload returned to HTTP and mobile clients.

**Record declaration:**

```csharp
public sealed record MusicSearchResponse(
    IReadOnlyList<MusicSearchResultResponse> Results,
    string? NextPageToken = null,
    bool IsLimited = true);
```

---

### MusicSearchResultResponse

**File:** `API/Responses/MusicSearch/MusicSearchResultResponse.cs`

**Purpose:** Carries the music search result payload returned to HTTP and mobile clients.

**Record declaration:**

```csharp
public sealed record MusicSearchResultResponse(
    string Id,
    string ProviderId,
    string Kind,
    string Title,
    string? Creator,
    string? ThumbnailUrl,
    int? DurationSeconds,
    int? ItemCount,
    string? CanonicalUrl,
    bool CanEnqueue,
    string? UnavailableReason = null);
```

---

## Related Components

- `API/Responses/README.md` — Parent feature overview and neighboring components.
- `API/README.md` — Project setup and architecture.
- `DC bot.Contracts/` — Shared storage and command protocol used by handlers and services.
