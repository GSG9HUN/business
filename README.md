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
- `DC bot tests/README.md` - test layout and commands
- `lavalink-server/README.md` - Lavalink configuration

## Build

```bash
dotnet restore "DC bot.sln"
dotnet build "DC bot.sln"
```

## Docker Compose

Docker Compose builds the bot from `DC bot/Dockerfile`, reads local secrets from the repository-root `.env` file, and passes them to the container as environment variables. Lavalink configuration is sourced from `lavalink-server/application.yaml`.

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
- DSharpPlus 5 event-handler registration through the service collection
- command and reaction handler activation

`DiscordClientEventHandler` receives dependencies directly through constructor injection. `BotServiceProviderFactory` configures Discord lifecycle/message/reaction events through DSharpPlus 5 builder APIs, while `BotHandlerRegistrar` enables the text command and reaction handlers after the service graph is built.
