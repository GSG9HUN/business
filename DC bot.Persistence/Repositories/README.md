# Repositories

Implements shared storage contracts with EF Core and PostgreSQL-specific concurrency and notification behavior.

## Overview

- Repositories open short-lived contexts, map entities to contract records, and publish change signals where required. Feature folders separate command, playback, queue, profile, and playlist storage.
- Services consume contracts rather than tracked entities. Serializable retries and rollback cleanup are shared infrastructure concerns, not command-handler logic.

## Subfolders

### BotControl/

Implements shared storage operations for durable bot commands, their claim/completion state, and cross-process change signals.

**Files:**

- `BotControlCommandsRepository.cs`
- `PostgreSqlBotControlCommandNotifier.cs`

**Purpose:** Callers resolve the contracts through AddPersistenceServices; implementations create contexts through IDbContextFactory and map database state into shared records.

---

### GuildBotStatus/

Implements shared storage operations for persisted bot voice presence, channel display metadata, and voice-user counts.

**Files:**

- `GuildBotStatusRealtimeNotifier.cs`
- `GuildBotStatusRepository.cs`

**Purpose:** Callers resolve the contracts through AddPersistenceServices; implementations create contexts through IDbContextFactory and map database state into shared records.

---

### Guilds/

Implements shared storage operations for guild defaults, premium state, and the mapped premium-audit schema.

**Files:**

- `GuildDataBootstrapper.cs`
- `GuildDataRepository.cs`

**Purpose:** Callers resolve the contracts through AddPersistenceServices; implementations create contexts through IDbContextFactory and map database state into shared records.

---

### MobileApps/

Implements shared storage operations for mobile identities, guild access, refresh sessions, and realtime subscription membership.

**Files:**

- `MobileAppSessionRepository.cs`
- `MobileAppUserRepository.cs`
- `MobileRealtimeMembershipRepository.cs`

**Purpose:** Callers resolve the contracts through AddPersistenceServices; implementations create contexts through IDbContextFactory and map database state into shared records.

---

### MobileAppUserSettings/

Implements shared storage operations for per-user mobile language, theme, feedback, sound, and telemetry preferences.

**Files:**

- `MobileAppUserSettingsRepository.cs`

**Purpose:** Callers resolve the contracts through AddPersistenceServices; implementations create contexts through IDbContextFactory and map database state into shared records.

---

### Playback/

Implements shared storage operations for current playback state, repeat flags, start ownership, and canonical batch metadata.

**Files:**

- `PlaybackRealtimeNotifier.cs`
- `PlaybackStateRepository.cs`
- `RepeatListRepository.cs`

**Purpose:** Callers resolve the contracts through AddPersistenceServices; implementations create contexts through IDbContextFactory and map database state into shared records.

---

### Playlists/

Implements shared storage operations for saved playlist metadata and ordered, serialized playlist tracks.

**Files:**

- `PlaylistRepository.cs`
- `PlaylistTrackRepository.cs`

**Purpose:** Callers resolve the contracts through AddPersistenceServices; implementations create contexts through IDbContextFactory and map database state into shared records.

---

### Queue/

Implements shared storage operations for ordered queue entries, canonical batches, conditional start claims, and bounded playback history.

**Files:**

- `QueueBatchRepository.cs`
- `QueueBatchStore.cs`
- `QueueClaimService.cs`
- `QueueItemMapper.cs`
- `QueueRealtimeNotifier.cs`
- `QueueRepository.cs`

**Purpose:** Callers resolve the contracts through AddPersistenceServices; implementations create contexts through IDbContextFactory and map database state into shared records.

---

### Shared/

Provides PostgreSQL transaction retries and owned rollback handling shared by repositories.

**Files:**

- `PostgreSqlConcurrencyHelper.cs`
- `TransactionCleanup.cs`

**Purpose:** PostgreSqlConcurrencyHelper retries supported concurrency conflicts. TransactionCleanup attempts rollback with a fresh five-second deadline even if the caller was cancelled.

---

### Status/

Implements IDbStatusCheck without requiring a feature-specific data query.

**Files:**

- `DbStatusCheck.cs`

**Purpose:** DbStatusCheck creates a context through the factory and calls the provider's connectivity check with caller cancellation.

---

## Related Components

- `DC bot.Persistence/README.md` — Parent feature overview and neighboring components.
- `DC bot.Persistence/Repositories/BotControl/README.md` — Detailed component contracts and examples for this subfolder.
- `DC bot.Persistence/Repositories/GuildBotStatus/README.md` — Detailed component contracts and examples for this subfolder.
- `DC bot.Persistence/Repositories/Guilds/README.md` — Detailed component contracts and examples for this subfolder.
- `DC bot.Persistence/Repositories/MobileApps/README.md` — Detailed component contracts and examples for this subfolder.
- `DC bot.Persistence/Repositories/MobileAppUserSettings/README.md` — Detailed component contracts and examples for this subfolder.
- `DC bot.Persistence/Repositories/Playback/README.md` — Detailed component contracts and examples for this subfolder.
- `DC bot.Persistence/Repositories/Playlists/README.md` — Detailed component contracts and examples for this subfolder.
- `DC bot.Persistence/Repositories/Queue/README.md` — Detailed component contracts and examples for this subfolder.
- `DC bot.Persistence/Repositories/Shared/README.md` — Detailed component contracts and examples for this subfolder.
- `DC bot.Persistence/Repositories/Status/README.md` — Detailed component contracts and examples for this subfolder.
- `DC bot.Contracts/Interface/Service/Persistence/` — Shared interfaces and immutable values implemented by persistence.
