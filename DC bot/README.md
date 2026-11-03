# DC bot

The Discord music-bot executable runs prefix commands, slash command adapters, reaction controls, and commands submitted by the mobile API. DSharpPlus handles Discord events, Lavalink4NET controls audio playback, and PostgreSQL stores queue, batch, playlist, and mobile-command state.

## Overview

- Music lifecycle code is organized into Connections, Sessions, Cleanup, Recovery, PersistentVoice, and Models. PlaybackLifecycleOptions belongs to the bot Configuration folder.
- The registered `24-7` text and slash commands share `TwentyFourSevenService` for initial voice setup and saved guild preferences. `PersistentVoiceRecoveryService` monitors connections and restores confirmed checkpoints; explicit leave disables the preference before owned cleanup.
- Public contracts document Discord adapter values, playback outcomes, validation results, and configuration; implementations inherit interface documentation where the contract is shared.
- Validate Discord user and voice context before music operations.
- Resolve music queries or provider URLs, persist queue entries, and coordinate track starts.
- Protect per-guild player generations and control reservations during concurrent commands, events, and recovery.
- Maintain saved playlists separately from the live queue and its canonical playback batch.
- Execute durable API-originated commands and persist their outcomes.
- Format English or Hungarian responses using guild language preferences.

Provider availability depends on the deployed Lavalink source managers, plugins, credentials, and rollout configuration. A recognized provider prefix does not guarantee that a source can currently load or play.

## Runtime Flow

```text
Discord message / slash interaction / reaction
  -> command or adapter
  -> injected service
  -> per-guild playback lifecycle + repository contracts
  -> Lavalink transport / PostgreSQL / Discord response

API request -> durable command row -> BotControlWorker
  -> context resolution + dispatch -> the same bot service boundaries
```

[DC bot.Contracts](../DC%20bot.Contracts/README.md) defines shared storage and command protocol values. [DC bot.Persistence](../DC%20bot.Persistence/README.md) implements the repositories. The bot does not host the mobile HTTP or SignalR endpoints; those belong to [API](../API/README.md).

## Run Locally

Build from the repository root with the .NET 9 SDK, then run from the bot project directory so relative translation paths resolve. Supply the Discord, Lavalink, and PostgreSQL configuration before starting:

```bash
dotnet restore "DC bot.sln"
dotnet build "DC bot.sln"
cd "DC bot"
dotnet run --project "DC bot.csproj"
```

`Program.cs` loads an optional `.env` from the current working directory without replacing values already present in the process environment. It also resolves `localization/` and `guildFiles/` relative to that directory. For the project-directory run above, provide process environment values or an optional `.env` in that directory; the bot does not search parent directories for the repository-root `.env`. Compose passes the root `.env` explicitly instead.

### Configuration

| Variable | Requirement or default | Purpose |
|---|---|---|
| `DISCORD_TOKEN` | Required | Discord bot authentication. |
| `BOT_PREFIX` | `!` | Prefix for message commands. |
| `LAVALINK_HOSTNAME` | Required | Lavalink host reachable from the bot process. |
| `LAVALINK_PORT` | `2333` | Node port. |
| `LAVALINK_SECURED` | `false` | Use TLS for node connectivity. |
| `LAVALINK_PASSWORD` | Empty when absent | Must match the deployed node authentication. |
| `MUSIC_JOIN_DEADLINE` | Required positive `TimeSpan` | Bound joining and readiness work. |
| `MUSIC_LEAVE_DEADLINE` | Required positive `TimeSpan` | Bound lifecycle cleanup work. |
| `POSTGRES_HOST` | `localhost` | Database host. |
| `POSTGRES_PORT` | `5432` | Database port. |
| `POSTGRES_DB` | `dc_bot` | Database name. |
| `POSTGRES_USER` | `postgres` | Database user. |
| `POSTGRES_PASSWORD` | `postgres` | Local fallback used by the loader; supply deployment credentials explicitly. |

