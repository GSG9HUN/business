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
4. enable command/reaction handlers
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

### BotServiceCollectionExtensions.cs

Domain-specific DI registration extensions used by the composition root:

- `AddDiscordRuntime` wires DSharpPlus client and lifecycle/message/reaction event callbacks.
- `AddSlashCommandProcessor` registers slash modules with `SlashCommandProcessor`.
- `AddLavalinkRuntime` configures Lavalink4NET.
- `AddPersistenceServices` registers EF Core and repositories.
- `AddCoreBotServices`, `AddTextCommands`, `AddSlashCommandServices`, and `AddMusicServices` register application services by domain.

### BotServiceProviderFactory.cs

The DI composition root. It validates required startup settings and composes the service graph through the domain-specific extension methods in `BotServiceCollectionExtensions.cs`.

The graph includes:

- DSharpPlus `DiscordClient`
- DSharpPlus 5 lifecycle/message/reaction event handlers
- DSharpPlus Commands slash command processor and slash modules
- Lavalink4NET client services
- EF Core `BotDbContext` factory
- repositories
- command handlers and commands
- Discord message wrapper factory
- music services
- localization, validation, response, and file-system services

Discord event handlers are configured through DSharpPlus 5 builder APIs in `AddDiscordRuntime`. `BotHandlerRegistrar` only activates the text command handler and reaction handler after the graph is built.

Current slash command modules are registered from:

- `Commands/SlashCommands/Music/`
- `Commands/SlashCommands/Queue/`
- `Commands/SlashCommands/Utility/`

### DatabaseMigrationRunner.cs

Creates a scoped `BotDbContext` and applies pending EF Core migrations with `MigrateAsync()`.

### BotHandlerRegistrar.cs

Activates runtime command/reaction handlers after the service graph has been built:

```csharp
commandHandler.RegisterHandler(discordClient);
reactionHandler.RegisterHandler(discordClient);
```

Lifecycle, message-created, and reaction events are already configured by `BotServiceCollectionExtensions.AddDiscordRuntime(...)`.

## Design Notes

- `Program.cs` stays responsible only for process-level bootstrapping.
- `BotServiceProviderFactory` remains the composition root, while detailed registrations stay grouped by runtime domain.
- `CommandHandlerService` receives `IDiscordMessageFactory` through DI, so the DSharpPlus message wrapper boundary remains testable.
- `DiscordClientEventHandler` uses constructor injection, not service locator lookups.
- `DiscordClientFactory` does not know about event handlers.
- Slash commands use `DSharpPlus.Commands` with `SlashCommandProcessor`; the legacy `DSharpPlus.SlashCommands` package is not used.
- Startup integration tests can resolve the full service graph without starting the bot.

## Related Components

- `../Program.cs` - process entry point
- `../PROGRAM_CS_README.md` - startup flow and environment details
- `../Wrapper/DiscordClientFactory.cs` - direct/test Discord client creation outside the production DI startup path
- `../Wrapper/DiscordClientEventHandler.cs` - Discord lifecycle event handling
- `../Service/BotService.cs` - bot lifecycle runtime
