# Requests

Groups client input by feature before handler validation and orchestration.

## Overview

- Minimal API binding creates these values; filters and handlers interpret them and build repository inputs or bot-command payloads.
- HTTP request shapes are distinct from shared persistence records. Validation should not be hidden in EF entities or response models.

## Subfolders

### Auth/

Defines client-supplied input for application access/refresh tokens and short-lived login exchange input.

**Files:**

- `ExchangeRequest.cs`
- `RefreshRequest.cs`

**Purpose:** Minimal API binding constructs these values; feature filters and handlers normalize and validate them before repository or service calls.

---

### Playback/

Defines client-supplied input for current playback state, repeat flags, start ownership, and canonical batch metadata.

**Files:**

- `SetRepeatModeRequest.cs`

**Purpose:** Minimal API binding constructs these values; feature filters and handlers normalize and validate them before repository or service calls.

---

### Playlists/

Defines client-supplied input for saved playlist metadata and ordered, serialized playlist tracks.

**Files:**

- `AddSongRequest.cs`
- `CreatePlaylistRequest.cs`
- `RenamePlaylistRequest.cs`
- `SavePlaylistRequest.cs`

**Purpose:** Minimal API binding constructs these values; feature filters and handlers normalize and validate them before repository or service calls.

---

### Profile/

Defines client-supplied input for mobile user identity and optional preference updates.

**Files:**

- `UpdateProfileSettingsRequest.cs`

**Purpose:** Minimal API binding constructs these values; feature filters and handlers normalize and validate them before repository or service calls.

---

### Queue/

Defines client-supplied input for ordered queue entries, canonical batches, conditional start claims, and bounded playback history.

**Files:**

- `EnqueueRequest.cs`

**Purpose:** Minimal API binding constructs these values; feature filters and handlers normalize and validate them before repository or service calls.

---

## Related Components

- `API/README.md` — Parent feature overview and neighboring components.
- `API/Requests/Auth/README.md` — Detailed component contracts and examples for this subfolder.
- `API/Requests/Playback/README.md` — Detailed component contracts and examples for this subfolder.
- `API/Requests/Playlists/README.md` — Detailed component contracts and examples for this subfolder.
- `API/Requests/Profile/README.md` — Detailed component contracts and examples for this subfolder.
- `API/Requests/Queue/README.md` — Detailed component contracts and examples for this subfolder.
- `DC bot.Contracts/` — Shared storage and command protocol used by handlers and services.
