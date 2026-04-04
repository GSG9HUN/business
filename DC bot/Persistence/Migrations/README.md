# Persistence Migrations

This folder contains EF Core migrations and the model snapshot.

## Current Migrations

- `20260401102254_InitialMusicPersistence`
- `20260401121744_UpdateModel_20260401`
- `20260402134541_AddGuildRepeatListItems`

`BotDbContextModelSnapshot.cs` reflects the latest schema model.

## Workflow

Common commands (run from solution root):

```bash
dotnet ef migrations add <MigrationName> --project "DC bot/DC bot.csproj"
dotnet ef database update --project "DC bot/DC bot.csproj"
```

## Runtime Behavior

`Program.cs` checks for pending migrations and applies them automatically during startup.

## Caution

- Keep generated migration files source-controlled.
- Do not manually edit designer files unless absolutely necessary.
