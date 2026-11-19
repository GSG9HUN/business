# business

Contains the .NET Discord music bot, mobile API, Kotlin mobile application, shared storage projects, tests and runtime configuration.

## Overview

- Contains the Discord music bot, authenticated mobile API, shared contracts and PostgreSQL persistence.
- Includes a Kotlin multiplatform mobile application and the supporting Lavalink/Docker configuration.
- Separates runtime projects, automated tests and technical documentation.

## Subfolders

### DC bot/

Runs the Discord music bot, command pipeline, and playback lifecycle.

**Files:**

- `DC bot.csproj`
- `Program.cs`

**Purpose:** Runs the Discord music bot, command pipeline, and playback lifecycle. Discord replies and channel notifications use localized embeds; long queue, playlist, help, favorite, lyrics and custom-preset results use caller-bound navigation buttons. Rejected embeds use bounded text fallbacks, including unknown commands and long lists; search selection preserves its menu and cancel button.

---

### API/

Exposes authenticated mobile HTTP and realtime operations through shared storage contracts.

**Files:**

- `API.csproj`
- `Program.cs`

**Purpose:** Exposes authenticated mobile HTTP and realtime operations through shared storage contracts. Guild-scoped requests require membership and active premium; denials return HTTP 403 with `PremiumRequired` and `not premium member`.

---

### DC bot.Contracts/

Defines shared persistence and mobile command contracts.

**Files:**

- `DC bot.Contracts.csproj`
- `README.md`

**Purpose:** Defines shared persistence and mobile command contracts.

---

### DC bot.Persistence/

Implements PostgreSQL repositories, EF mappings, and migrations.

**Files:**

- `DC bot.Persistence.csproj`
- `Db/BotDbContext.cs`

**Purpose:** Implements PostgreSQL repositories, EF mappings, and migrations.

---

### DC bot tests/

Contains unit, integration, and E2E test projects and scenarios.

**Files:**

- `DC bot tests.csproj`
- `README.md`

**Purpose:** Contains unit, integration, and E2E test projects and scenarios.

---

### MobileApp/

Contains the Kotlin multiplatform client and platform applications.

**Files:**

- `settings.gradle.kts`
- `build.gradle.kts`

**Purpose:** Authenticates users, displays guild/player state and submits commands to the API.

---

### lavalink-server/

Configures the external Lavalink audio server.

**Files:**

- `application.yaml`
- `README.md`

**Purpose:** Configures the external Lavalink audio server.

---

## Development and Runtime

### Build

```bash
dotnet restore "DC bot.sln"
dotnet build "DC bot.sln"
```

---

### Docker Compose

Docker Compose builds the bot from `DC bot/Dockerfile`, reads local secrets from the repository-root `.env` file, and passes them to the container as environment variables. PostgreSQL and Lavalink host ports are bound to `127.0.0.1`; inside the Compose network the bot reaches them as `postgres` and `lavalink`. Lavalink configuration is sourced from `lavalink-server/application.yaml`.

---

### Test

```bash
dotnet test "DC bot tests/DC bot tests.csproj" --filter "Category=Unit"
dotnet test "DC bot tests/DC bot tests.csproj" --filter "Category=Integration"
dotnet test "DC bot tests/DC bot tests.csproj" --filter "Category!=E2E"
```

External E2E tests require real Discord/Lavalink configuration. Local pipeline E2E tests use isolated dependencies; the non-E2E command excludes both categories.

---

### Repository Hygiene

Do not commit local secrets, IDE state, build output, test result artifacts, coverage output, or scratch notes. The root `.gitignore` covers `.env`, `.env.*`, `bin/`, `obj/`, `TestResults/`, coverage files, IDE folders, and local Docker Compose overrides. `.dockerignore` keeps the same local artifacts out of Docker build context. Keep `.env.example`, source files, migrations, localization files, workflow files, and README files tracked.

---

### Application Startup

`Program.cs` delegates application startup to the runtime composition layer. Runtime startup responsibilities live under `DC bot/Startup/`:

- configuration loading
- service provider creation
- migration execution
- DSharpPlus 5 gateway/message/reaction/voice event-handler registration through `Startup/DependencyInjection/DiscordServiceCollectionExtensions.cs`
- command and reaction handler activation through `BotHandlerRegistrar`
- cancellation-token based runtime shutdown from `Program.cs` through `BotApplication` to `BotService`

`DiscordClientEventHandler` receives dependencies directly through constructor injection. `CommandHandlerService` and `HelpCommand` use `ICommandRegistry` instead of command service-location. `BotServiceProviderFactory` composes focused DI modules under `Startup/DependencyInjection/`; `AddDiscordRuntime` wires Discord socket/session/guild/voice/message/reaction callbacks through DSharpPlus 5 builder APIs, while `BotHandlerRegistrar` enables the text command and reaction handlers after the service graph is built. Track identity persistence is centralized behind `ITrackSerializer`; queue item states use the explicit `QueueItemState` enum; saved playlists are persisted through playlist repository records and EF Core playlist entities.

---

### Queue and Playlist Model

Playlists use guild playback ownership and canonical batches. Canonical BatchId/BatchItemId remain long database keys; separate EnqueueGroupId values retain frontend grouping. Queue edits update canonical repeat items by stable ID, preserving history and duplicate tracks. Queue snapshot saving, cover artwork, limits, and mobile history remain available.

---

## Related Components

- `docs/plans/music-command-roadmap.md` - Bot feature proposals and deployment verification backlog.
- `docs/plans/mobile-music-parity-roadmap.md` - API, command bridge and Android/iOS integration backlog.
- `docker-compose.yaml` - Local runtime stack.
- `readme_required_tests.md` - Required verification matrix and test policy.
- `DC bot/Startup/README.md` - Runtime composition and database migration startup.
- `DC bot tests/README.md` - Test layout and verification commands.
