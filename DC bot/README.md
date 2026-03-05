# DC Bot - Discord Music Bot

A feature-rich Discord music bot built with DSharpPlus and Lavalink4NET, featuring queue management, multiple languages, and robust error handling.

## Project Structure

```
DC bot/
├── Commands/              # Text and slash command implementations
│   ├── SlashCommands/     # Modern Discord slash commands
│   └── README.md
├── Configuration/         # Configuration models (Options pattern)
│   └── README.md
├── Constants/             # Application-wide constants and localization keys
│   └── README.md
├── Exceptions/            # Custom exception types
│   └── README.md
├── Helper/                # Utility classes and result types
│   └── README.md
├── Interface/             # Service and wrapper contracts (abstractions)
│   └── README.md
├── IO/                    # File system abstraction layer
│   └── README.md
├── Logging/               # Structured logging extensions
│   ├── EventIdTable.md
│   └── README.md
├── Service/               # Business logic layer
│   ├── MusicServices/     # Music-specific services
│   │   └── README.md
│   └── README.md
├── Wrapper/               # Discord API wrappers (DSharpPlus abstraction)
│   └── README.md
├── localization/          # Language files (eng.json, hu.json)
├── guildFiles/            # Per-guild persistent data
├── Program.cs             # Application entry point
└── DC bot.csproj          # Project file

```

## Key Features

### 🎵 Music Playback
- Play from YouTube, Spotify, SoundCloud
- Queue management with persistence
- Repeat modes (single track, playlist)
- Pause/Resume/Skip controls
- Voice channel state management

### 🌍 Multi-Language Support
- English (default)
- Hungarian
- Per-guild language selection
- Extensible localization system

### 🎯 Command System
- Text commands with `!` prefix
- Modern slash commands with auto-complete
- Comprehensive validation
- Consistent error handling

### 🔧 Architecture
- Clean architecture (Commands → Services → External APIs)
- Dependency injection throughout
- Interface-based design (testable, flexible)
- Wrapper pattern for external libraries
- Repository pattern for data access

### 📊 Observability
- Structured logging with event IDs
- Command execution tracking
- Error logging with context
- Performance monitoring ready

## Getting Started

### Prerequisites
- .NET 9.0 SDK
- Lavalink server (audio processing)
- Discord bot token

### Configuration

Create `.env` file:
```env
BOT__TOKEN=your_discord_bot_token
BOT__PREFIX=!
LAVALINK__HOSTNAME=localhost
LAVALINK__PORT=2333
LAVALINK__PASSWORD=youshallnotpass
```

Or use `appsettings.json`:
```json
{
  "Bot": {
    "Token": "your_token_here",
    "Prefix": "!"
  },
  "Lavalink": {
    "Hostname": "localhost",
    "Port": 2333,
    "Password": "youshallnotpass"
  }
}
```

### Running the Bot

```bash
# Restore dependencies
dotnet restore

# Build
dotnet build

# Run
dotnet run
```

### Running Lavalink

```bash
# Download Lavalink.jar
# Create application.yml config
# Run
java -jar Lavalink.jar
```

## Architecture Overview

### Layer Diagram
```
┌─────────────────────────────────────┐
│          Discord (User)             │
└──────────────┬──────────────────────┘
               │
┌──────────────▼──────────────────────┐
│     Commands (Presentation)         │
│  - PlayCommand                      │
│  - PauseCommand                     │
│  - Slash commands                   │
└──────────────┬──────────────────────┘
               │
┌──────────────▼──────────────────────┐
│       Services (Business Logic)     │
│  - LavaLinkService                  │
│  - MusicQueueService                │
│  - ValidationService                │
└──────────────┬──────────────────────┘
               │
┌──────────────▼──────────────────────┐
│    External APIs (Infrastructure)   │
│  - Discord (DSharpPlus)             │
│  - Lavalink (Audio)                 │
│  - File System                      │
└─────────────────────────────────────┘
```

### Design Patterns

- **Dependency Injection** - Constructor injection throughout
- **Service Layer** - Business logic isolated from presentation
- **Repository** - Queue persistence abstraction
- **Wrapper/Adapter** - DSharpPlus abstraction
- **Factory** - Discord client and wrapper creation
- **Options** - Strongly-typed configuration
- **Result** - Validation results instead of exceptions

### Key Principles (SOLID)

