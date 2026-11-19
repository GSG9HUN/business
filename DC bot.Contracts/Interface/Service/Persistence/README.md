# Persistence

Defines the shared storage vocabulary for guild data, commands, queues, playback, playlists, and mobile identities.

## Overview

- Defines repository operations for guild, playback, queue, playlist and mobile application state.
- Carries caller cancellation and explicit ownership scope across the storage boundary.
- Exchanges immutable records instead of EF entities or live player objects.

## Subfolders

### MusicControl/

Stores default-disabled vote skip and an optional DJ role independently of 24/7, autoplay and presets. GetAsync returns disabled/null defaults; SetVoteSkipAsync and SetDjRoleAsync update only their own field.

**Files:**

- `IGuildMusicControlSettingsRepository.cs`

**Purpose:** Stores default-disabled vote skip and an optional DJ role independently of 24/7, autoplay and presets. GetAsync returns disabled/null defaults; SetVoteSkipAsync and SetDjRoleAsync update only their own field.

---

### Filters/

Contains the dedicated saved filter preset contracts.

**Files:**

- `IFilterPresetRepository.cs`

**Purpose:** Keep durable guild definitions separate from temporary audio, playlist and queue state.

---

### Discovery/

Contains the dedicated music discovery contracts and data.

**Files:**

- `IFavoriteRepository.cs`
- `IDiscoverySettingsRepository.cs`

**Purpose:** Keep personal favorites and discovery opt-in responsibilities separate from queue, playlist and mobile state.

---

### BotControl/

Defines repository operations for durable bot commands, their claim/completion state, and cross-process change signals.

**Files:**

- `IBotControlCommandNotifier.cs`
- `IBotControlCommandsRepository.cs`

**Purpose:** The bot and API consume these interfaces; concrete implementations and DI registration live in DC bot.Persistence unless noted below.

---

### BotRuntimeStatus/

Defines repository operations for the retained bot-runtime heartbeat and liveness contract.

**Files:**

- `IBotRuntimeStatusRepository.cs`

**Purpose:** The bot and API consume these interfaces; concrete implementations and DI registration live in DC bot.Persistence unless noted below.

---

### Exceptions/

Defines the implementation-independent error used for a uniqueness conflict.

**Files:**

- `UniqueConstraintConflictException.cs`

**Purpose:** Repositories translate a provider-specific uniqueness violation into UniqueConstraintConflictException so callers can produce a stable conflict outcome.

---

### GuildBotStatus/

Defines repository operations for persisted bot voice presence, channel display metadata, and voice-user counts.

**Files:**

- `IGuildBotStatusRepository.cs`

**Purpose:** The bot and API consume these interfaces; concrete implementations and DI registration live in DC bot.Persistence unless noted below.

---

### Guilds/

Defines repository operations for guild defaults, premium state, and the mapped premium-audit schema.

**Files:**

- `IGuildDataRepository.cs`

**Purpose:** The bot and API consume these interfaces; concrete implementations and DI registration live in DC bot.Persistence unless noted below.

---

### MobileApps/

Defines repository operations for mobile identities, guild access, refresh sessions, and realtime subscription membership.

**Files:**

- `IMobileAppSessionRepository.cs`
- `IMobileAppUserRepository.cs`
- `IMobileRealtimeMembershipRepository.cs`

**Purpose:** The bot and API consume these interfaces; concrete implementations and DI registration live in DC bot.Persistence unless noted below.

---

### MobileAppUserSettings/

Defines repository operations for per-user mobile language, theme, feedback, sound, and telemetry preferences.

**Files:**

- `IMobileAppUserSettingsRepository.cs`

**Purpose:** The bot and API consume these interfaces; concrete implementations and DI registration live in DC bot.Persistence unless noted below.

---

### Models/

Groups immutable records and state enums passed across repository boundaries.

**Files:**

- `README.md` — Folder overview and navigation to its source components.

**Purpose:** Repositories map their EF entities into these records; services and API mappers use them without knowing table mappings.

---

### Playback/

Defines repository operations for current playback state, repeat flags, start ownership, and canonical batch metadata.

**Files:**

- `IPlaybackStateRepository.cs`
- `IRepeatListRepository.cs`

**Purpose:** The bot and API consume these interfaces; concrete implementations and DI registration live in DC bot.Persistence unless noted below.

---

### Playlists/

Defines repository operations for saved playlist metadata and ordered, serialized playlist tracks.

**Files:**

- `IPlaylistRepository.cs`
- `IPlaylistTrackRepository.cs`

**Purpose:** The bot and API consume these interfaces; concrete implementations and DI registration live in DC bot.Persistence unless noted below.

---

### Queue/

Defines repository operations for ordered queue entries, canonical batches, conditional start claims, and bounded playback history.

**Files:**

- `IQueueRepository.cs`

**Purpose:** The bot and API consume these interfaces; concrete implementations and DI registration live in DC bot.Persistence unless noted below.

---

### Status/

Defines repository operations for database reachability checks.

**Files:**

- `IDbStatusCheck.cs`

**Purpose:** The bot and API consume these interfaces; concrete implementations and DI registration live in DC bot.Persistence unless noted below.

---

## Related Components

- `DC bot.Contracts/Interface/Service/README.md` — Parent feature overview and neighboring components.
- `DC bot.Contracts/README.md` — Project setup and architecture.
- `DC bot.Contracts/Interface/Service/Persistence/BotControl/README.md` — Detailed component contracts and examples for this subfolder.
- `DC bot.Contracts/Interface/Service/Persistence/BotRuntimeStatus/README.md` — Detailed component contracts and examples for this subfolder.
- `DC bot.Contracts/Interface/Service/Persistence/Exceptions/README.md` — Detailed component contracts and examples for this subfolder.
- `DC bot.Contracts/Interface/Service/Persistence/GuildBotStatus/README.md` — Detailed component contracts and examples for this subfolder.
- `DC bot.Contracts/Interface/Service/Persistence/Guilds/README.md` — Detailed component contracts and examples for this subfolder.
- `DC bot.Contracts/Interface/Service/Persistence/MobileApps/README.md` — Detailed component contracts and examples for this subfolder.
- `DC bot.Contracts/Interface/Service/Persistence/MobileAppUserSettings/README.md` — Detailed component contracts and examples for this subfolder.
- `DC bot.Contracts/Interface/Service/Persistence/Models/README.md` — Detailed component contracts and examples for this subfolder.
- `DC bot.Contracts/Interface/Service/Persistence/Playback/README.md` — Detailed component contracts and examples for this subfolder.
- `DC bot.Contracts/Interface/Service/Persistence/Playlists/README.md` — Detailed component contracts and examples for this subfolder.
- `DC bot.Contracts/Interface/Service/Persistence/Queue/README.md` — Detailed component contracts and examples for this subfolder.
- `DC bot.Contracts/Interface/Service/Persistence/Status/README.md` — Detailed component contracts and examples for this subfolder.
- `DC bot.Persistence/` — Repository implementations and database mappings that consume these shared contracts.
