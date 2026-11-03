# DC bot.Persistence

This class library implements the solution's PostgreSQL storage using EF Core. The bot and API share its repositories, model configuration, migrations, and cross-process notification behavior through contracts defined in `DC bot.Contracts`.

## Overview

- `GuildDataRepository` saves twenty-four-seven preferences through tracked entity changes and `SaveChangesAsync`, preserving preferred channels when disabled and leaving premium settings intact. Apply `AddTwentyFourSevenSettings` before using these operations.
- Entity member documentation identifies stored keys, nullable lifecycle timestamps, navigation relationships, and playback start ownership; BotDbContext documents each entity set.
- Implement repository contracts with short-lived EF contexts and PostgreSQL.
- Protect command claims, queue order, batch state, and playback operation ownership.
- Maintain schema migrations and publish database change notifications.

## Runtime Flow

```text
Bot service or API handler
  -> shared repository interface
  -> repository + short-lived BotDbContext
  -> mapped PostgreSQL entities
  -> immutable contract result
```

Repository notifications wake workers or signal snapshot readers; the database remains the durable source of command and playback state. This project does not execute Discord commands, send HTTP responses, or own live Lavalink players.

## Stored Features

| Area | Stored responsibilities |
|---|---|
| Guilds | Guild defaults and premium state; premium-audit schema is mapped without a dedicated audit repository. |
| Playback | Current track, repeat flags, estimated position, start ownership, and remote node/session binding. |
| Queue and batches | Waiting order, conditional start claims, canonical batch items, repeat, and recent history. |
| Saved playlists | Named guild playlists and ordered serialized tracks. |
| Bot commands | API-submitted work, claim ownership, progress, errors, and result JSON. |
| Mobile users | Discord identity, stored guild access, refresh-session hashes, and realtime memberships. |
| User settings | Language, theme, feedback, sound, and telemetry preferences. |
| Bot voice status | Displayable guild voice presence, channel metadata, and voice-user counts. |

The retained `IBotRuntimeStatusRepository` contract is not bound by the current persistence registrations. Its existence must not be read as a running heartbeat writer or a join-readiness gate.

## Registration and Context Lifetime

Both executable projects call `AddPersistenceServices(connectionString)`. This registers `IDbContextFactory<BotDbContext>`, repository contracts, queue claim/batch helpers, and the bot-command notifier. Repository singletons create short-lived contexts; they do not share a single tracked context across concurrent operations.

Resolve queue storage through `IQueueRepository` so `QueueClaimService` and `QueueBatchRepository` are supplied by the composition root. Return shared records to callers instead of exposing EF entities. Discord ID conversion is centralized in model configuration helpers.

## Transactions, Cancellation, and Notifications

Multi-step mutations use conditional updates or transactions to protect queue ordering, command claims, and playback-start ownership. Supported PostgreSQL concurrency conflicts can be retried by the shared helper.

Requested cancellation reaches normal database I/O and retry delays. Rollback is owned cleanup: it uses a fresh five-second deadline so an already-cancelled caller does not prevent cleanup. If rollback fails, the original exception remains primary, the cleanup failure is attached under `DC_bot.TransactionRollbackFailure`, and no new transaction retry is started.

Notification helpers publish feature-specific PostgreSQL signals. Consumers reload current records and snapshots; a notification payload is not a complete queue or player state.

## Subfolders

### Configurations/

Groups PostgreSQL table, key, index, relationship, and conversion mappings by feature.

**Files:**

- `README.md` — Overview and navigation to feature-specific source files.

**Purpose:** BotDbContext applies these IEntityTypeConfiguration implementations when constructing its EF model.

---

### Db/

Owns the EF Core context, design-time creation, and startup migration runner.

**Files:**

- `BotDbContext.cs`
- `BotDbContextFactory.cs`
- `DatabaseMigrationRunner.cs`

**Purpose:** Runtime repositories receive IDbContextFactory<BotDbContext>. BotDbContext applies feature configurations; DatabaseMigrationRunner applies pending migrations when called by bot startup.

---

### DependencyInjection/

Registers the shared PostgreSQL context factory, repositories, queue helpers, and command notifier.

**Files:**

- `PersistenceServiceCollectionExtensions.cs`

**Purpose:** Both executable projects call AddPersistenceServices with their connection string. Singleton repositories create short-lived contexts rather than sharing a tracked DbContext.

---

### Entities/

Groups tracked database entity types by storage feature.

**Files:**

- `README.md` — Overview and navigation to feature-specific source files.

**Purpose:** BotDbContext and feature repositories use these types; Configurations defines their PostgreSQL mapping.

---

### Migrations/

Contains chronological EF Core migration operations, generated metadata, and the current model snapshot.

**Files:**

- `BotDbContextModelSnapshot.cs`
- Timestamped migration and matching designer files.

**Purpose:** Records schema evolution applied by bot startup or explicit EF tooling.

---

### Properties/

Controls test access to internal persistence helpers.

**Files:**

- `AssemblyInfo.cs`

**Purpose:** AssemblyInfo.cs exposes selected internal code to the test assembly through a friend-assembly declaration.

---

### Repositories/

Implements shared storage contracts with EF Core and PostgreSQL-specific concurrency and notification behavior.

**Files:**

- `README.md` — Overview and navigation to feature-specific source files.

**Purpose:** Repositories open short-lived contexts, map entities to contract records, and publish change signals where required. Feature folders separate command, playback, queue, profile, and playlist storage.

---

## Schema Workflow

Change the contract, entity, mapping, and repository behavior together when the storage use case changes. A schema change also needs a new migration; documentation-only or service-only changes do not.

Bot startup invokes `DatabaseMigrationRunner` before connecting the Discord runtime. The API does not currently invoke it. EF design-time tooling uses `BotDbContextFactory`, which reads `POSTGRES_HOST`, `POSTGRES_PORT`, `POSTGRES_DB`, `POSTGRES_USER`, and `POSTGRES_PASSWORD` from the process environment and does not itself load `.env`.

See [Migrations](Migrations/README.md) for generation and application commands. Check that tooling targets the intended database before applying schema changes.

## Related Components

- `DC bot/` — Discord commands, live playback lifecycle, and the mobile command worker.
- `API/` — HTTP request handling, authentication, response mapping, and SignalR publication.
- `DC bot.Contracts/` — Shared storage interfaces, immutable models, command payloads, and protocol names.
- `DC bot.Persistence/` — PostgreSQL repository implementations, migrations, and database notifications.
- `DC bot tests/` — Behavior tests and explicitly skipped planned cases.
- `AGENTS.md` — Implementation and detailed README conventions.
