# Responses

Groups stable client-facing output shapes for auth, guilds, playback, queues, profiles, playlists, and music search.

## Overview

- Handlers, mappers, and snapshot providers construct these values from shared repository records and service results.
- Keep HTTP serialization concerns here. A response record must not expose a tracked EF entity or a live Discord or Lavalink object.

## Subfolders

### Auth/

Defines client-facing output for application access/refresh tokens and short-lived login exchange input.

**Files:**

- `AuthSessionResponse.cs`

**Purpose:** Handlers, mappers, and snapshot providers construct these records from shared contract values and return them through HTTP or matching realtime events.

---

### BotControl/

Defines client-facing output for durable bot commands, their claim/completion state, and cross-process change signals.

**Files:**

- `BotControlCommandResponse.cs`
- `BotControlCommandStatusResponse.cs`

**Purpose:** Handlers, mappers, and snapshot providers construct these records from shared contract values and return them through HTTP or matching realtime events.

---

### Guilds/

Defines client-facing output for guild defaults, premium state, and the mapped premium-audit schema.

**Files:**

- `GuildBotStatusResponse.cs`
- `GuildSummaryResponse.cs`

**Purpose:** Handlers, mappers, and snapshot providers construct these records from shared contract values and return them through HTTP or matching realtime events.

---

### MusicSearch/

Defines client-facing output for provider capabilities, search results, and safe provider error responses.

**Files:**

- `MusicSearchCapabilityResponse.cs`
- `MusicSearchErrorResponse.cs`
- `MusicSearchResponse.cs`
- `MusicSearchResultResponse.cs`

**Purpose:** Handlers, mappers, and snapshot providers construct these records from shared contract values and return them through HTTP or matching realtime events.

---

### Playback/

Defines client-facing output for current playback state, repeat flags, start ownership, and canonical batch metadata.

**Files:**

- `PlaybackStatusResponse.cs`
- `PlaybackTrackResponse.cs`

**Purpose:** Handlers, mappers, and snapshot providers construct these records from shared contract values and return them through HTTP or matching realtime events.

---

### Playlists/

Defines client-facing output for saved playlist metadata and ordered, serialized playlist tracks.

**Files:**

- `PlaylistDetailResponse.cs`
- `PlaylistSummaryResponse.cs`
- `PlaylistTrackResponse.cs`

**Purpose:** Handlers, mappers, and snapshot providers construct these records from shared contract values and return them through HTTP or matching realtime events.

---

### Profile/

Defines client-facing output for mobile user identity and optional preference updates.

**Files:**

- `ProfileResponse.cs`
- `ProfileSettingsResponse.cs`
- `ProfileUserResponse.cs`

**Purpose:** Handlers, mappers, and snapshot providers construct these records from shared contract values and return them through HTTP or matching realtime events.

---

### Queue/

Defines client-facing output for ordered queue entries, canonical batches, conditional start claims, and bounded playback history.

**Files:**

- `QueueResponse.cs`
- `QueueTrackResponse.cs`

**Purpose:** Handlers, mappers, and snapshot providers construct these records from shared contract values and return them through HTTP or matching realtime events.

---

## Related Components

- `API/README.md` — Parent feature overview and neighboring components.
- `API/Responses/Auth/README.md` — Detailed component contracts and examples for this subfolder.
- `API/Responses/BotControl/README.md` — Detailed component contracts and examples for this subfolder.
- `API/Responses/Guilds/README.md` — Detailed component contracts and examples for this subfolder.
- `API/Responses/MusicSearch/README.md` — Detailed component contracts and examples for this subfolder.
- `API/Responses/Playback/README.md` — Detailed component contracts and examples for this subfolder.
- `API/Responses/Playlists/README.md` — Detailed component contracts and examples for this subfolder.
- `API/Responses/Profile/README.md` — Detailed component contracts and examples for this subfolder.
- `API/Responses/Queue/README.md` — Detailed component contracts and examples for this subfolder.
- `DC bot.Contracts/` — Shared storage and command protocol used by handlers and services.
