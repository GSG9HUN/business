# MusicSearch

Authorizes provider capability discovery and track or playlist searches.

## Overview

- After guild-access validation, the handler calls IMusicSearchService and maps provider failures and retry hints into safe HTTP responses. Repeated search query parameters are rejected.
- The request token reaches authorization and search I/O. Provider response codes and rate-limit headers belong at this HTTP boundary.

## Classes

### MusicSearchHandlers

**File:** `API/Handlers/MusicSearch/MusicSearchHandlers.cs`

**Purpose:** Authorizes guild music searches and translates provider failures into HTTP responses.

**Methods:**

- `GetCapabilitiesAsync` — Returns deployed provider search and enqueue capabilities based on the Lavalink node configuration.

```csharp
public static Task<IResult> GetCapabilitiesAsync(
    HttpContext context,
    IMobileAppUserRepository users,
    IMusicSearchService service,
    CancellationToken cancellationToken);
```

- `SearchAsync` — Validates the music query and requests matching results from an enabled provider.

```csharp
public static Task<IResult> SearchAsync(
    HttpContext context,
    IMobileAppUserRepository users,
    IMusicSearchService service,
    CancellationToken cancellationToken);
```

**Usage:**

The caller supplies the component and argument values used in this example.

```csharp
var result = await MusicSearchHandlers.GetCapabilitiesAsync(context, users, service, cancellationToken);
```

---

## Related Components

- `API/Handlers/README.md` — Parent feature overview and neighboring components.
- `API/README.md` — Project setup and architecture.
- `API/Responses/MusicSearch/` — Defines client-facing output for provider capabilities, search results, and safe provider error responses.
- `API/Services/MusicSearch/` — Validates provider queries and returns bounded track or playlist search results from Lavalink.
- `DC bot.Contracts/Interface/Service/Persistence/MobileApps/` — Defines repository operations for mobile identities, guild access, refresh sessions, and realtime subscription membership.
- `DC bot.Contracts/` — Shared storage and command protocol used by handlers and services.
