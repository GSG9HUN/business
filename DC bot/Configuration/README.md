# Configuration

This folder contains application configuration models using the Options pattern.

## Files

### BotSettings.cs

**Purpose:** Bot Discord settings.

```csharp
public sealed class BotSettings
{
    public string? Token { get; init; }
    public string Prefix { get; init; } = "!";
}
```

**Properties:**
- `Token` - Discord bot token (from environment or appsettings)
- `Prefix` - Command prefix (default: `!`)

**Usage:**
```csharp
// In Program.cs
var botSettings = configuration.GetSection("Bot").Get<BotSettings>();

// In services
var commandHandler = new CommandHandlerService(..., botSettings);
```

---

### LavalinkSettings.cs

**Purpose:** Lavalink audio server connection settings.

**Properties:**
- `Hostname` - Lavalink server host
- `Port` - Lavalink server port
- `Password` - Lavalink server password

---

### SearchResolverOptions.cs

**Purpose:** Music search resolution options.

**Properties:**
- `DefaultQueryMode` - Default search mode (YouTube, Spotify, etc.)

---

## Configuration Sources

Configuration can come from:

1. **Environment Variables** (highest priority)
   ```
   BOT__TOKEN=your_token
   BOT__PREFIX=!
   LAVALINK__HOSTNAME=localhost
   ```

2. **appsettings.json**
   ```json
   {
     "Bot": {
       "Token": "your_token",
       "Prefix": "!"
     },
     "Lavalink": {
       "Hostname": "localhost",
       "Port": 2333,
       "Password": "youshallnotpass"
     }
   }
   ```

3. **.env file** (via environment variable binding)

## Related Components

- **Program.cs** - Configuration setup
- **Service/** - Consumes configuration
- **Interface/Service/** - Configuration injection

