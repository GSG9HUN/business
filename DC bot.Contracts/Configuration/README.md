# Configuration

Shared configuration models consumed by the API and bot runtime.

## Overview

- Keep saved-playlist limit defaults consistent across executable projects.
- Contain configuration data without runtime or database dependencies.

## Configuration Models

### PlaylistOptions

**Purpose:** Defines maximum playlists per guild (5), tracks per playlist (100), and tracks per import (100).

**Properties:** `MaxPlaylistsPerGuild`, `MaxTracksPerPlaylist`, and `MaxImportedTracks`.

Consumers clamp effective limits to at least one. Both runtimes register options with code defaults; there is no environment-variable mapping for these limits.

---

## Related Components

- `API/Handlers/Playlists/PlaylistHandlers.cs` - Checks the per-guild limit before creation.
- `DC bot/Service/Music/PlaylistService/` - Enforces playlist and track limits in bot operations.
