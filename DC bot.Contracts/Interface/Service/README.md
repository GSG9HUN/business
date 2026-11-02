# Service

Groups persistence abstractions used by application services and handlers.

## Overview

- Feature interfaces expose immutable records and cancellable operations; the persistence project supplies database implementations.
- The folder name describes the consuming layer, not a requirement to implement database access inside a service.

## Subfolders

### Persistence/

Defines the shared storage vocabulary for guild data, commands, queues, playback, playlists, and mobile identities.

**Files:**

- `README.md` — Folder overview and navigation to its source components.

**Purpose:** API handlers and bot services depend on these interfaces while persistence registrations select the implementations.

---

## Related Components

- `DC bot.Contracts/Interface/README.md` — Parent feature overview and neighboring components.
- `DC bot.Contracts/README.md` — Project setup and architecture.
- `DC bot.Contracts/Interface/Service/Persistence/README.md` — Detailed component contracts and examples for this subfolder.
- `DC bot.Persistence/` — Repository implementations and database mappings that consume these shared contracts.
