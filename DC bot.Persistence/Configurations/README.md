# Configurations

Groups PostgreSQL table, key, index, relationship, and conversion mappings by feature.

## Overview

- BotDbContext applies these IEntityTypeConfiguration implementations when constructing its EF model.
- Entity shape belongs in Entities; storage behavior belongs in Repositories. A schema-affecting mapping change normally needs a new migration.

## Subfolders

### BotControl/

Defines EF Core table mappings for durable bot commands, their claim/completion state, and cross-process change signals.

**Files:**

- `BotControlCommandsConfiguration.cs`

**Purpose:** These mappings connect the corresponding Entities feature to PostgreSQL columns, keys, indexes, and relationships used by repositories.

---

### GuildBotStatus/

Defines EF Core table mappings for persisted bot voice presence, channel display metadata, and voice-user counts.

**Files:**

- `GuildBotStatusConfiguration.cs`

**Purpose:** These mappings connect the corresponding Entities feature to PostgreSQL columns, keys, indexes, and relationships used by repositories.

---

### Guilds/

Defines EF Core table mappings for guild defaults, premium state, and the mapped premium-audit schema.

**Files:**

- `GuildDataConfiguration.cs`
- `GuildPremiumAuditConfiguration.cs`

**Purpose:** These mappings connect the corresponding Entities feature to PostgreSQL columns, keys, indexes, and relationships used by repositories.

---

### MobileApps/

Defines EF Core table mappings for mobile identities, guild access, refresh sessions, and realtime subscription membership.

**Files:**

- `MobileAppSessionsConfiguration.cs`
- `MobileAppUsersConfiguration.cs`
- `MobileRealtimeGuildMembershipsConfiguration.cs`
- `UserGuildsConfiguration.cs`

**Purpose:** These mappings connect the corresponding Entities feature to PostgreSQL columns, keys, indexes, and relationships used by repositories.

---

### MobileAppUserSettings/

Defines EF Core table mappings for per-user mobile language, theme, feedback, sound, and telemetry preferences.

**Files:**

- `MobileAppUserSettingsConfiguration.cs`

**Purpose:** These mappings connect the corresponding Entities feature to PostgreSQL columns, keys, indexes, and relationships used by repositories.

---

### Playback/

Defines EF Core table mappings for current playback state, repeat flags, start ownership, and canonical batch metadata.

**Files:**

- `GuildPlaybackStateConfiguration.cs`
- `GuildRepeatListItemConfiguration.cs`
- `PlaybackBatchConfiguration.cs`
- `PlaybackBatchItemConfiguration.cs`

**Purpose:** These mappings connect the corresponding Entities feature to PostgreSQL columns, keys, indexes, and relationships used by repositories.

---

### Playlists/

Defines EF Core table mappings for saved playlist metadata and ordered, serialized playlist tracks.

**Files:**

- `PlaylistConfiguration.cs`
- `PlaylistTrackConfiguration.cs`

**Purpose:** These mappings connect the corresponding Entities feature to PostgreSQL columns, keys, indexes, and relationships used by repositories.

---

### Queue/

Defines EF Core table mappings for ordered queue entries, canonical batches, conditional start claims, and bounded playback history.

**Files:**

- `GuildQueueItemConfiguration.cs`

**Purpose:** These mappings connect the corresponding Entities feature to PostgreSQL columns, keys, indexes, and relationships used by repositories.

---

### Shared/

Centralizes the Discord guild identifier conversion used by entity mappings.

**Files:**

- `GuildIdPropertyBuilderExtensions.cs`

**Purpose:** Feature configuration classes call GuildIdPropertyBuilderExtensions rather than repeating provider conversion rules.

---

## Related Components

- `DC bot.Persistence/README.md` — Parent feature overview and neighboring components.
- `DC bot.Persistence/Configurations/BotControl/README.md` — Detailed component contracts and examples for this subfolder.
- `DC bot.Persistence/Configurations/GuildBotStatus/README.md` — Detailed component contracts and examples for this subfolder.
- `DC bot.Persistence/Configurations/Guilds/README.md` — Detailed component contracts and examples for this subfolder.
- `DC bot.Persistence/Configurations/MobileApps/README.md` — Detailed component contracts and examples for this subfolder.
- `DC bot.Persistence/Configurations/MobileAppUserSettings/README.md` — Detailed component contracts and examples for this subfolder.
- `DC bot.Persistence/Configurations/Playback/README.md` — Detailed component contracts and examples for this subfolder.
- `DC bot.Persistence/Configurations/Playlists/README.md` — Detailed component contracts and examples for this subfolder.
- `DC bot.Persistence/Configurations/Queue/README.md` — Detailed component contracts and examples for this subfolder.
- `DC bot.Persistence/Configurations/Shared/README.md` — Detailed component contracts and examples for this subfolder.
- `DC bot.Contracts/Interface/Service/Persistence/` — Shared interfaces and immutable values implemented by persistence.
