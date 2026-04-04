# Program.cs - Application Entry Point

This file wires up runtime configuration, dependency injection, database migrations, and startup handlers.

## Responsibilities

- load `.env`
- read required bot and Lavalink settings
- configure DI container
- register repository implementations and services
- apply pending EF Core migrations
- register handlers and start the bot

## Startup Flow

1. `Main()` verifies `.env` exists, then calls `RunBotAsync()`.
2. `RunBotAsync()` reads environment values using `GetEnv(...)`.
3. `ConfigureServices(...)` builds the `ServiceProvider`.
4. `ApplyMigrationsIfNeededAsync(...)` applies pending DB migrations.
5. Handlers are registered, then `BotService.StartAsync()` starts the bot.

## Environment Variables

### Required

- `DISCORD_TOKEN`
- `LAVALINK_HOSTNAME`

### Optional Lavalink

- `LAVALINK_PORT` (default: `2333`)
- `LAVALINK_SECURED` (default: `false`)
- `LAVALINK_PASSWORD` (default: empty)

### Optional Bot

- `BOT_PREFIX` (if omitted, downstream command parsing behavior applies)

### Optional PostgreSQL

- `POSTGRES_HOST` (default: `localhost`)
- `POSTGRES_PORT` (default: `5432`)
- `POSTGRES_DB` (default: `dc_bot`)
- `POSTGRES_USER` (default: `postgres`)
- `POSTGRES_PASSWORD` (default: `postgres`)

## Example .env

```env
DISCORD_TOKEN=your_bot_token_here
BOT_PREFIX=!

LAVALINK_HOSTNAME=lavalink.example.com
LAVALINK_PORT=443
LAVALINK_SECURED=true
LAVALINK_PASSWORD=your_password

POSTGRES_HOST=localhost
POSTGRES_PORT=5432
POSTGRES_DB=dc_bot
POSTGRES_USER=postgres
POSTGRES_PASSWORD=postgres
```

## Persistence Wiring

`ConfigureServices(...)` registers:

- `AddDbContextFactory<BotDbContext>(options => options.UseNpgsql(...))`
- `IGuildDataRepository -> GuildDataRepository`
- `IPlaybackStateRepository -> PlaybackStateRepository`
- `IQueueRepository -> QueueRepository`
- `IRepeatListRepository -> RepeatListRepository`

At startup, `ApplyMigrationsIfNeededAsync(...)` executes `MigrateAsync()` when pending migrations exist.

## Slash Commands

Slash command registration code is currently present but commented out in `RegisterSlashCommands(...)`.

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

