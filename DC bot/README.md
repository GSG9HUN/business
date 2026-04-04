# DC Bot - Discord Music Bot

A feature-rich Discord music bot built with DSharpPlus and Lavalink4NET, featuring queue management, multiple languages,
robust error handling, and comprehensive documentation.

## Quick Start

### Prerequisites

- .NET 9.0 SDK
- Lavalink server (audio processing)
- Discord bot token

### Setup

1. **Create `.env` file** in project root:

```env
DISCORD_TOKEN=your_bot_token_here
BOT_PREFIX=!
LAVALINK_HOSTNAME=lavalinkv4.serenetia.com
LAVALINK_PASSWORD=your_password
LAVALINK_PORT=443
LAVALINK_SECURED=true
POSTGRES_HOST=localhost
POSTGRES_PORT=5432
POSTGRES_DB=dc_bot
POSTGRES_USER=postgres
POSTGRES_PASSWORD=postgres
```

2. **Build and run**:

```bash
dotnet restore
dotnet build
dotnet run
```

---

## Table of Contents

1. [Project Structure](#project-structure)
2. [Key Features](#key-features)
3. [Architecture Overview](#architecture-overview)
4. [Application Entry Point (Program.cs)](#application-entry-point-programcs)
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
│   ├── Music/                     # Playback control (play, pause, skip, etc.)
│   ├── Queue/                     # Queue management (shuffle, repeat, clear, etc.)
│   ├── SlashCommands/             # Modern Discord slash commands (disabled)
│   ├── Utility/                   # General commands (help, ping, language, tag)
│   └── README.md
│
├── Exceptions/                    # Custom exception types
│   ├── Localization/              # Language file errors
│   ├── Messaging/                 # Discord message send failures
│   ├── Music/                     # Lavalink, queue, track loading
│   ├── Validation/                # Validation errors (currently unused)
│   └── README.md
│
├── Helper/                        # Utility classes and result types
│   ├── Validation/                # Validation result models
│   ├── Factory/                   # Object creation factories
│   ├── SlashCommandResponseHelper.cs
│   └── README.md
│
├── Interface/                     # Service and wrapper contracts (abstractions)
│   ├── Core/                      # ICommandHelper, IValidationService
│   ├── Discord/                   # Discord object abstractions
│   ├── Service/                   # Service contracts
│   │   ├── IO/                    # IFileSystem
│   │   ├── Localization/          # ILocalizationService
│   │   ├── Music/                 # ILavaLinkService, etc.
│   │   ├── Presentation/          # IResponseBuilder
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
│   │   └── README.md
│   ├── Presentation/              # ResponseBuilder
│   └── README.md
│
├── Wrapper/                       # Discord API wrappers (DSharpPlus abstraction)
│   ├── DiscordMessage/UserWrapper.cs
│   ├── DiscordChannelWrapper.cs
│   ├── DiscordClientFactory.cs
│   ├── DiscordClientEventHandler.cs
│   ├── LavalinkTrackWrapper.cs
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
│   ├── queues/                    # Legacy queue files (DB is active path)
│   └── README.md
│
├── Program.cs                     # Application entry point
├── PROGRAM_CS_README.md           # Program.cs detailed documentation
├── DC bot.csproj                  # Project file
└── README.md                      # This file

```

---

## Key Features

### 🎵 Music Playback

- **Multiple sources:** YouTube, Spotify, SoundCloud, Apple Music, Deezer, Yandex Music
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
- **Slash commands:** Modern Discord interactions (currently disabled)
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
│  ├─ PlayCommand                     │
│  ├─ PauseCommand                    │
│  ├─ SkipCommand                     │
│  └─ ... (15 text commands)          │
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
| **Factory**              | Discord client creation       | Centralized setup           |
| **Options**              | Configuration models          | Type-safe config            |
| **Result Objects**       | Validation results            | Exception-free errors       |

### SOLID Principles

- ✅ **Single Responsibility** - Each class has one reason to change
- ✅ **Open/Closed** - Extend via interfaces, not modification
- ✅ **Liskov Substitution** - All implementations substitutable
- ✅ **Interface Segregation** - Small, focused interfaces
- ✅ **Dependency Inversion** - Depend on abstractions

---

## Application Entry Point (Program.cs)

### Overview

`Program.cs` is responsible for:

1. Loading environment variables from `.env` file
2. Creating configuration objects
3. Configuring the Dependency Injection container
4. Registering event handlers
5. Starting the bot

For detailed documentation, see **[PROGRAM_CS_README.md](PROGRAM_CS_README.md)**

### Initialization Flow

```
Main()
  ↓
Load .env file
  ↓
Parse environment variables
  ↓
Create BotSettings & LavalinkSettings
  ↓
ConfigureServices()
  ├─ Add Logging
  ├─ Configure Lavalink
  ├─ Register Core Services
  ├─ Register All Commands (15 text commands)
  ├─ Register Music Services (11 specialized services)
  ├─ Register Validation & Localization
  └─ Build ServiceProvider
  ↓
RegisterHandlers()
  ├─ CommandHandlerService.RegisterHandler()
  └─ ReactionHandler.RegisterHandler()
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
2. Environment Variables (.env file)
   └─ Override defaults
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
LAVALINK_HOSTNAME=lavalinkv4.serenetia.com

# Lavalink Settings (Optional)
LAVALINK_PORT=443
LAVALINK_SECURED=true
LAVALINK_PASSWORD=your_lavalink_password
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

---

## Service Registration & Dependency Injection

### Total Services Registered: 43

#### Logging Configuration

- Console logging enabled
- Minimum level: Debug
- All services receive `ILogger<T>`

#### Lavalink Configuration

- HTTP/HTTPS automatic selection
- WebSocket (WS/WSS) automatic selection
- Server address configuration
- Authentication setup

#### Core Services (5 services)

- `DiscordClient` - Discord connection
- `BotService` - Bot lifecycle
- `CommandHandlerService` - Message routing
- `ReactionHandler` - Reaction handling
- `IFileSystem` - File operations

#### All 15 Text Commands

- 6 Music commands (play, pause, resume, skip, join, leave)
- 5 Queue commands (viewList, shuffle, repeat, repeatList, clear)
- 4 Utility commands (help, ping, language, tag)

#### All 11 Music Services

- `LavaLinkService` - Playback orchestration
- `MusicQueueService` - Queue management
- `RepeatService` - Repeat logic
- `CurrentTrackService` - Track state
- `TrackNotificationService` - Notifications
- `TrackFormatterService` - Formatting
- `PlayerConnectionService` - Voice connections
- `PlaybackEventHandlerService` - Event handling
- `TrackPlaybackService` - Playback control
- `TrackEndedHandlerService` - Track end events
- `TrackSearchResolverService` - URL/query resolution

#### Validation & Localization (5 services)

- `IValidationService` → `ValidationService`
- `IUserValidationService` → `ValidationService`
- `ILocalizationService` → `LocalizationService`
- `IResponseBuilder` → `ResponseBuilder`
- `ICommandHelper` → `CommandValidationService`

---

## Documentation Guide

The project has **40+ README.md files** documenting every component:

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

### Detailed Documentation

- **Exceptions/** - Exception types and error handling
- **Helper/** - Validation result types and factories
- **IO/** - File system abstraction
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

1. **Create command class** in `Commands/` folder:

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

2. **Register in Program.cs:**

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

### Adding a New Language

1. Create `localization/xx.json` (e.g., `de.json` for German)
2. Copy structure from `eng.json`
3. Translate all values
4. Test with `!language xx` command

### Adding a New Service

1. Create interface in `Interface/Service/`
2. Implement in `Service/`
3. Register in `Program.cs`:
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
- `.env` file in project root

### Build and Run

```bash
# Restore dependencies
dotnet restore

# Build project
dotnet build

# Run bot
dotnet run
```

### Deployment (Release Build)

```bash
# Publish for production
dotnet publish -c Release

# Run published executable
./bin/Release/net9.0/DC bot.exe
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
└── IntegrationTests/
    ├── Service/
    └── Wrapper/
```

---

## Troubleshooting

### "Please provide .env file."

**Cause:** `.env` file not found

**Solution:** Create `.env` in project root with required variables

### "DISCORD_TOKEN is not set"

**Cause:** `DISCORD_TOKEN` missing or empty in `.env`

**Solution:** Add `DISCORD_TOKEN=your_token` to `.env`

### "LAVALINK_HOSTNAME is not set"

**Cause:** `LAVALINK_HOSTNAME` missing or empty

**Solution:** Add `LAVALINK_HOSTNAME=your_host` to `.env`

### Bot connects but commands don't work

**Cause:** Command prefix doesn't match or bot lacks permissions

**Solution:**

- Verify `BOT_PREFIX` in `.env` (default: `!`)
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

**Cause:** File system permissions or `guildFiles/` directory missing

**Solution:**

- Ensure bot has write permissions
- Directory is auto-created if missing
- Check disk space

---

## Performance Characteristics

| Metric             | Value     | Notes                 |
|--------------------|-----------|-----------------------|
| Memory (idle)      | ~50-100MB | Base overhead         |
| Memory (per guild) | ~20MB     | With active music     |
| CPU (idle)         | <1%       | Waiting for events    |
| CPU (per guild)    | ~5%       | During playback       |
| Disk I/O           | Minimal   | Queue save on changes |

---

## Security

- ✅ Bot token in `.env` (not in source control)
- ✅ Input validation on all commands
- ✅ Rate limiting via Discord API
- ✅ No sensitive data in logs
- ✅ File system sandboxing

---

## Contributing

### Code Style

- Use C# naming conventions
- Async methods end with `Async`
- Interfaces start with `I`
- Use `var` when type is obvious
- XML comments on public APIs

### Documentation

- Update README in modified folder
- Update PROGRAM_CS_README.md for changes
- Add localization keys for user text

---

## Version Information

- **.NET:** 9.0
- **DSharpPlus:** 4.4+
- **Lavalink4NET:** 4.0+
- **Last Updated:** 2026-03-05

---

## Quick Links

- **[PROGRAM_CS_README.md](PROGRAM_CS_README.md)** - Detailed Program.cs documentation
- **[Commands/README.md](Commands/README.md)** - Command architecture
- **[Service/README.md](Service/README.md)** - Service layer documentation
- **[Interface/README.md](Interface/README.md)** - Interface abstractions
- **GitHub Issues** - Report bugs or request features


