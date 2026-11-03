# Api

Isolated API tests for search, enqueue validation, and saved playlist artwork responses.

## Overview

- Verify search provider handling and endpoint behavior without live services.
- Validate allowed and rejected enqueue inputs.
- Verify playlist cover selection and per-track artwork using encoded metadata and mocked repositories.

## Components

### PlaylistArtworkTests

**Purpose:** Verifies playlist list and detail responses choose the first available track artwork, retain individual track artwork, and serialize the cover as `artworkUri`.

**Validation Checks:**

- Empty playlists and missing artwork return null.
- The first available image wins when multiple tracks have artwork.
- Tracks without artwork and undecodable tracks are skipped during cover selection.
- Invalid metadata does not prevent returning a playlist response.

---

### Music Search and Enqueue Tests

**Files:**

- `MusicSearchTests.cs`
- `MusicSearchEndpointTests.cs`
- `MusicEnqueueValidationTests.cs`

**Purpose:** Verify provider capabilities, search output, request validation, and endpoint responses.

## Related Components

- `API/Handlers/Playlists/` - playlist list and detail handlers under test.
- `API/Mapping/TrackResponseMapper.cs` - saved track decoding and cover selection.
- `API/Services/MusicSearch/` - search services under test.

## Custom Cover Coverage

`PlaylistArtworkTests` also verifies custom-cover precedence, clearing a cover with null, whitespace trimming, rejected URL schemes/credentials, and guild authorization before repository writes.

## Playlist Command Execution

The new `PlaylistCreationTests` and `PlaylistBotControlHandlerTests` contain test scaffolds only. All scenarios are explicitly skipped with implementation pending; they do not provide behavior coverage.

## Queue Snapshot Playlist Saving

QueuePlaylistSnapshotTests is a skipped scaffold for history/current/queue ordering, duplicate URLs, access, limits, and playback advancement. Test implementation is pending.
