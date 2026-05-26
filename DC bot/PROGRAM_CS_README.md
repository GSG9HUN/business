# Program.cs and Startup

`Program.cs` is intentionally thin. It verifies that the `.env` file exists, loads it with DotNetEnv, and delegates runtime startup to `Startup/BotApplication.cs`.

## Responsibilities

### Program.cs

- locate `.env` in the current working directory
- print `Please provide .env file.` and exit when the file is missing
- load environment variables with `Env.Load(...)`
- call `BotApplication.RunAsync()`

### Startup components

- `BotApplication.cs` - coordinates the startup flow
- `BotConfigurationLoader.cs` - reads and validates required environment values
- `BotRuntimeSettings.cs` - groups bot, Lavalink, and database startup settings
- `BotServiceProviderFactory.cs` - builds the DI container
- `DatabaseMigrationRunner.cs` - applies pending EF Core migrations
- `BotHandlerRegistrar.cs` - wires Discord client events, command handling, and reaction handling

## Startup Flow

1. `Program.Main()` checks for `.env`.
2. `Program.Main()` loads `.env` and calls `BotApplication.RunAsync()`.
3. `BotConfigurationLoader.LoadFromEnvironment(...)` builds `BotRuntimeSettings`.
4. `BotServiceProviderFactory.Create(...)` creates the `ServiceProvider`.
5. `DatabaseMigrationRunner.ApplyMigrationsIfNeededAsync(...)` applies pending DB migrations.
6. `BotHandlerRegistrar.RegisterHandlers(...)` wires runtime event handlers.
7. `BotService.StartAsync()` connects the Discord client and keeps the bot running.

## Handler Registration

`DiscordClientFactory` only creates a configured `DiscordClient`.

`BotHandlerRegistrar` owns event subscription:

```csharp
discordClient.Ready += eventHandler.OnClientReady;
discordClient.GuildAvailable += eventHandler.OnGuildAvailable;
commandHandler.RegisterHandler(discordClient);
reactionHandler.RegisterHandler(discordClient);
```

This keeps client construction independent from `DiscordClientEventHandler`, which now receives its dependencies directly through constructor injection.

## Environment Variables

### Required

- `DISCORD_TOKEN`
- `LAVALINK_HOSTNAME`

### Optional Bot

- `BOT_PREFIX` (default: `!`)

### Optional Lavalink

- `LAVALINK_PORT` (default: `2333`)
- `LAVALINK_SECURED` (default: `false`)
- `LAVALINK_PASSWORD` (default: empty)

### Optional PostgreSQL

- `POSTGRES_HOST` (default: `localhost`)
- `POSTGRES_PORT` (default: `5432`)
- `POSTGRES_DB` (default: `dc_bot`)
- `POSTGRES_USER` (default: `postgres`)
- `POSTGRES_PASSWORD` (default: `postgres`)

### Optional Lavalink Provider Settings

These are consumed by `lavalink-server/application.yaml` through Docker Compose, not directly by the C# startup classes:

- `SPOTIFY_CLIENT_ID`
- `SPOTIFY_CLIENT_SECRET`
- `APPLE_MUSIC_API_TOKEN`
- `DEEZER_ARL`
- `YANDEX_MUSIC_ACCESS_TOKEN`

## Example .env

```env
DISCORD_TOKEN=your_bot_token_here
BOT_PREFIX=!

LAVALINK_HOSTNAME=lavalink
LAVALINK_PORT=2333
LAVALINK_SECURED=false
LAVALINK_PASSWORD=your_password

POSTGRES_HOST=postgres
POSTGRES_PORT=5432
POSTGRES_DB=dc_bot
POSTGRES_USER=postgres
POSTGRES_PASSWORD=your_postgres_password

SPOTIFY_CLIENT_ID=
SPOTIFY_CLIENT_SECRET=
APPLE_MUSIC_API_TOKEN=
DEEZER_ARL=
YANDEX_MUSIC_ACCESS_TOKEN=
```

## Persistence Wiring

`BotServiceProviderFactory` registers:

- `AddDbContextFactory<BotDbContext>(options => options.UseNpgsql(...))`
- `IGuildDataRepository -> GuildDataRepository`
- `IPlaybackStateRepository -> PlaybackStateRepository`
- `IQueueRepository -> QueueRepository`
- `IRepeatListRepository -> RepeatListRepository`

`DatabaseMigrationRunner.ApplyMigrationsIfNeededAsync(...)` checks pending migrations and runs `MigrateAsync()` when needed.

## Error Handling

Required settings are validated before the DI container is created:

- missing `DISCORD_TOKEN` prints `DISCORD_TOKEN is not set in the environment variables.`
- missing `LAVALINK_HOSTNAME` prints `LAVALINK_HOSTNAME is not set in the environment variables.`
- missing `.env` prints `Please provide .env file.`

In these cases startup exits before connecting to Discord.

## Running the Bot

```bash
dotnet restore
dotnet build
dotnet run --project "DC bot/DC bot.csproj"
```

## Related Components

- `Startup/README.md` - startup component details
- `Configuration/BotSettings.cs` - bot configuration model
- `Configuration/LavalinkSettings.cs` - Lavalink configuration model
- `Service/BotService.cs` - bot lifecycle management
- `Service/Core/CommandHandlerService.cs` - command routing
- `Service/ReactionHandler.cs` - reaction handling
- `Wrapper/DiscordClientFactory.cs` - Discord client creation
- `Wrapper/DiscordClientEventHandler.cs` - Discord lifecycle event handling
