# Startup

This folder contains the runtime composition layer for the bot.

## Overview

Startup code is separated from `Program.cs` so configuration loading, dependency injection, migrations, and runtime event wiring can be tested independently.

## Files

### BotApplication.cs

Coordinates the application startup sequence:

1. load runtime settings
2. create the DI container
3. apply pending database migrations
4. register Discord handlers
5. start `BotService`

### BotConfigurationLoader.cs

Reads environment variables and builds `BotRuntimeSettings`.

Required values:

- `DISCORD_TOKEN`
- `LAVALINK_HOSTNAME`

Optional values:

- `BOT_PREFIX`
- `LAVALINK_PORT`
- `LAVALINK_SECURED`
- `LAVALINK_PASSWORD`
- `POSTGRES_HOST`
- `POSTGRES_PORT`
- `POSTGRES_DB`
- `POSTGRES_USER`
- `POSTGRES_PASSWORD`

### BotRuntimeSettings.cs

Small aggregate record that carries:

- `BotSettings`
- `LavalinkSettings`
- PostgreSQL connection string

### BotServiceProviderFactory.cs

The DI composition root.

It registers:

- DSharpPlus `DiscordClient`
- Lavalink4NET client services
- EF Core `BotDbContext` factory
- repositories
- command handlers and commands
- music services
- localization, validation, response, and file-system services

`DiscordClientFactory` is used only for client creation. Discord event subscription happens later in `BotHandlerRegistrar`.

### DatabaseMigrationRunner.cs

Creates a scoped `BotDbContext` and applies pending EF Core migrations with `MigrateAsync()`.

### BotHandlerRegistrar.cs

Wires runtime handlers after the service graph has been built:

```csharp
discordClient.Ready += eventHandler.OnClientReady;
discordClient.GuildAvailable += eventHandler.OnGuildAvailable;
commandHandler.RegisterHandler(discordClient);
reactionHandler.RegisterHandler(discordClient);
```

This avoids constructing `DiscordClientEventHandler` while `DiscordClient` is still being created.

## Design Notes

- `Program.cs` stays responsible only for process-level bootstrapping.
- `DiscordClientEventHandler` uses constructor injection, not service locator lookups.
- `DiscordClientFactory` does not know about event handlers.
- Startup integration tests can resolve the full service graph without starting the bot.

## Related Components

- `../Program.cs` - process entry point
- `../PROGRAM_CS_README.md` - startup flow and environment details
- `../Wrapper/DiscordClientFactory.cs` - Discord client creation
- `../Wrapper/DiscordClientEventHandler.cs` - Discord lifecycle event handling
- `../Service/BotService.cs` - bot lifecycle runtime
