# Migrations

This folder contains EF Core schema migrations, their generated metadata, and the current model snapshot.

## Overview

`AddRequesterDisplayNames` adds nullable `requested_by_display_name` columns to `guild_queue_item` and `playback_batch_items` using the current EF table mappings. Existing requester mentions are retained; no destructive data rewrite is performed. The bot startup migration runner applies pending migrations before runtime activation.

`AddPlaybackVolume` adds the nullable integer `volume_percent` column to `guild_playback_state`; its rollback drops that column. Apply it from the repository root with `dotnet ef database update --project "DC bot.Persistence/DC bot.Persistence.csproj" --context BotDbContext` before deploying the updated API and bot. Migration generation does not apply schema changes.

- Records EF Core schema transitions and their generated target-model metadata.
- Keeps timestamped migration operations and the current model snapshot together.
- Supports applying schema changes during startup or with EF tooling.

## Migration Components

### AddGuildMusicControlSettings

**Purpose:** Creates `guild_music_control_settings` with a guild primary key, default-disabled voting and nullable numeric DJ role ID.

**Files:**

- `20261118092853_AddGuildMusicControlSettings.cs`
- `20261118092853_AddGuildMusicControlSettings.Designer.cs`

**Methods:** `Up` creates the table; `Down` drops it.

---

### AddFilterPresets

**Purpose:** Creates `filter_preset` with a composite guild/name primary key, schema version, JSONB definition, saving administrator and UTC timestamp.

**Files:**

- `20261117144346_AddFilterPresets.cs`
- `20261117144346_AddFilterPresets.Designer.cs`

**Methods:** `Up` creates the table; `Down` drops it.

---

### AddMusicDiscovery

**Files:**

- `20261117135456_AddMusicDiscovery.cs`, its designer metadata and `BotDbContextModelSnapshot.cs`

**Purpose:** Creates personal favorites with a unique user/source identity and independent disabled-by-default user picker/guild autoplay settings. No existing rows are rewritten.

**Methods:** `Up` creates three tables and the favorite index; `Down` drops those three tables.

---

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

Queue/canonical rows retain `bigint` batch keys. `enqueue_group_id` separates display grouping, `batch_position` records position and `is_removed` excludes canonical occurrences from repeats. Legacy occurrences receive deterministic display groups.

**Behavior:** `Up` defines batch tables, occurrence metadata, indexes and relationships; `Down` reverses that schema transition.

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
