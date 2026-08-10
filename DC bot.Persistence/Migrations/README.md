# Persistence Migrations

This folder contains EF Core migrations and the model snapshot.

## Why This Folder Exists

Migrations are the source-controlled history of database schema changes. They explain how the database moved from one shape to the next and allow new environments to be created consistently.

Unlike entities, configurations, and repositories, migrations are not organized by feature. EF tooling expects them as a chronological history, and keeping them together makes schema order obvious.

## Current Migrations

- `20260401102254_InitialMusicPersistence`
- `20260401121744_UpdateModel_20260401`
- `20260402134541_AddGuildRepeatListItems`
- `20260425193122_AddPlaybackStateQueueItemId`
- `20260425212714_AddQueueItemIdToPlaybackState` (renames `QueueItemId` to `queue_item_id` on `guild_playback_state`)
- `20260521133944_FixPendingModelChanges`
- `20260710104810_AddPlaylists`
- `20260803192210_AddBotControlCommands`
- `20260809192001_AddMobileAppUsers`
- `20260809211205_AddMobileAppSessions`
- `20260809215023_FixMobileAppSessionColumnNames`

`BotDbContextModelSnapshot.cs` reflects the latest schema model.

## Workflow

Common commands (run from solution root):

```bash
dotnet ef migrations add <MigrationName> --project "DC bot.Persistence/DC bot.Persistence.csproj" --startup-project "DC bot/DC bot.csproj"
dotnet ef database update --project "DC bot.Persistence/DC bot.Persistence.csproj" --startup-project "DC bot/DC bot.csproj"
```

## Runtime Behavior

`Db/DatabaseMigrationRunner.cs` checks for pending migrations and applies them automatically during bot startup.

## Caution

- Keep generated migration files source-controlled.
- Do not manually edit designer files unless absolutely necessary.
- Keep historical migration files in this folder instead of reorganizing them by feature.
