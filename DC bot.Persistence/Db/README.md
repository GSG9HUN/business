# Persistence Db

This folder contains EF Core context types for the database layer.

## Why This Folder Exists

This folder is the database entry point for EF Core. It contains the runtime context used by repositories, the design-time context factory used by EF tooling, and the migration runner used during startup.

Keeping these pieces together makes it clear where database startup and schema application are configured.

## Files

### BotDbContext.cs

Main EF Core context.

DbSets:

- `GuildData`
- `GuildPlaybackStates`
- `GuildQueueItems`
- `GuildRepeatListItems`
- `GuildPremiumAudits`
- `Playlists`
- `PlaylistTracks`
- `BotControlCommands`
- `MobileAppUsers`
- `UserGuilds`
- `MobileAppSessions`

`OnModelCreating` applies all mappings from `../Configurations/`.

### BotDbContextFactory.cs

Design-time context factory for EF tooling (`dotnet ef`).

- reads PostgreSQL connection settings from environment variables
- creates a context instance for migration creation and update commands

### DatabaseMigrationRunner.cs

Runtime migration helper used by the bot startup flow.

- creates a scoped `BotDbContext`
- checks pending migrations
- applies migrations with `MigrateAsync()`

## Notes

- Context is consumed via `IDbContextFactory<BotDbContext>` in repository implementations.
- Use this folder when changing DB schema bootstrap behavior.
- Do not put repository query logic in `BotDbContext`; keep behavior in repositories.