Example deadline values are `00:00:30` and `00:00:15`; they are configuration examples rather than measured production recommendations. Both must fit the supported timer duration. Startup resolves these options before migration or Discord connection.

For local execution against Compose-published services, use the host addresses and ports. Inside the Compose network the service names are `postgres` and `lavalink`. Provider-side credentials and plugins are configured separately in the [Lavalink server](../lavalink-server/README.md).

### Docker Compose

The repository stack builds the bot from this project's Dockerfile:

```bash
docker compose up -d --build
```

Compose passes the root `.env` to the bot and waits for PostgreSQL and Lavalink health checks. Updating an existing container's environment requires recreation; `docker compose restart` keeps its current environment.

## Entry Point

### Program

**File:** `DC bot/Program.cs`

**Purpose:** Creates the process cancellation source, loads the optional working-directory environment file, and delegates to BotApplication.

**Method:**

```csharp
private static async Task Main();
```

**Behavior:** Console cancellation flows into application startup and the runtime workers. Localization and guild preference paths are relative to the process working directory.

---

## Startup and Shutdown

1. `Program` creates the shutdown token and loads the optional environment file.
2. `BotConfigurationLoader` reads bot, node, and database settings.
3. `BotServiceProviderFactory` composes logging, Discord, Lavalink, persistence, commands, and music services.
4. `BotApplication` validates playback deadlines and calls `DatabaseMigrationRunner`.
5. `BotHandlerRegistrar` enables the command and reaction handlers.
6. The Discord runtime, mobile command worker, and playback recovery loop run under linked cancellation.

The bot uses a service provider directly. Startup explicitly validates lifecycle options, and shutdown observes worker faults instead of hiding them as ordinary cancellation.

## Playback, Batches, and History

A guild has a live player generation and durable queue state. A start claim records operation ownership before the transport call. If the remote outcome is uncertain, recovery reconciles it before another start is allowed.

The active canonical batch retains its full sequence, including tracks added before the batch closes. Full-list repeat can therefore include already played items. Waiting queue position is separate from canonical batch position. Previous lookup stays within the active batch; the read-only `history` command lists recent batches without replaying them.

Track-end events are accepted only for the current correlated attempt. Skip explicitly completes its accepted attempt after stopping so a delayed SDK stop event cannot advance the queue twice. Cleanup reports local invalidation, transport, subscription, and persistence outcomes separately.

Gateway reconnection, Lavalink node readiness, and Discord voice readiness are separate conditions. Validation may confirm voice readiness using unchanged remote session evidence when the SDK connection flag lags. Persisted bot presence is not itself a readiness lock.

## Subfolders

### Commands/

Groups prefix-based text commands and slash command adapters.

**Files:**

- `README.md` — Overview and navigation to feature-specific source files.

**Purpose:** Text commands implement ICommand and reuse services and localized responses. Slash adapters enter the same command pipeline through SlashCommandExecutor.

---

### Configuration/

Defines typed options for bot identity, Lavalink connectivity, search resolution, and saved-playlist limits.

**Files:**

- `BotSettings.cs`
- `LavalinkSettings.cs`
- `PlaylistOptions.cs`
- `PlaybackLifecycleOptions.cs`
- `SearchResolverOptions.cs`

**Purpose:** Startup builds the runtime settings and registers options. PlaybackLifecycleOptions defines the join and cleanup deadlines used by the music lifecycle services.

---

### Constants/

Centralizes identifiers used to select localized messages and validation errors.

**Files:**

- `LocalizationKeys.cs`
- `ValidationErrorKeys.cs`

**Purpose:** Commands and services pass these keys to LocalizationService and ResponseBuilder instead of duplicating translation strings.

---

### Exceptions/

Defines the common exception boundary and feature-specific failure categories.

**Files:**

- `BotException.cs`

**Purpose:** Services raise typed failures where callers need operation context; command boundaries log failures and produce localized responses.

---

### guildFiles/

Groups local files used for guild-specific preferences.

**Files:**

- `README.md` — Overview and navigation to feature-specific source files.

**Purpose:** The localization subfolder holds guild language selections managed by LocalizationService through IFileSystem.

