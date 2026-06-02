# DC Bot - Discord Music Bot

A feature-rich Discord music bot built with DSharpPlus and Lavalink4NET, featuring queue management, multiple languages,
robust error handling, and comprehensive documentation.

## Quick Start

### Prerequisites

- .NET 9.0 SDK
- Lavalink server (audio processing)
- Discord bot token

### Setup

1. **Create `.env` file** in the repository root for local or Docker Compose runs, or provide the same keys as environment variables:

```env
DISCORD_TOKEN=your_bot_token_here
BOT_PREFIX=!

# Host dotnet run/tests against docker-compose: 127.0.0.1
# Bot running inside Docker Compose network: lavalink
LAVALINK_HOSTNAME=127.0.0.1
LAVALINK_PASSWORD=CHANGE_ME
LAVALINK_PORT=2333
LAVALINK_SECURED=false

# Host dotnet run/tests against docker-compose: 127.0.0.1
# Bot running inside Docker Compose network: postgres
POSTGRES_HOST=127.0.0.1
POSTGRES_PORT=5432
POSTGRES_DB=dc_bot
POSTGRES_USER=postgres
POSTGRES_PASSWORD=CHANGE_ME

SPOTIFY_CLIENT_ID=
SPOTIFY_CLIENT_SECRET=
APPLE_MUSIC_API_TOKEN=
DEEZER_ARL=
YANDEX_MUSIC_ACCESS_TOKEN=
```

2. **Build and run**:

```bash
dotnet restore "DC bot.sln"
dotnet build "DC bot.sln"
dotnet run --project "DC bot/DC bot.csproj"
```

---

## Table of Contents

