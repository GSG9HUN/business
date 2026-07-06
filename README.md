# DC Bot Repository

This repository contains a .NET 9 Discord music bot, its test project, Docker/Lavalink support files, and project documentation.

## Projects

- `DC bot/` - bot application source
- `DC bot tests/` - unit, integration, and E2E tests
- `lavalink-server/` - Lavalink server configuration
- `docker-compose.yaml` - local runtime stack
- `readme_required_tests.md` - required verification matrix and test policy

## Start Here

- `DC bot/README.md` - application architecture and setup
- `DC bot/PROGRAM_CS_README.md` - process entry point and startup flow
- `DC bot/Startup/README.md` - runtime composition, DI, migrations, DSharpPlus event wiring, and handler activation
- `DC bot/Startup/DependencyInjection/README.md` - domain-specific service registration map
- `DC bot tests/README.md` - test layout and commands
- `lavalink-server/README.md` - Lavalink configuration

## Build

```bash
dotnet restore "DC bot.sln"
dotnet build "DC bot.sln"
```

## Docker Compose

Docker Compose builds the bot from `DC bot/Dockerfile`, reads local secrets from the repository-root `.env` file, and passes them to the container as environment variables. PostgreSQL and Lavalink host ports are bound to `127.0.0.1`; inside the Compose network the bot reaches them as `postgres` and `lavalink`. Lavalink configuration is sourced from `lavalink-server/application.yaml`.

## Test

```bash
dotnet test "DC bot tests/DC bot tests.csproj" --filter "Category=Unit"
dotnet test "DC bot tests/DC bot tests.csproj" --filter "Category=Integration"
dotnet test "DC bot tests/DC bot tests.csproj" --filter "Category!=E2E"
```

E2E tests require real Discord/Lavalink configuration and are excluded from the normal non-E2E verification command.

## Repository Hygiene

Do not commit local secrets, IDE state, build output, test result artifacts, coverage output, or scratch notes. The root `.gitignore` covers `.env`, `.env.*`, `bin/`, `obj/`, `TestResults/`, coverage files, IDE folders, and local Docker Compose overrides. `.dockerignore` keeps the same local artifacts out of Docker build context. Keep `.env.example`, source files, migrations, localization files, workflow files, and README files tracked.

## Startup Refactor Notes

`Program.cs` is now a thin entry point. Runtime startup responsibilities live under `DC bot/Startup/`:

- configuration loading
- service provider creation
- migration execution
- DSharpPlus 5 gateway/message/reaction/voice event-handler registration through `Startup/DependencyInjection/DiscordServiceCollectionExtensions.cs`
- command and reaction handler activation through `BotHandlerRegistrar`
- cancellation-token based runtime shutdown from `Program.cs` through `BotApplication` to `BotService`

`DiscordClientEventHandler` receives dependencies directly through constructor injection. `CommandHandlerService` and `HelpCommand` use `ICommandRegistry` instead of command service-location. `BotServiceProviderFactory` composes focused DI modules under `Startup/DependencyInjection/`; `AddDiscordRuntime` wires Discord socket/session/guild/voice/message/reaction callbacks through DSharpPlus 5 builder APIs, while `BotHandlerRegistrar` enables the text command and reaction handlers after the service graph is built. Track identity persistence is centralized behind `ITrackSerializer`, and queue item states use the explicit `QueueItemState` enum in code.
