# Models

Groups immutable records and state enums passed across repository boundaries.

## Overview

- Repositories map their EF entities into these records; services and API mappers use them without knowing table mappings.
- Models describe storage use cases rather than HTTP serialization or live SDK state. Input records, snapshot records, and conditional mutation results have different roles.

## Subfolders

### BotControlCommand/

Defines shared records and state values for bot control command operations.

**Files:**

- `BotControlCommandRecord.cs`
- `BotControlCommandState.cs`

**Purpose:** Persistence maps entities into these values, and application services or API mappers consume them without database-provider dependencies.

---

### BotRuntimeStatus/

Defines shared records and state values for the retained bot-runtime heartbeat and liveness contract.

**Files:**

- `BotRuntimeStatusRecord.cs`

**Purpose:** Persistence maps entities into these values, and application services or API mappers consume them without database-provider dependencies.

---

### GuildBotStatus/

Defines shared records and state values for persisted bot voice presence, channel display metadata, and voice-user counts.

**Files:**

- `GuildBotStatusRecord.cs`

**Purpose:** Persistence maps entities into these values, and application services or API mappers consume them without database-provider dependencies.

---

### MobileApps/

Defines shared records and state values for mobile identities, guild access, refresh sessions, and realtime subscription membership.

**Files:**

- `MobileAppSessionRecord.cs`
- `MobileAppUserGuildRecord.cs`
- `MobileAppUserGuildUpsertRecord.cs`
- `MobileAppUserRecord.cs`
- `MobileAppUserUpsertRecord.cs`

**Purpose:** Persistence maps entities into these values, and application services or API mappers consume them without database-provider dependencies.

---

### MobileAppUserSettings/

Defines shared records and state values for per-user mobile language, theme, feedback, sound, and telemetry preferences.

**Files:**

- `MobileAppLanguageCode.cs`
- `MobileAppTheme.cs`
- `MobileAppUserSettingsPatchRecord.cs`
- `MobileAppUserSettingsRecord.cs`

**Purpose:** Persistence maps entities into these values, and application services or API mappers consume them without database-provider dependencies.

---

### Playback/

Defines shared records and state values for current playback state, repeat flags, start ownership, and canonical batch metadata.

**Files:**

- `PlaybackStateRecord.cs`

**Purpose:** Persistence maps entities into these values, and application services or API mappers consume them without database-provider dependencies.

---

### Playlists/

Defines shared records and state values for saved playlist metadata and ordered, serialized playlist tracks.

**Files:**

- `PlaylistRecord.cs`
- `PlaylistSummaryRecord.cs`
- `PlaylistTrackCreateRecord.cs`
- `PlaylistTrackRecord.cs`

**Purpose:** Persistence maps entities into these values, and application services or API mappers consume them without database-provider dependencies.

---

### Queue/

Defines shared records and state values for ordered queue entries, canonical batches, conditional start claims, and bounded playback history.

**Files:**

- `PendingPlaybackStart.cs`
- `PlaybackBatchRecord.cs`
- `QueueClaimResult.cs`
- `QueueClaimStatus.cs`
- `QueueItemRecord.cs`
- `QueueItemRemovalRecord.cs`
- `QueueItemState.cs`
- `QueueItemToEnqueue.cs`
- `QueueStartClaim.cs`

**Purpose:** Persistence maps entities into these values, and application services or API mappers consume them without database-provider dependencies.

---

## Guild Preference Models

### Guilds/

Contains the immutable persistent voice preference snapshot.

**Files:**

- `TwentyFourSevenSettingsRecord.cs`
- `README.md`

**Purpose:** Share the saved enabled flag and preferred voice/text channel identifiers between repositories and bot services without EF or Discord dependencies.

---

## Related Components

- `DC bot.Contracts/Interface/Service/Persistence/README.md` — Parent feature overview and neighboring components.
- `DC bot.Contracts/README.md` — Project setup and architecture.
- `DC bot.Contracts/Interface/Service/Persistence/Models/BotControlCommand/README.md` — Detailed component contracts and examples for this subfolder.
- `DC bot.Contracts/Interface/Service/Persistence/Models/BotRuntimeStatus/README.md` — Detailed component contracts and examples for this subfolder.
- `DC bot.Contracts/Interface/Service/Persistence/Models/GuildBotStatus/README.md` — Detailed component contracts and examples for this subfolder.
- `DC bot.Contracts/Interface/Service/Persistence/Models/MobileApps/README.md` — Detailed component contracts and examples for this subfolder.
- `DC bot.Contracts/Interface/Service/Persistence/Models/MobileAppUserSettings/README.md` — Detailed component contracts and examples for this subfolder.
- `DC bot.Contracts/Interface/Service/Persistence/Models/Playback/README.md` — Detailed component contracts and examples for this subfolder.
- `DC bot.Contracts/Interface/Service/Persistence/Models/Playlists/README.md` — Detailed component contracts and examples for this subfolder.
- `DC bot.Contracts/Interface/Service/Persistence/Models/Queue/README.md` — Detailed component contracts and examples for this subfolder.
- `DC bot.Persistence/` — Repository implementations and database mappings that consume these shared contracts.
