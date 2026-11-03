# Db

Owns the EF Core context, design-time creation, and startup migration runner.

## Overview

- Runtime repositories receive IDbContextFactory<BotDbContext>. BotDbContext applies feature configurations; DatabaseMigrationRunner applies pending migrations when called by bot startup.
- BotDbContextFactory reads process environment for EF tooling; it does not load a .env file itself. The API registers persistence but does not call the migration runner in its current Program.cs.

## Classes

### BotDbContext

**File:** `DC bot.Persistence/Db/BotDbContext.cs`

**Purpose:** Owns the shared EF Core model and persisted guild, queue, playback, playlist, and mobile state.

**Implements / inherits:** `DbContext`.

**Properties:**

```csharp
// The set of persisted Discord guild records.
public DbSet<GuildDataEntity> GuildData { get; }
public DbSet<GuildPlaybackStateEntity> GuildPlaybackStates { get; }
public DbSet<GuildQueueItemEntity> GuildQueueItems { get; }
public DbSet<PlaybackBatchEntity> PlaybackBatches { get; }
public DbSet<PlaybackBatchItemEntity> PlaybackBatchItems { get; }
public DbSet<GuildRepeatListItemEntity> GuildRepeatListItems { get; }
public DbSet<GuildPremiumAuditEntity> GuildPremiumAudits { get; }
public DbSet<PlaylistEntity> Playlists { get; }
public DbSet<PlaylistTrackEntity> PlaylistTracks { get; }
public DbSet<BotControlCommandEntity> BotControlCommands { get; }
public DbSet<MobileAppUserEntity> MobileAppUsers { get; }
public DbSet<UserGuildEntity> UserGuilds { get; }
public DbSet<MobileRealtimeGuildMembershipEntity> MobileRealtimeGuildMemberships { get; }
public DbSet<MobileAppSessionEntity> MobileAppSessions { get; }
public DbSet<GuildBotStatusEntity> GuildBotStatus { get; }
public DbSet<MobileAppUserSettingsEntity> MobileAppUserSettings { get; }
```

---

### DatabaseMigrationRunner

**File:** `DC bot.Persistence/Db/DatabaseMigrationRunner.cs`

**Purpose:** Applies pending EF migrations before application startup without taking ownership of the application's service provider.

**Methods:**

- `ApplyMigrationsIfNeededAsync` — Applies pending EF migrations in an asynchronous scope, forwarding application cancellation to database operations.

```csharp
public static async Task ApplyMigrationsIfNeededAsync(IServiceProvider services, CancellationToken cancellationToken = default);
```

**Usage:**

The caller supplies the component and argument values used in this example.

```csharp
await DatabaseMigrationRunner.ApplyMigrationsIfNeededAsync(services, cancellationToken);
```

---

## Factory Classes

### BotDbContextFactory

**File:** `DC bot.Persistence/Db/BotDbContextFactory.cs`

**Purpose:** Creates PostgreSQL contexts for EF design-time tooling using environment-based connection settings.

**Implements / inherits:** `IDesignTimeDbContextFactory<BotDbContext>`.

**Methods:**

- `CreateDbContext` — Implements `CreateDbContext` using the contract shown below.

```csharp
public BotDbContext CreateDbContext(string[] args);
```

**Usage:**

The caller supplies the component and argument values used in this example.

```csharp
var result = botDbContextFactory.CreateDbContext(args);
```

---

## Related Components

- `DC bot.Persistence/README.md` — Parent feature overview and neighboring components.
- `DC bot.Persistence/Configurations/BotControl/` — Defines EF Core table mappings for durable bot commands, their claim/completion state, and cross-process change signals.
- `DC bot.Persistence/Configurations/GuildBotStatus/` — Defines EF Core table mappings for persisted bot voice presence, channel display metadata, and voice-user counts.
- `DC bot.Persistence/Configurations/Guilds/` — Defines EF Core table mappings for guild defaults, premium state, and the mapped premium-audit schema.
- `DC bot.Persistence/Configurations/MobileApps/` — Defines EF Core table mappings for mobile identities, guild access, refresh sessions, and realtime subscription membership.
- `DC bot.Contracts/Interface/Service/Persistence/` — Shared interfaces and immutable values implemented by persistence.
