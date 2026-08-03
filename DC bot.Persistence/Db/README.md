# Persistence Db

This folder contains EF Core context types for the database layer.

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
