# Program.cs - Application Entry Point

This document describes the Discord bot application entry point and initialization flow.

## Overview

`Program.cs` is responsible for:
- Loading environment variables from `.env` file
- Configuring application services (Dependency Injection)
- Registering Discord event handlers
- Starting the bot

## Entry Point

```csharp
private static async Task Main()
{
    var envPath = Path.Combine(Directory.GetCurrentDirectory(), ".env");

    if (!File.Exists(envPath))
    {
        Console.WriteLine("Please provide .env file.");
        return;
    }
    
    Env.Load(envPath);
    await new Program().RunBotAsync();
}
```

**Flow:**
1. Check if `.env` file exists in current directory
2. Load environment variables from `.env`
3. Call `RunBotAsync()` to start the bot

**Required File:** `.env` in project root

---

## Environment Variables

### Required Variables

#### Bot Settings
- **DISCORD_TOKEN** - Discord bot token (required)
  ```
  DISCORD_TOKEN=your_bot_token_here
  ```

#### Lavalink Settings
- **LAVALINK_HOSTNAME** - Lavalink server host (required)
  ```
  LAVALINK_HOSTNAME=lavalinkv4.serenetia.com
  ```

### Optional Variables

#### Bot Settings
- **BOT_PREFIX** - Command prefix (default: `!`)
  ```
  BOT_PREFIX=!
  ```

#### Lavalink Settings
- **LAVALINK_PORT** - Lavalink server port (default: `2333`)
  ```
  LAVALINK_PORT=443
  ```

- **LAVALINK_SECURED** - Use HTTPS/WSS (default: `false`)
  ```
  LAVALINK_SECURED=true
  ```

- **LAVALINK_PASSWORD** - Lavalink authentication password (default: empty)
  ```
  LAVALINK_PASSWORD=https://dsc.gg/ajidevserver
  ```

### Example `.env` File

```env
DISCORD_TOKEN=your_bot_token_here
BOT_PREFIX=!
LAVALINK_HOSTNAME=lavalinkv4.serenetia.com
LAVALINK_PASSWORD=your_password
LAVALINK_PORT=443
LAVALINK_SECURED=true
```

---

## Initialization Flow

### 1. Environment Variable Loading

```csharp
static string? GetEnv(string key)
{
    var value = Environment.GetEnvironmentVariable(key);
    return string.IsNullOrWhiteSpace(value) ? null : value.Trim().Trim('"');
}

var botSettings = new BotSettings
{
    Token = GetEnv("DISCORD_TOKEN"),
    Prefix = GetEnv("BOT_PREFIX") ?? "!"
};

var lavaLinkSettings = new LavalinkSettings
{
    Hostname = GetEnv("LAVALINK_HOSTNAME"),
    Port = int.TryParse(GetEnv("LAVALINK_PORT"), out var port) ? port : 2333,
    Secured = string.Equals(GetEnv("LAVALINK_SECURED"), "true", StringComparison.OrdinalIgnoreCase),
    Password = GetEnv("LAVALINK_PASSWORD") ?? string.Empty
};
```

**Behavior:**
- Strips whitespace and quotes from values
- Provides sensible defaults
- Validates required settings

### 2. Service Registration (Dependency Injection)

```csharp
var services = ConfigureServices(botSettings, lavaLinkSettings);
```

The `ConfigureServices()` method configures the DI container with:

#### Logging
```csharp
.AddLogging(builder => { 
    builder.AddConsole().SetMinimumLevel(LogLevel.Debug); 
})
```
- Console logging enabled
- Minimum level: Debug

#### Lavalink Configuration
```csharp
.ConfigureLavalink(options =>
{
    var httpScheme = lavaLinkSettings.Secured ? "https" : "http";
    var wsScheme = lavaLinkSettings.Secured ? "wss" : "ws";
    var baseAddress = new Uri($"{httpScheme}://{lavaLinkSettings.Hostname}:{lavaLinkSettings.Port}");
    var webSocketUri = new Uri($"{wsScheme}://{lavaLinkSettings.Hostname}:{lavaLinkSettings.Port}/v4/websocket");
    
    options.BaseAddress = baseAddress;
    options.WebSocketUri = webSocketUri;
    options.Passphrase = lavaLinkSettings.Password;
})
.AddLavalink()
```

