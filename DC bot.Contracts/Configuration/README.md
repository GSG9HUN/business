# Configuration

Shared configuration models consumed by the API and bot runtime.

## Overview

- Keep saved-playlist count limit defaults consistent across executable projects for guilds without active premium.
- Contain configuration data without runtime or database dependencies.

## Configuration Models

### PlaylistOptions

**Purpose:** Defines maximum playlists per guild (5), tracks per playlist (100), and tracks per import (100) for guilds without active premium. Consumers bypass these count limits when premium is enabled and has no expiry or has not expired. Name, ownership, uniqueness, and storage validation still apply.

**Properties:** `MaxPlaylistsPerGuild`, `MaxTracksPerPlaylist`, and `MaxImportedTracks`.

Consumers clamp effective limits to at least one. Both runtimes register options with code defaults; there is no environment-variable mapping for these limits.

---

## Related Components

- `API/Handlers/Playlists/PlaylistHandlers.cs` - Requires active premium and delegates atomic creation to the repository.
- `DC bot/Service/Music/PlaylistService/` - Applies playlist, track, and import count limits only to guilds without active premium.
- `DC bot.Persistence/Repositories/Playlists/PlaylistRepository.cs` - Checks premium entitlement and applicable snapshot limits inside the creation transaction.