1. [Project Structure](#project-structure)
2. [Key Features](#key-features)
3. [Architecture Overview](#architecture-overview)
4. [Application Startup](#application-startup)
5. [Environment Configuration](#environment-configuration)
6. [Service Registration & Dependency Injection](#service-registration--dependency-injection)
7. [Documentation Guide](#documentation-guide)
8. [Common Tasks](#common-tasks)
9. [Troubleshooting](#troubleshooting)

---

## Project Structure

```
DC bot/
├── Commands/                      # Text and slash command implementations
│   ├── TextCommands/              # Prefix-based text commands
│   │   ├── Music/                 # Playback control (play, pause, skip, etc.)
│   │   ├── Queue/                 # Queue management (shuffle, repeat, clear, etc.)
│   │   └── Utility/               # General commands (help, ping, language, tag)
│   ├── SlashCommands/             # Discord slash command adapters
│   │   ├── Music/                 # Playback slash commands
│   │   ├── Queue/                 # Queue slash commands
│   │   └── Utility/               # General slash commands
│   └── README.md
│
├── Exceptions/                    # Custom exception types
│   ├── Localization/              # Language file errors
│   ├── Messaging/                 # Discord message send failures
│   ├── Music/                     # Lavalink, queue, track loading
│   ├── Validation/                # Validation exception type
│   └── README.md
│
├── Helper/                        # Utility classes and result types
│   ├── Validation/                # Validation result models
│   ├── Factory/                   # Object creation factories
│   └── README.md
│
├── Interface/                     # Service and wrapper contracts (abstractions)
│   ├── Core/                      # ICommandHelper, IValidationService
│   ├── Discord/                   # Discord object abstractions
│   ├── Service/                   # Service contracts
│   │   ├── IO/                    # IFileSystem
│   │   ├── Localization/          # ILocalizationService
│   │   ├── Music/                 # ILavaLinkService, etc.
│   │   ├── Persistence/           # Repository contracts
│   │   ├── Presentation/          # IResponseBuilder
│   │   ├── SlashCommands/         # Slash command adapter contracts
│   │   └── README.md
│   ├── ICommand.cs
│   ├── ILavaLinkTrack.cs
│   └── README.md
│
├── IO/                            # File system abstraction
│   ├── PhysicalFileSystem.cs      # Production implementation
│   └── README.md
│
├── Service/                       # Business logic layer
│   ├── BotService.cs              # Bot lifecycle management
│   ├── LocalizationService.cs     # Multi-language support
│   ├── ReactionHandler.cs         # Music control reactions
│   ├── Core/                      # CommandHandler, Validation
│   ├── Music/                     # Playback orchestration
│   │   ├── LavaLinkService.cs
│   │   ├── TrackSearchResolverService.cs
│   │   ├── MusicServices/         # Granular music services
│   │   ├── ProgressiveTimer/      # Now-playing message timer updates
│   │   └── README.md
│   ├── Presentation/              # ResponseBuilder
│   ├── SlashCommands/             # Slash command executor
│   └── README.md
│
├── Startup/                       # Runtime composition and startup orchestration
│   ├── BotApplication.cs          # Application runtime flow
│   ├── BotConfigurationLoader.cs  # Environment configuration loading
│   ├── BotHandlerRegistrar.cs     # Command/reaction handler activation
│   ├── BotRuntimeSettings.cs      # Startup settings aggregate
│   ├── BotServiceCollectionExtensions.cs # Domain-specific DI registration extensions
│   ├── BotServiceProviderFactory.cs # Dependency injection composition root
│   ├── DatabaseMigrationRunner.cs # EF Core migration execution
│   └── README.md
│
├── Wrapper/                       # Discord API wrappers (DSharpPlus abstraction)
│   ├── DiscordMessageWrapper.cs
│   ├── DiscordUserWrapper.cs
│   ├── DiscordChannelWrapper.cs
│   ├── DiscordClientFactory.cs
│   ├── DiscordClientEventHandler.cs
│   ├── LavalinkTrackWrapper.cs
│   ├── SlashInteractionContextFactory.cs
│   ├── SlashInteractionContextWrapper.cs
│   ├── SlashInteractionMessageWrapper.cs
│   └── README.md
│
├── Configuration/                 # Configuration models (Options pattern)
│   ├── BotSettings.cs
│   ├── LavalinkSettings.cs
│   ├── SearchResolverOptions.cs
│   └── README.md
│
├── Constants/                     # Application-wide constants
│   ├── AppConstants.cs            # Localization keys
│   └── README.md
│
├── Logging/                       # Structured logging
│   ├── LogExtensions.cs           # Logging methods
│   ├── LoggingScopes.cs
│   ├── EventIdTable.md
│   └── README.md
│
├── Persistence/                   # EF Core + PostgreSQL persistence layer
│   ├── Db/                        # BotDbContext and factory
│   ├── Entities/                  # EF Core entities
│   ├── Configurations/            # EF model mapping
│   ├── Repositories/              # Repository implementations
│   ├── Migrations/                # DB schema migrations
│   └── README.md
│
├── Model/                         # Data models
│   ├── SerializedTrack.cs         # Lightweight track identity model
│   └── README.md
│
├── Properties/                    # Assembly metadata
│   ├── AssemblyInfo.cs
│   └── README.md
│
├── localization/                  # Language files
│   ├── eng.json                   # English translations
│   ├── hu.json                    # Hungarian translations
│   └── README.md
│
├── guildFiles/                    # Per-guild persistent data
│   ├── localization/              # Guild language preferences
│   └── README.md
│
├── Program.cs                     # Thin process entry point
├── PROGRAM_CS_README.md           # Program.cs and startup documentation
├── DC bot.csproj                  # Project file
└── README.md                      # This file

```

---

## Key Features

### 🎵 Music Playback

- **Multiple sources:** YouTube, YouTube Music, Spotify, SoundCloud, Apple Music, Deezer, Yandex Music, Bandcamp
- **Queue management:** Persistent queue storage per guild via PostgreSQL
- **Repeat modes:** Single track repeat, queue repeat
- **Playback controls:** Play, pause, resume, skip
- **Voice channel management:** Auto-join, disconnect, state tracking

### 🌍 Multi-Language Support

- **Languages:** English (default), Hungarian
- **Per-guild selection:** Each guild can choose preferred language
- **Extensible:** Easy to add new languages
- **Localization system:** Type-safe key constants

### 🎯 Command System

- **Text commands:** Traditional prefix-based (`!play`, `!pause`)
- **Slash commands:** Modern Discord interactions through DSharpPlus Commands
- **Comprehensive validation:** User, player, connection checks
- **Consistent error handling:** Localized error messages

### 🔧 Architecture

- **Clean layers:** Commands → Services → External APIs
- **Dependency injection:** Full DI throughout application
- **Interface-based design:** Testable and flexible
- **Wrapper pattern:** DSharpPlus abstraction
- **Repository pattern:** Database persistence abstraction

### 📊 Observability

- **Structured logging:** Event IDs for all operations
- **Execution tracking:** Command start/completion logging
- **Context preservation:** Log scopes with guild/user IDs
- **Error details:** Full exception information with context

---

## Architecture Overview

### Layer Diagram

```
┌─────────────────────────────────────┐
│          Discord (User)             │
└──────────────┬──────────────────────┘
               │ (Discord events)
┌──────────────▼──────────────────────┐
│     Commands (Presentation Layer)   │
│  ├─ Text commands (15)              │
│  ├─ Slash modules (14)              │
│  └─ Shared command pipeline         │
└──────────────┬──────────────────────┘
               │ (service calls)
┌──────────────▼──────────────────────┐
│  Services (Business Logic Layer)    │
│  ├─ LavaLinkService                 │
│  ├─ MusicQueueService               │
│  ├─ ValidationService               │
│  ├─ LocalizationService             │
│  └─ ResponseBuilder                 │
└──────────────┬──────────────────────┘
               │ (API calls)
┌──────────────▼──────────────────────┐
│ External APIs (Infrastructure Layer)│
│  ├─ Discord (DSharpPlus)            │
│  ├─ Lavalink (Audio)                │
│  ├─ PostgreSQL                      │
│  └─ File System                     │
└─────────────────────────────────────┘
```

### Design Patterns

| Pattern                  | Implementation                | Benefits                    |
|--------------------------|-------------------------------|-----------------------------|
| **Dependency Injection** | Constructor injection         | Loose coupling, testability |
| **Service Layer**        | Commands delegate to services | Separation of concerns      |
| **Repository**           | Queue persistence abstraction | Flexible storage            |
| **Wrapper/Adapter**      | DSharpPlus abstraction        | Library independence        |
| **Factory/Adapter**      | Wrapper and message creation  | Centralized boundary setup  |
| **Options**              | Configuration models          | Type-safe config            |
| **Result Objects**       | Validation results            | Exception-free errors       |

### SOLID Principles

- ✅ **Single Responsibility** - Each class has one reason to change
- ✅ **Open/Closed** - Extend via interfaces, not modification
- ✅ **Liskov Substitution** - All implementations substitutable
- ✅ **Interface Segregation** - Small, focused interfaces
- ✅ **Dependency Inversion** - Depend on abstractions

---

## Application Startup

### Overview

Startup is split between a thin process entry point and focused startup components:

1. `Program.cs` loads `.env` when present and delegates to `BotApplication`.
2. `BotConfigurationLoader` reads required bot and Lavalink settings from the environment.
3. `BotServiceProviderFactory` builds the Dependency Injection container.
4. `DatabaseMigrationRunner` applies pending EF Core migrations.
5. `BotHandlerRegistrar` activates command and reaction handlers.
6. `BotService` starts the Discord client lifecycle.

For detailed documentation, see **[PROGRAM_CS_README.md](PROGRAM_CS_README.md)** and **[Startup/README.md](Startup/README.md)**.

### Initialization Flow

```
Main()
  ↓
Load optional .env file and validate required environment variables
  ↓
BotApplication.RunAsync()
  ↓
BotConfigurationLoader.LoadFromEnvironment()
  ↓
BotServiceProviderFactory.Create()
  ├─ AddDiscordRuntime
  ├─ AddSlashCommandProcessor
  ├─ AddLavalinkRuntime
  ├─ AddBotLogging
  ├─ AddPersistenceServices
  ├─ AddCoreBotServices
  ├─ AddSlashCommandServices
  ├─ AddTextCommands (15 command implementations)
  ├─ AddMusicServices (15 music-related services)
  └─ Build ServiceProvider
  ↓
DatabaseMigrationRunner.ApplyMigrationsIfNeededAsync()
  ↓
BotHandlerRegistrar.RegisterHandlers()
  ├─ CommandHandlerService.RegisterHandler(discordClient)
  └─ ReactionHandler.RegisterHandler(discordClient)
  ↓
BotService.StartAsync()
  ├─ Connect DiscordClient
  └─ Run indefinitely
```

---

## Environment Configuration

### Configuration Loading Order

```
1. Hardcoded Defaults (in code)
   └─ Prefix = "!"
   └─ Port = 2333
   └─ Secured = false
   └─ Password = ""
        ↓
2. Environment Variables
   └─ Provided by `.env`, Docker Compose `env_file`, CI secrets, or the host process environment
        ↓
3. Validation
   └─ Check required: DISCORD_TOKEN, LAVALINK_HOSTNAME
   └─ Exit if missing
```

### .env File Example

```env
# Bot Settings (Required)
DISCORD_TOKEN=your_bot_token_here

# Bot Settings (Optional)
BOT_PREFIX=!

# Lavalink Settings (Required)
# Host dotnet run/tests against docker-compose: 127.0.0.1
# Bot running inside Docker Compose network: lavalink
LAVALINK_HOSTNAME=127.0.0.1

# Lavalink Settings (Optional)
LAVALINK_PORT=2333
LAVALINK_SECURED=false
LAVALINK_PASSWORD=your_lavalink_password

# PostgreSQL Settings (Optional)
# Host dotnet run/tests against docker-compose: 127.0.0.1
# Bot running inside Docker Compose network: postgres
POSTGRES_HOST=127.0.0.1
POSTGRES_PORT=5432
POSTGRES_DB=dc_bot
POSTGRES_USER=postgres
POSTGRES_PASSWORD=your_postgres_password

# Lavalink Provider Settings (Optional)
SPOTIFY_CLIENT_ID=
SPOTIFY_CLIENT_SECRET=
APPLE_MUSIC_API_TOKEN=
DEEZER_ARL=
YANDEX_MUSIC_ACCESS_TOKEN=
```

### Environment Variables

| Variable            | Required | Default | Purpose              |
|---------------------|----------|---------|----------------------|
| `DISCORD_TOKEN`     | ✅ Yes    | N/A     | Bot authentication   |
| `LAVALINK_HOSTNAME` | ✅ Yes    | N/A     | Lavalink server host |
| `BOT_PREFIX`        | ❌ No     | `!`     | Command prefix       |
| `LAVALINK_PORT`     | ❌ No     | `2333`  | Lavalink port        |
| `LAVALINK_SECURED`  | ❌ No     | `false` | Use HTTPS/WSS        |
| `LAVALINK_PASSWORD` | ❌ No     | ``      | Lavalink password    |
| `POSTGRES_HOST`     | ❌ No     | `localhost` | PostgreSQL host |
| `POSTGRES_PORT`     | ❌ No     | `5432` | PostgreSQL port |
| `POSTGRES_DB`       | ❌ No     | `dc_bot` | PostgreSQL database |
| `POSTGRES_USER`     | ❌ No     | `postgres` | PostgreSQL username |
| `POSTGRES_PASSWORD` | ❌ No     | `postgres` | PostgreSQL password |
| `SPOTIFY_CLIENT_ID` | ❌ No     | `` | Spotify client ID for Lavalink lavasrc |
| `SPOTIFY_CLIENT_SECRET` | ❌ No | `` | Spotify client secret for Lavalink lavasrc |
| `APPLE_MUSIC_API_TOKEN` | ❌ No | `` | Apple Music media API token for Lavalink lavasrc |
| `DEEZER_ARL` | ❌ No | `` | Deezer ARL cookie for Lavalink lavasrc |
| `YANDEX_MUSIC_ACCESS_TOKEN` | ❌ No | `` | Yandex Music access token for Lavalink lavasrc |

---

## Service Registration & Dependency Injection

### Runtime Registrations

`Startup/BotServiceProviderFactory.cs` is the composition root. It validates startup settings and composes runtime services through domain-specific methods in `Startup/BotServiceCollectionExtensions.cs`.

`BotServiceCollectionExtensions.cs` groups Discord runtime wiring, slash command registration, Lavalink configuration, persistence, core services, text commands, slash adapter services, and music services.

`Startup/BotHandlerRegistrar.cs` activates command/reaction handlers after the `DiscordClient` and dependent services are constructed. DSharpPlus event handlers are configured by `AddDiscordRuntime`.

#### Logging Configuration

- Console logging enabled
- Minimum level: Debug
- All services receive `ILogger<T>`

#### Lavalink Configuration

- HTTP/HTTPS automatic selection
- WebSocket (WS/WSS) automatic selection
- Server address configuration
- Authentication setup

#### Core Services

- `DiscordClient` - Discord connection
- `DiscordClientEventHandler` - Discord lifecycle event handling with direct constructor-injected dependencies
- `BotService` - Bot lifecycle
- `CommandHandlerService` - Message routing
- `ReactionHandler` - Reaction handling
- `IDiscordMessageFactory` - DSharpPlus message to `IDiscordMessage` wrapper boundary
- `IFileSystem` - File operations

#### Persistence Registrations

- `BotDbContext` factory - EF Core/PostgreSQL context creation
- `IGuildDataRepository` - Guild row and premium state
- `IPlaybackStateRepository` - Current playback state
- `IQueueRepository` - Queue entries
- `IRepeatListRepository` - Repeat-list entries

#### All 15 Text Commands

- 6 Music commands (play, pause, resume, skip, join, leave)
- 5 Queue commands (viewList, shuffle, repeat, repeatList, clear)
- 4 Utility commands (help, ping, language, tag)

#### Slash Commands

- Music: `/join`, `/play`, `/pause`, `/resume`, `/skip`, `/leave`
- Queue: `/queue`, `/shuffle`, `/repeat track`, `/repeat list`, `/clear`
- Utility: `/ping`, `/help`, `/tag`, `/language`
- Registered through `DSharpPlus.Commands` and `SlashCommandProcessor`
- Delegate to the existing text command pipeline through `ISlashCommandExecutor`

#### All 15 Music-Related Services

- `LavaLinkService` - Playback orchestration
- `MusicQueueService` - Queue management and repeat-list snapshot rehydration
- `RepeatService` - Repeat flags and repeat-list snapshot writes
- `CurrentTrackService` - Track state
- `TrackNotificationService` - Notifications
- `TrackFormatterService` - Formatting
- `PlayerConnectionService` - Voice connections
- `LavalinkNodeConnectionService` - Lavalink node startup guard
- `PlaybackEventHandlerService` - Event handling
- `PlaybackControlService` - Pause, resume, skip, and leave orchestration
- `PlaybackRequestService` - URL/query playback request orchestration
- `TrackPlaybackService` - Playback control
- `TrackEndedHandlerService` - Track end events
- `TrackSearchResolverService` - URL/query resolution
- `ProgressiveTimerService` - Now-playing message timer updates

#### Validation & Localization (5 services)

- `IValidationService` → `ValidationService`
- `IUserValidationService` → `ValidationService`
- `ILocalizationService` → `LocalizationService`
- `IResponseBuilder` → `ResponseBuilder`
- `ICommandHelper` → `CommandValidationService`

---

## Documentation Guide

The project has **50+ README.md files** documenting every component:

### Start Here

1. **[Commands/README.md](Commands/README.md)** - Understand command system
    - Command architecture
    - Text vs. slash commands
    - Adding new commands

2. **[Service/README.md](Service/README.md)** - Business logic layer
    - Service patterns
    - Core services (CommandHandler, Validation)
    - Music services architecture

3. **[Interface/README.md](Interface/README.md)** - Architecture abstractions
    - Why interfaces matter
    - Service contracts
    - Discord wrapper contracts

4. **[PROGRAM_CS_README.md](PROGRAM_CS_README.md)** - Application startup
    - Environment variables
    - Service registration details
    - Initialization flow

5. **[Startup/README.md](Startup/README.md)** - Startup composition
    - Configuration loading
    - DI registration
    - Discord event wiring

### Detailed Documentation

- **Exceptions/** - Exception types and error handling
- **Helper/** - Validation result types and factories
- **IO/** - File system abstraction
- **Startup/** - Runtime composition and startup orchestration
- **Wrapper/** - Discord API wrapper implementation
- **Configuration/** - Configuration models
- **Constants/** - Localization keys
- **Logging/** - Structured logging
- **Model/** - Data models
- **localization/** - Language files
- **guildFiles/** - Persistent data structure

---

## Common Tasks

### Adding a New Text Command

1. **Create command class** in the matching `Commands/TextCommands/<domain>/` folder:

```csharp
public class MyCommand(
    IMyService service,
    IResponseBuilder responseBuilder,
    ILogger<MyCommand> logger) : ICommand
{
    public string Name => "mycommand";
    public string Description => "Does something cool";

    public async Task ExecuteAsync(IDiscordMessage message)
    {
        logger.CommandInvoked(Name);
        
        // Validate input
        // Call service
        // Send response
        
        logger.CommandExecuted(Name);
    }
}
```

2. **Register in `Startup/BotServiceCollectionExtensions.cs` inside `AddTextCommands`:**

```csharp
.AddSingleton<ICommand, MyCommand>()
```

3. **Add localization keys** in `Constants/AppConstants.cs`:

```csharp
public const string MyCommandDescription = "mycommand_description";
```

4. **Add translations** in `localization/*.json`:

```json
{
  "mycommand_description": "Does something cool"
}
```

5. **Write unit tests** in `DC bot tests/`

### Adding a New Slash Command

1. Create the slash adapter in the matching `Commands/SlashCommands/<domain>/` folder.
2. Delegate to the existing text command behavior through `ISlashCommandExecutor` when the command has a text command equivalent.
3. Register the module in `Startup/BotServiceCollectionExtensions.cs` both in `AddSlashCommandProcessor` and `AddSlashCommandServices`.
4. Add or update the slash README in that command domain.
5. Write unit tests for module delegation, integration tests for DI/module resolution, and E2E-category pipeline tests for the slash adapter path.

### Adding a New Language

1. Create `localization/xx.json` (e.g., `de.json` for German)
2. Copy structure from `eng.json`
3. Translate all values
4. Add the language code to `LanguageCommand.AllowedLanguageCodes`
5. Add or update the `/language` slash choices when the language should be selectable through Discord slash UX
6. Test with `!language xx` and `/language`

### Adding a New Service

1. Create interface in `Interface/Service/`
2. Implement in `Service/`
3. Register in the matching domain method in `Startup/BotServiceCollectionExtensions.cs`:
   ```csharp
   .AddSingleton<IMyService, MyService>()
   ```
4. Inject where needed
5. Write unit tests

### Adding Validation

1. Create result type in `Helper/Validation/`
2. Add method to appropriate service
3. Use in commands via validation result

---

## Running the Bot

### Prerequisites

- .NET 9.0 SDK
- Lavalink server running and accessible
- Discord bot token (from Discord Developer Portal)
- `.env` file in the repository root, or equivalent environment variables

### Build and Run

```bash
# Restore dependencies
dotnet restore

# Build project
dotnet build

# Run bot
dotnet run --project "DC bot/DC bot.csproj"
```

### Deployment (Release Build)

```bash
# Publish for production
dotnet publish "DC bot/DC bot.csproj" -c Release

# Run published output
dotnet "DC bot/bin/Release/net9.0/publish/MelodiasMario.dll"
```

---

## Testing

```bash
# Run all tests
dotnet test

# Run with coverage
dotnet test --collect:"XPlat Code Coverage"
```

**Test structure:**

```
DC bot tests/
├── UnitTests/
│   ├── Commands/
│   ├── Service/
│   └── Model/
├── IntegrationTests/
│   ├── Commands/
│   ├── Persistence/
│   └── Service/
└── EndToEndTests/
    ├── Commands/
    ├── Service/
    └── Wrapper/
```

---

## Troubleshooting

### "DISCORD_TOKEN is not set"

**Cause:** `DISCORD_TOKEN` missing or empty in `.env` or the process environment

**Solution:** Add `DISCORD_TOKEN=your_token` to `.env`, Docker Compose `env_file`, CI secrets, or the host environment

### "LAVALINK_HOSTNAME is not set"

**Cause:** `LAVALINK_HOSTNAME` missing or empty in `.env` or the process environment

**Solution:** Add `LAVALINK_HOSTNAME=your_host` to `.env`, Docker Compose `env_file`, CI secrets, or the host environment

### Bot connects but commands don't work

**Cause:** Command prefix doesn't match or bot lacks permissions

**Solution:**

- Verify `BOT_PREFIX` in `.env` or the process environment (default: `!`)
- Check bot has `MESSAGE_CONTENT` intent enabled
- Verify bot has message permissions in Discord

### Lavalink connection fails

**Cause:** Wrong hostname, port, password, or server offline

**Solution:**

- Verify Lavalink server is running
- Check `LAVALINK_HOSTNAME`, `LAVALINK_PORT`, `LAVALINK_PASSWORD`
- Verify `LAVALINK_SECURED` matches server configuration

### Music doesn't play

**Cause:** Bot not in voice channel or queue empty

**Solution:**

- Join voice channel first
- Use `!play <query>` to add track
- Check logs for errors

### Queue not persisting

**Cause:** PostgreSQL connection issue or pending migrations

**Solution:**

- Ensure PostgreSQL is running and database environment settings are correct
- Check that all EF Core migrations have been applied (bot applies them automatically on startup)
- Check logs for database errors

---

## Performance Characteristics

| Metric             | Value     | Notes                 |
|--------------------|-----------|-----------------------|
| Memory (idle)      | ~50-100MB | Base overhead         |
| Memory (per guild) | ~20MB     | With active music     |
| CPU (idle)         | <1%       | Waiting for events    |
| CPU (per guild)    | ~5%       | During playback       |
| Disk I/O           | Minimal   | Localization and guild language files |
| Database I/O       | Moderate  | Queue, repeat, playback state, and premium data |

---

## Security

- ✅ Bot token provided by `.env` or environment variables; `.env` is ignored by git
- ✅ `.env.example` documents required keys without storing secrets
- ✅ Input validation on all commands
- ✅ Rate limiting via Discord API
- ✅ No sensitive data in logs
- ✅ File access is isolated behind `IFileSystem` for testability

---

## Contributing

### Code Style

- Use C# naming conventions
- Async methods end with `Async`
- Interfaces start with `I`
- Use `var` when type is obvious
- Keep public APIs small and covered by nearby README documentation

### Documentation

- Update README in modified folder
- Update PROGRAM_CS_README.md for changes
- Add localization keys for user text

---

## Version Information

- **.NET:** 9.0
- **DSharpPlus:** 5.0.0-nightly-02574
- **Lavalink4NET:** 4.2.1
- **EF Core:** 9.0.10
- **PostgreSQL provider:** Npgsql.EntityFrameworkCore.PostgreSQL 9.0.4
- **Last Updated:** 2026-06-01

---

## Quick Links

- **[PROGRAM_CS_README.md](PROGRAM_CS_README.md)** - Program.cs and startup documentation
- **[Startup/README.md](Startup/README.md)** - Startup composition
- **[Commands/README.md](Commands/README.md)** - Command architecture
- **[Service/README.md](Service/README.md)** - Service layer documentation
- **[Interface/README.md](Interface/README.md)** - Interface abstractions
- **GitHub Issues** - Report bugs or request features