---

### Helper/

Contains small adapters and utilities used by command and presentation boundaries.

**Files:**

- `DiscordTextSanitizer.cs`

**Purpose:** DiscordTextSanitizer prepares text for Discord output; factories wrap SDK messages and validation models carry normalized outcomes.

---

### Interface/

Defines command and adapter boundaries used inside the bot executable.

**Files:**

- `ICommand.cs`

**Purpose:** ICommand is the common entry point for registered text commands. Subfolders contain core validation, Discord wrappers, and service contracts.

---

### IO/

Implements the injected file-system boundary for translation and guild preference data.

**Files:**

- `PhysicalFileSystem.cs`

**Purpose:** PhysicalFileSystem performs the local file operations requested through IFileSystem.

---

### localization/

Contains the English and Hungarian message catalogs selected by LocalizationService.

**Files:**

- `README.md` — Overview and navigation to feature-specific source files.

**Purpose:** Constants identify translation keys; ResponseBuilder and commands format the selected text using guild preferences and fallback lookup.

---

### Logging/

Defines typed log messages and reusable scopes for bot operations.

**Files:**

- `LogExtensions.cs`
- `LoggingScopes.cs`

**Purpose:** LogExtensions uses source-generated logging methods; LoggingScopes attaches command, guild, channel, and user context where supplied.

---

### Properties/

Contains assembly-level metadata for supported internal test access.

**Files:**

- `AssemblyInfo.cs`

**Purpose:** AssemblyInfo.cs exposes internal implementation details to the test assembly without making those types public application APIs.

---

### Service/

Groups process lifecycle, localization, command orchestration, music, reactions, and slash execution.

**Files:**

- `BotService.cs`
- `LocalizationService.cs`

**Purpose:** BotService runs the Discord client; LocalizationService handles translations and guild language files. Feature folders own the larger workflows.

---

### Startup/

Loads runtime settings, composes dependencies, applies migrations, enables handlers, and observes shutdown.

**Files:**

- `BotApplication.cs`
- `BotConfigurationLoader.cs`
- `BotHandlerRegistrar.cs`
- `BotRuntimeSettings.cs`
- `BotServiceProviderFactory.cs`

**Purpose:** BotApplication validates lifecycle options before migrations, then runs the Discord service, mobile command worker, and playback recovery together under linked cancellation.

---

### Wrapper/

Groups Discord, Lavalink, and slash-interaction implementations of bot-facing contracts.

**Files:**

- `README.md` — Overview and navigation to feature-specific source files.

**Purpose:** The Discord wrappers expose SDK objects through application interfaces; Lavalink wrappers adapt track metadata; slash wrappers translate interaction responses.

---

## Verification and Maintenance

`Logging/EventIdTable.md` documents application event declarations and their visibility under the configured category filters. Direct lifecycle logs can carry additional operation identifiers outside that catalog.

Build the solution after changing a shared contract. Run relevant behavior tests from the [test project](../DC%20bot%20tests/README.md); methods marked `Skip` remain planned cases rather than implemented coverage. Live Discord/Lavalink tests require their own configured environment.

Use [AGENTS.md](../AGENTS.md) for implementation conventions. Keep folder documentation focused on current responsibilities and behavior. The [audit](../docs/dc-bot-agents-audit-2026-10-21.md) records findings and verification history separately.

## Related Components

- `docs/plans/music-command-roadmap.md` — Proposed text/slash music controls, audio effects, and lifecycle requirements; these proposals are not registered commands.
- `DC bot/` — Discord commands, live playback lifecycle, and the mobile command worker.
- `API/` — HTTP request handling, authentication, response mapping, and SignalR publication.
- `DC bot.Contracts/` — Shared storage interfaces, immutable models, command payloads, and protocol names.
- `DC bot.Persistence/` — PostgreSQL repository implementations, migrations, and database notifications.
- `DC bot tests/` — Behavior tests and explicitly skipped planned cases.
- `AGENTS.md` — Implementation and detailed README conventions.
