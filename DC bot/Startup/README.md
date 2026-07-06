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
5. start `BotService` with the provided shutdown `CancellationToken`

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

### DependencyInjection/

Domain-specific DI registration extensions used by the composition root:

- `LoggingServiceCollectionExtensions.cs` - `AddBotLogging()` configures console logging.
- `CoreServiceCollectionExtensions.cs` - `AddCoreBotServices(...)` registers bot settings, command registry, localization, validation, response building, wrappers, reaction components, handlers, and file-system services.
- `DiscordServiceCollectionExtensions.cs` - `AddDiscordRuntime(...)` creates the DSharpPlus client and wires gateway, message, reaction, and voice event callbacks.
- `LavalinkServiceCollectionExtensions.cs` - `AddLavalinkRuntime(...)` configures Lavalink4NET HTTP and WebSocket endpoints.
- `PersistenceServiceCollectionExtensions.cs` - `AddPersistenceServices(...)` registers EF Core and repositories.
- `CommandServiceCollectionExtensions.cs` - `AddCommandServices()` registers text commands, slash command services, slash modules, and `SlashCommandProcessor`.
- `MusicServiceCollectionExtensions.cs` - `AddMusicServices()` registers music playback, queue, repeat, notification, and progressive timer services.

### BotServiceProviderFactory.cs

The DI composition root. It validates required startup settings and composes the service graph through the extension methods in `Startup/DependencyInjection/`.

The graph includes:

- DSharpPlus `DiscordClient`
- DSharpPlus 5 socket/session/guild/voice/message/reaction/unknown-event handlers
- DSharpPlus Commands slash command processor and slash modules
- Lavalink4NET client services
- EF Core `BotDbContext` factory
- repositories
- command handlers and commands
- Discord message wrapper factory
- music services
- localization, validation, response, and file-system services

Discord event handlers are configured through DSharpPlus 5 builder APIs in `AddDiscordRuntime`. `BotHandlerRegistrar` only flips command/reaction handlers into their active state after the graph is built.

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

Gateway lifecycle, voice-state, message-created, and reaction events are already wired by `DiscordServiceCollectionExtensions.AddDiscordRuntime(...)`.

## Design Notes

- `Program.cs` stays responsible only for process-level bootstrapping.
- `BotServiceProviderFactory` remains the composition root, while detailed registrations stay grouped by runtime domain under `DependencyInjection/`.
- `CommandHandlerService` receives `IDiscordMessageFactory` through DI, so the DSharpPlus message wrapper boundary remains testable.
- `CommandHandlerService` and `HelpCommand` use `ICommandRegistry` for command lookup/enumeration instead of resolving commands from `IServiceProvider`.
- `ReactionHandlerService` delegates control-message creation, context construction, and emoji action dispatch to focused services.
- `BotApplication.RunAsync` and `BotService.StartAsync` accept `CancellationToken` values for controlled shutdown.
- `DiscordClientEventHandler` uses constructor injection, not service locator lookups, even though DSharpPlus resolves the handler from the service provider at event-dispatch time.
- `DiscordClientFactory` does not register event handlers and remains useful for tests/direct construction.
- Slash commands use `DSharpPlus.Commands` with `SlashCommandProcessor`; the legacy `DSharpPlus.SlashCommands` package is not used.
- Startup integration tests can resolve the full service graph without starting the bot.

## Related Components

- `../Program.cs` - process entry point
- `../PROGRAM_CS_README.md` - startup flow and environment details
- `DependencyInjection/README.md` - DI registration map
- `../Wrapper/DiscordClientFactory.cs` - direct/test Discord client creation outside the production DI startup path
- `../Wrapper/DiscordClientEventHandler.cs` - Discord lifecycle, voice, and gateway event handling
- `../Service/BotService.cs` - bot lifecycle runtime
