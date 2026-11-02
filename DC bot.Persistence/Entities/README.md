# Entities

Groups tracked database entity types by storage feature.

## Overview

- BotDbContext and feature repositories use these types; Configurations defines their PostgreSQL mapping.
- Return shared contract records to services and API handlers instead of exposing tracked entities across the persistence boundary.

## Subfolders

### BotControl/

Represents the database rows used for durable bot commands, their claim/completion state, and cross-process change signals.

**Files:**

- `BotControlCommandEntity.cs`

**Purpose:** Feature repositories read and mutate these values in short-lived EF contexts, then map results into DC bot.Contracts records.

---

### GuildBotStatus/

Represents the database rows used for persisted bot voice presence, channel display metadata, and voice-user counts.

**Files:**

- `GuildBotStatusEntity.cs`

**Purpose:** Feature repositories read and mutate these values in short-lived EF contexts, then map results into DC bot.Contracts records.

---

### Guilds/

Represents the database rows used for guild defaults, premium state, and the mapped premium-audit schema.

**Files:**

- `GuildDataEntity.cs`
- `GuildPremiumAuditEntity.cs`

**Purpose:** Feature repositories read and mutate these values in short-lived EF contexts, then map results into DC bot.Contracts records.

---

### MobileApps/

Represents the database rows used for mobile identities, guild access, refresh sessions, and realtime subscription membership.

**Files:**

- `MobileAppSessionEntity.cs`
- `MobileAppUserEntity.cs`
- `MobileRealtimeGuildMembershipEntity.cs`
- `UserGuildEntity.cs`

**Purpose:** Feature repositories read and mutate these values in short-lived EF contexts, then map results into DC bot.Contracts records.

---

### MobileAppUserSettings/

Represents the database rows used for per-user mobile language, theme, feedback, sound, and telemetry preferences.

**Files:**

- `MobileAppUserSettingsEntity.cs`

**Purpose:** Feature repositories read and mutate these values in short-lived EF contexts, then map results into DC bot.Contracts records.

---

### Playback/

Represents the database rows used for current playback state, repeat flags, start ownership, and canonical batch metadata.

**Files:**

- `GuildPlaybackStateEntity.cs`
- `GuildRepeatListItemEntity.cs`
- `PlaybackBatchEntity.cs`
- `PlaybackBatchItemEntity.cs`

**Purpose:** Feature repositories read and mutate these values in short-lived EF contexts, then map results into DC bot.Contracts records.

---

### Playlists/

Represents the database rows used for saved playlist metadata and ordered, serialized playlist tracks.

**Files:**

- `PlaylistEntity.cs`
- `PlaylistTrackEntity.cs`

**Purpose:** Feature repositories read and mutate these values in short-lived EF contexts, then map results into DC bot.Contracts records.

---

### Queue/

Represents the database rows used for ordered queue entries, canonical batches, conditional start claims, and bounded playback history.

**Files:**

- `GuildQueueItemEntity.cs`

**Purpose:** Feature repositories read and mutate these values in short-lived EF contexts, then map results into DC bot.Contracts records.

---

## Related Components

- `DC bot.Persistence/README.md` — Parent feature overview and neighboring components.
- `DC bot.Persistence/Entities/BotControl/README.md` — Detailed component contracts and examples for this subfolder.
- `DC bot.Persistence/Entities/GuildBotStatus/README.md` — Detailed component contracts and examples for this subfolder.
- `DC bot.Persistence/Entities/Guilds/README.md` — Detailed component contracts and examples for this subfolder.
- `DC bot.Persistence/Entities/MobileApps/README.md` — Detailed component contracts and examples for this subfolder.
- `DC bot.Persistence/Entities/MobileAppUserSettings/README.md` — Detailed component contracts and examples for this subfolder.
- `DC bot.Persistence/Entities/Playback/README.md` — Detailed component contracts and examples for this subfolder.
- `DC bot.Persistence/Entities/Playlists/README.md` — Detailed component contracts and examples for this subfolder.
- `DC bot.Persistence/Entities/Queue/README.md` — Detailed component contracts and examples for this subfolder.
- `DC bot.Contracts/Interface/Service/Persistence/` — Shared interfaces and immutable values implemented by persistence.