- ✅ **Single Responsibility** - Each class has one job
- ✅ **Open/Closed** - Extend via interfaces, not modification
- ✅ **Liskov Substitution** - Interfaces are substitutable
- ✅ **Interface Segregation** - Small, focused interfaces
- ✅ **Dependency Inversion** - Depend on abstractions

## Testing

```bash
# Run all tests
dotnet test

# Run with coverage
dotnet test --collect:"XPlat Code Coverage"
```

Test structure:
```
DC bot tests/
├── UnitTests/
│   ├── CommandTests/
│   ├── ServiceTests/
│   └── ValidationTests/
└── IntegrationTests/
    └── ServiceTests/
```

## Documentation

Each folder has a `README.md` with:
- Purpose and responsibilities
- Key components
- Usage examples
- Design patterns
- Best practices
- Related folders

Start here:
- **Commands/README.md** - Command implementation guide
- **Service/README.md** - Business logic layer
- **Interface/README.md** - Understanding interfaces
- **Wrapper/README.md** - Discord API abstraction

## Contributing

### Code Style
- Use C# naming conventions (PascalCase for types, camelCase for locals)
- Async methods end with `Async`
- Interfaces start with `I`
- Use `var` when type is obvious
- XML comments for public APIs

### Adding a New Command

1. Create `XCommand.cs` in `Commands/`
2. Implement `ICommand` interface
3. Add to DI container in `Program.cs`
4. Add localization keys to `Constants/AppConstants.cs`
5. Add translations to `localization/*.json`
6. Write unit tests

Example:
```csharp
public class MyCommand(
    IMyService service,
    ILogger<MyCommand> logger) : ICommand
{
    public string Name => "mycommand";
    public string Description => "Does something cool";

    public async Task ExecuteAsync(IDiscordMessage message)
    {
        logger.CommandInvoked(Name);
        
        // Validation
        // Business logic
        // Response
        
        logger.CommandExecuted(Name);
    }
}
```

### Adding a New Service

1. Create interface in `Interface/IMyService.cs`
2. Implement in `Service/MyService.cs`
3. Register in `Program.cs`:
   ```csharp
   services.AddSingleton<IMyService, MyService>();
   ```
4. Inject where needed
5. Write unit tests with mocks

## Common Tasks

### Adding a New Language

1. Create `localization/xx.json` (e.g., `es.json` for Spanish)
2. Copy structure from `eng.json`
3. Translate all values
4. Test with `/language xx` command

### Adding a New Validation

1. Add result type in `Helper/` (e.g., `MyValidationResult.cs`)
2. Add method to `ValidationService.cs`
3. Use in commands via `CommandValidationHelper`

### Adding Event Handling

1. Create handler in `Service/` (e.g., `MyEventHandler.cs`)
2. Register in `BotService.cs`
3. Implement `RegisterHandler()` and `UnregisterHandler()`
4. Clean up on shutdown

## Troubleshooting

### Bot doesn't respond
- Check token in `.env` or `appsettings.json`
- Verify bot has `MESSAGE_CONTENT` intent enabled in Discord Developer Portal
- Check logs for connection errors

### Music doesn't play
- Ensure Lavalink is running
- Check Lavalink connection in logs
- Verify bot is in voice channel
- Check Lavalink password matches configuration

### Commands not found
- Verify command registration in `Program.cs`
- Check command name matches (case-sensitive)
- Ensure prefix is correct (`!` by default)

### Queue not persisting
- Check `guildFiles/queues/` directory exists
- Verify file system permissions
- Check logs for serialization errors

## Performance Considerations

- **Memory:** ~50-100MB idle, +20MB per active guild
- **CPU:** Minimal when idle, ~5% per playing guild
- **Network:** Depends on music streaming (handled by Lavalink)
- **Disk I/O:** Queue persistence on track changes

## Security

- ✅ Bot token in environment variables (not source control)
- ✅ Input validation on all commands
- ✅ Rate limiting via Discord API
- ✅ No sensitive data in logs
- ✅ File system sandboxing (`guildFiles/`)

## License

[Your License Here]

## Support

- **Issues:** GitHub Issues
- **Discord:** [Your Discord Server]
- **Documentation:** See folder READMEs

## Credits

- **DSharpPlus** - Discord API library
- **Lavalink4NET** - Audio streaming
- **ASP.NET Core** - Dependency injection and configuration

---

**Ready to dive deeper?** Start with:
1. `Commands/README.md` - Understand commands
2. `Service/README.md` - Business logic layer
3. `Interface/README.md` - Architecture abstractions
4. `Logging/README.md` - Observability