**Features:**
- Automatically selects HTTP/HTTPS based on `LAVALINK_SECURED`
- Automatically selects WS/WSS based on `LAVALINK_SECURED`
- Configures Lavalink server address and authentication

#### Core Services Registration

| Service | Implementation | Lifetime |
|---------|----------------|----------|
| `IFileSystem` | `PhysicalFileSystem` | Singleton |
| `BotService` | `BotService` | Singleton |
| `DiscordClient` | `DiscordClient` | Singleton |
| `CommandHandlerService` | `CommandHandlerService` | Singleton |
| `ReactionHandler` | `ReactionHandler` | Singleton |

#### Command Registration

All text commands registered as `ICommand`:
- `TagCommand`
- `JoinCommand`
- `PingCommand`
- `HelpCommand`
- `PlayCommand`
- `SkipCommand`
- `ClearCommand`
- `LeaveCommand`
- `PauseCommand`
- `ResumeCommand`
- `RepeatCommand`
- `ShuffleCommand`
- `LanguageCommand`
- `ViewQueueCommand`
- `RepeatListCommand`

#### Music Services Registration

| Service | Implementation |
|---------|----------------|
| `ILavaLinkService` | `LavaLinkService` |
| `IMusicQueueService` | `MusicQueueService` |
| `IRepeatService` | `RepeatService` |
| `ICurrentTrackService` | `CurrentTrackService` |
| `ITrackNotificationService` | `TrackNotificationService` |
| `ITrackFormatterService` | `TrackFormatterService` |
| `IPlayerConnectionService` | `PlayerConnectionService` |
| `IPlaybackEventHandlerService` | `PlaybackEventHandlerService` |
| `ITrackPlaybackService` | `TrackPlaybackService` |
| `ITrackEndedHandlerService` | `TrackEndedHandlerService` |
| `ITrackSearchResolverService` | `TrackSearchResolverService` |

#### Validation & Localization Services

| Service | Implementation |
|---------|----------------|
| `IValidationService` | `ValidationService` |
| `IUserValidationService` | `ValidationService` |
| `ILocalizationService` | `LocalizationService` |
| `IResponseBuilder` | `ResponseBuilder` |
| `ICommandHelper` | `CommandValidationService` |

### 3. Handler Registration

```csharp
RegisterHandlers(services);
```

**Handlers Registered:**

```csharp
commandHandler.RegisterHandler(discordClient);  // Message handling
reactionHandler.RegisterHandler(discordClient);  // Reaction handling
```

**Features:**
- `CommandHandlerService` - Routes Discord messages to registered commands
- `ReactionHandler` - Handles music control reactions

### 4. Bot Startup

```csharp
var botService = services.GetRequiredService<BotService>();
await botService.StartAsync();
```

**Flow:**
1. Retrieve `BotService` from DI container
2. Call `StartAsync()` to:
   - Connect Discord client
   - Run bot indefinitely

---

## Service Dependency Graph

```
Program.cs
    ↓
Configure Services (DI Container)
    ↓
├─ Logging Configuration
├─ Lavalink Configuration
├─ Core Services
│  ├─ DiscordClient
│  ├─ BotService
│  ├─ CommandHandlerService
│  └─ ReactionHandler
├─ Command Registration (all text commands)
├─ Music Services
│  ├─ LavaLinkService
│  ├─ MusicQueueService
│  └─ Specialized music services
└─ Validation & Localization
    ├─ ValidationService
    ├─ LocalizationService
    └─ ResponseBuilder
    ↓
Register Handlers
    ├─ CommandHandlerService.RegisterHandler()
    └─ ReactionHandler.RegisterHandler()
    ↓
BotService.StartAsync()
    ↓
Running Bot
```

