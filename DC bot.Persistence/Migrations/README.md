# Migrations

This folder contains EF Core schema migrations, their generated metadata, and the current model snapshot.

## Overview

Convention maintenance removes compiler-reported unused imports and documents the artwork target-model override. Migration operations, identifiers, and generated target-model expressions are unchanged.

- Preserve the chronological schema transitions required by the current database model.
- Keep each migration operation file together with its matching generated designer file.
- Apply pending schema changes through bot startup or an explicit EF tooling command.

**Schema Change Checks**

- Review the generated operations and snapshot against the entity/configuration change.
- Apply schema changes to the intended database environment.
- Add a new migration for a new schema transition instead of rewriting previously deployed history.
- Documentation, DI, XML, and cancellation-only changes do not need a migration unless the model also changes.

**Persistent Voice Schema**

**AddTwentyFourSevenSettings**

**Files:**

- `20261023110102_AddTwentyFourSevenSettings.cs`
- `20261023110102_AddTwentyFourSevenSettings.Designer.cs`

**Purpose:** Adds `is_twenty_four_seven_enabled` (default false), `preferred_voice_channel_id`, and `preferred_text_channel_id` to `guild_data`. The channel identifiers are nullable `numeric(20,0)` values. Apply this migration before using preference reads and writes; it changes storage only.

---

**Staging Integration**

The regenerated AddQueueBatches migration adds enqueue_group_id to queue and canonical rows, batch_position to queue rows, and is_removed to canonical items. It preserves the existing bigint batch_id foreign keys. Legacy occurrences receive deterministic separate display groups. AddPlaylistArtwork metadata includes the staging canonical batch model. These migrations were generated and checked without applying them to a running database.

## Migration Components

### Timestamped Migration Files

**Files:**

- Timestamped `.cs` migration files define `Up` and `Down` schema operations.
- Matching `.Designer.cs` files contain EF-generated model metadata.

**Purpose:** Record a schema transition rather than a change to live Discord or Lavalink state.

**Methods:**

Each migration overrides the EF migration contract:

```csharp
protected override void Up(MigrationBuilder migrationBuilder);
protected override void Down(MigrationBuilder migrationBuilder);
```

**Usage:**

Run from the repository root with an EF CLI compatible with the project's EF Core version:

```bash
dotnet ef migrations add <MigrationName> --project "DC bot.Persistence/DC bot.Persistence.csproj" --startup-project "DC bot.Persistence/DC bot.Persistence.csproj"
dotnet ef database update --project "DC bot.Persistence/DC bot.Persistence.csproj" --startup-project "DC bot.Persistence/DC bot.Persistence.csproj"
```

To inspect unapplied migrations without changing the database, run `dotnet ef migrations list --project "DC bot.Persistence/DC bot.Persistence.csproj" --startup-project "DC bot.Persistence/DC bot.Persistence.csproj"`. Apply existing pending migrations with the `database update` command above; generating a new migration is unnecessary for that operation. Run against the same database as the API. In the Docker Compose deployment, rebuilding and starting `discord-bot` also applies pending migrations before connecting to Discord. An existing history entry combined with missing columns indicates schema drift and needs separate investigation; an ordinary update will not rerun an already recorded migration.

`BotDbContextFactory` reads `POSTGRES_HOST`, `POSTGRES_PORT`, `POSTGRES_DB`, `POSTGRES_USER`, and `POSTGRES_PASSWORD` from the tooling process environment. It does not load `.env` itself. Bot startup separately invokes `DatabaseMigrationRunner`; the API does not currently apply migrations in its startup path.

---

### AddPlaybackBatches

**File:** `20261021105145_AddPlaybackBatches.cs`

**Purpose:** Adds canonical playback batch and item storage and links queue/playback state to that batch model.

**Behavior:** This migration is already present. Enabling the existing batch behavior does not require generating a duplicate migration. Inspect `Up` and `Down` for the exact columns, indexes, relationships, and tables involved.

---

### BotDbContextModelSnapshot

**File:** `BotDbContextModelSnapshot.cs`

**Purpose:** Describes the model after the generated migration history, allowing EF tooling to detect subsequent model changes.

**Benefits:**

- Keeps generated schema comparisons tied to a source-controlled model.
- Makes a new migration describe a transition from the previous model.
- Separates schema evolution from repository implementation and documentation changes.

## Related Components

- `DC bot.Persistence/Db/BotDbContext.cs` — EF model assembled from entity configurations.
- `DC bot.Persistence/Db/BotDbContextFactory.cs` — Design-time PostgreSQL context creation.
- `DC bot.Persistence/Db/DatabaseMigrationRunner.cs` — Applies pending migrations when invoked by startup.
- `DC bot.Persistence/Configurations/` — Table, column, relationship, index, and conversion definitions.
- `DC bot/Startup/BotApplication.cs` — Calls the migration runner before starting bot runtime work.
