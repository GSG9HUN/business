# DC bot.Persistence

This class library implements the solution's PostgreSQL storage using EF Core. The bot and API share its repositories, model configuration, migrations, and cross-process notification behavior through contracts defined in `DC bot.Contracts`.

## Overview

- Queue clearing can return the affected count from the committed serializable update. Existing no-count callers remain supported; the Discord response consumes this count without an earlier race-prone count query.

- Implements shared PostgreSQL storage for guilds, playback, queues, playlists and durable commands.
- Maps EF Core entities to SDK-independent repository contracts and result records.
- Coordinates transactional writes, concurrency retries and schema migrations.

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

## Related Components

- `DC bot/` — Discord commands, live playback lifecycle, and the mobile command worker.
- `API/` — HTTP request handling, authentication, response mapping, and SignalR publication.
- `DC bot.Contracts/` — Shared storage interfaces, immutable models, command payloads, and protocol names.
- `DC bot.Persistence/` — PostgreSQL repository implementations, migrations, and database notifications.
- `DC bot tests/` — Behavior tests and explicitly skipped planned cases.
- `AGENTS.md` — Implementation and detailed README conventions.
