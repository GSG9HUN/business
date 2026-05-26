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

- `Token` - Discord bot token (from `DISCORD_TOKEN`)
- `Prefix` - Command prefix (default: `!`)

**Usage:**

```csharp
// In Startup/BotConfigurationLoader.cs
var botSettings = new BotSettings
{
    Token = GetEnv("DISCORD_TOKEN"),
    Prefix = GetEnv("BOT_PREFIX") ?? "!"
};

// In services
var commandHandler = new CommandHandlerService(..., botSettings);
```

---

### LavalinkSettings.cs

**Purpose:** Lavalink audio server connection settings.

**Properties:**

- `Hostname` - Lavalink server host
- `Port` - Lavalink server port
- `Secured` - Whether to use HTTPS/WSS for Lavalink
- `Password` - Lavalink server password

---

### SearchResolverOptions.cs

**Purpose:** Music search resolution options.

**Properties:**

- `DefaultQueryMode` - Default search mode (YouTube, Spotify, etc.)

---

## Configuration Sources

Runtime configuration currently comes from `.env` through DotNetEnv and `Environment.GetEnvironmentVariable(...)`.

1. **Bot**
   ```env
   DISCORD_TOKEN=your_token
   BOT_PREFIX=!
   ```

2. **Lavalink connection**
   ```env
   LAVALINK_HOSTNAME=lavalink
   LAVALINK_PORT=2333
   LAVALINK_SECURED=false
   LAVALINK_PASSWORD=CHANGE_ME
   ```

3. **PostgreSQL**
   ```env
   POSTGRES_HOST=postgres
   POSTGRES_PORT=5432
   POSTGRES_DB=dc_bot
   POSTGRES_USER=postgres
   POSTGRES_PASSWORD=CHANGE_ME
   ```

4. **Lavalink provider secrets**
   ```env
   SPOTIFY_CLIENT_ID=
   SPOTIFY_CLIENT_SECRET=
   APPLE_MUSIC_API_TOKEN=
   DEEZER_ARL=
   YANDEX_MUSIC_ACCESS_TOKEN=
   ```

The provider secrets are consumed by `lavalink-server/application.yaml` through Docker Compose; they are not mapped to the C# configuration classes.

## Related Components

- **Startup/BotConfigurationLoader.cs** - Configuration loading
- **Startup/BotServiceProviderFactory.cs** - Configuration injection
- **Service/** - Consumes configuration
- **Interface/Service/** - Configuration injection