---

## Slash Commands (Currently Disabled)

Slash commands registration is present but commented out:

```csharp
/*
var slashCommandsConfig = discordClient.UseSlashCommands(new SlashCommandsConfiguration
{
    Services = services
});
slashCommandsConfig.RefreshCommands();
slashCommandsConfig.RegisterCommands<TagSlashCommand>();
slashCommandsConfig.RegisterCommands<PingSlashCommand>();
slashCommandsConfig.RegisterCommands<HelpSlashCommand>();
slashCommandsConfig.RegisterCommands<PlaySlashCommand>();
*/
```

**To Enable:**
1. Uncomment the code
2. Discord bot must have `applications.commands` scope
3. Slash commands will be registered on bot startup

---

## Error Handling

The application validates required settings and exits gracefully on errors:

```csharp
if (string.IsNullOrWhiteSpace(botSettings.Token))
{
    Console.WriteLine("DISCORD_TOKEN is not set in the environment variables.");
    return;
}

if (string.IsNullOrWhiteSpace(lavalinkHost))
{
    Console.WriteLine("LAVALINK_HOSTNAME is not set in the environment variables.");
    return;
}
```

**Missing Required Variables:**
- Application prints error message
- Exits without starting bot

---

## Running the Bot

### Prerequisites

1. **Create `.env` file** in project root with required variables
2. **.NET 9.0 SDK** installed
3. **Lavalink server** running and accessible

### Build and Run

```bash
# Restore dependencies
dotnet restore

# Build project
dotnet build

# Run bot
dotnet run
```

### Deployment

```bash
# Publish for production
dotnet publish -c Release

# Run published executable
./DC bot.exe
```

---

## Configuration Loading Order

1. **Default Values** (hardcoded in code)
   - `Prefix = "!"`
   - `Port = 2333`
   - `Secured = false`
   - `Password = ""`

2. **Environment Variables** (override defaults)
   - `BOT_PREFIX`
   - `LAVALINK_PORT`
   - `LAVALINK_SECURED`
   - `LAVALINK_PASSWORD`

3. **Validation** (check required values)
   - `DISCORD_TOKEN` - Required, exit if missing
   - `LAVALINK_HOSTNAME` - Required, exit if missing

---

## Related Components

- **Configuration/BotSettings.cs** - Bot configuration model
- **Configuration/LavalinkSettings.cs** - Lavalink configuration model
- **Service/BotService.cs** - Bot lifecycle management
- **Service/Core/CommandHandlerService.cs** - Command routing
- **Service/ReactionHandler.cs** - Reaction handling
- **Wrapper/DiscordClientFactory.cs** - Discord client creation
- **Interface/** - Service contracts
- **Commands/** - Registered text commands
- **Service/Music/** - Music playback services

---

## Troubleshooting

### "Please provide .env file."
**Cause:** `.env` file not found in project root

**Solution:** Create `.env` file in project root with required variables

### "DISCORD_TOKEN is not set"
**Cause:** `DISCORD_TOKEN` environment variable missing or empty

**Solution:** Add `DISCORD_TOKEN=your_token` to `.env` file

### "LAVALINK_HOSTNAME is not set"
**Cause:** `LAVALINK_HOSTNAME` environment variable missing or empty

**Solution:** Add `LAVALINK_HOSTNAME=your_host` to `.env` file

### Bot connects but commands don't work
**Cause:** Command prefix doesn't match `!`

**Solution:** Check `BOT_PREFIX` in `.env` or use correct prefix

### Lavalink connection fails
**Cause:** Wrong hostname, port, password, or Lavalink server offline

**Solution:**
- Verify `LAVALINK_HOSTNAME`, `LAVALINK_PORT`, `LAVALINK_PASSWORD`
- Check Lavalink server is running and accessible
- Verify `LAVALINK_SECURED` matches server configuration

