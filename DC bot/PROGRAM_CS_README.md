# Program.cs and Startup

`Program.cs` is intentionally thin. It loads `.env` with DotNetEnv when the file is present, then delegates runtime startup to `Startup/BotApplication.cs`.

## Responsibilities

### Program.cs

- locate `.env` in the current working directory
- load environment variables with `Env.NoClobber().Load(...)` when `.env` exists
- preserve already-provided process environment variables when the same key also exists in `.env`
- continue with already-provided environment variables when `.env` does not exist
- call `BotApplication.RunAsync()`
- create a shutdown `CancellationTokenSource` and cancel it on `Console.CancelKeyPress`

### Startup components

- `BotApplication.cs` - coordinates the startup flow
- `BotConfigurationLoader.cs` - reads and validates required environment values
- `BotRuntimeSettings.cs` - groups bot, Lavalink, and database startup settings
- `BotServiceProviderFactory.cs` - builds the DI container
- `Startup/DependencyInjection/*.cs` - groups DI registrations by logging, core services, Discord runtime, Lavalink, persistence, commands, and music domains
- `DatabaseMigrationRunner.cs` - applies pending EF Core migrations
- `BotHandlerRegistrar.cs` - activates command and reaction handlers after the service graph is built

## Startup Flow

1. `Program.Main()` loads `.env` when it exists.
2. `Program.Main()` calls `BotApplication.RunAsync()`.
3. `BotConfigurationLoader.LoadFromEnvironment(...)` builds `BotRuntimeSettings`.
4. `BotServiceProviderFactory.Create(...)` creates the `ServiceProvider`.
5. `DatabaseMigrationRunner.ApplyMigrationsIfNeededAsync(...)` applies pending DB migrations.
6. `BotHandlerRegistrar.RegisterHandlers(...)` activates command and reaction handlers.
7. `BotService.StartAsync()` connects the Discord client and keeps the bot running until the shutdown token is cancelled.

## Handler Registration

`BotServiceProviderFactory` calls `DiscordServiceCollectionExtensions.AddDiscordRuntime(...)`, which registers the DSharpPlus 5 gateway, message, reaction, and voice event handlers with `ConfigureEventHandlers(...)`.

`BotHandlerRegistrar` only enables handlers that maintain their own registration state:

```csharp
commandHandler.RegisterHandler(discordClient);
reactionHandler.RegisterHandler(discordClient);
```

This keeps startup ordering explicit: DSharpPlus event callbacks are registered during service composition, and command/reaction handling is enabled after the service graph is available.

## Environment Variables

Local development usually runs from the repository root, so the current working directory `.env` is normally the repository-root `.env` file. Docker Compose, CI, and production can provide the same keys directly as environment variables. Startup validates the required keys after optional `.env` loading, so the physical `.env` file is not required when the environment is already populated.

`.env` loading uses DotNetEnv `NoClobber`, so environment variables already present in the process take precedence over values from the file.

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
- `YOUTUBE_REFRESH_TOKEN`

## Example .env

```env
DISCORD_TOKEN=your_bot_token_here
BOT_PREFIX=!

# Host dotnet run/tests against docker-compose: 127.0.0.1
# Bot running inside Docker Compose network: lavalink
LAVALINK_HOSTNAME=127.0.0.1
LAVALINK_PORT=2333
LAVALINK_SECURED=false
LAVALINK_PASSWORD=your_password

# Host dotnet run/tests against docker-compose: 127.0.0.1
# Bot running inside Docker Compose network: postgres
POSTGRES_HOST=127.0.0.1
POSTGRES_PORT=5432
POSTGRES_DB=dc_bot
POSTGRES_USER=postgres
POSTGRES_PASSWORD=your_postgres_password

SPOTIFY_CLIENT_ID=
SPOTIFY_CLIENT_SECRET=
APPLE_MUSIC_API_TOKEN=
DEEZER_ARL=
YANDEX_MUSIC_ACCESS_TOKEN=
YOUTUBE_REFRESH_TOKEN=
```

## Persistence Wiring

`BotServiceProviderFactory` calls `PersistenceServiceCollectionExtensions.AddPersistenceServices(...)`, which registers:

- `AddDbContextFactory<BotDbContext>(options => options.UseNpgsql(...))`
- `IGuildDataRepository -> GuildDataRepository`
- `IPlaybackStateRepository -> PlaybackStateRepository`
- `IQueueRepository -> QueueRepository`
- `IPlaylistRepository -> PlaylistRepository`
- `IPlaylistTrackRepository -> PlaylistTrackRepository`
- `IRepeatListRepository -> RepeatListRepository`

`DatabaseMigrationRunner.ApplyMigrationsIfNeededAsync(...)` checks pending migrations and runs `MigrateAsync()` when needed.

The runtime PostgreSQL connection string is built from the `POSTGRES_*` variables by `BotConfigurationLoader.BuildPostgresConnectionString()`. The current C# startup path does not read `POSTGRES_CONNECTION_STRING` directly.

## Error Handling

Required settings are validated before the DI container is created:

- missing `DISCORD_TOKEN` prints `DISCORD_TOKEN is not set in the environment variables.`
- missing `LAVALINK_HOSTNAME` prints `LAVALINK_HOSTNAME is not set in the environment variables.`

In these cases startup exits before connecting to Discord.

## Running the Bot

```bash
dotnet restore
dotnet build
dotnet run --project "DC bot/DC bot.csproj"
```

## Related Components

- `Startup/README.md` - startup component details
- `Startup/DependencyInjection/README.md` - DI registration map
- `Configuration/BotSettings.cs` - bot configuration model
- `Configuration/LavalinkSettings.cs` - Lavalink configuration model
- `Service/BotService.cs` - bot lifecycle management
- `Service/Core/CommandHandlerService.cs` - command routing
- `Service/ReactionHandler/ReactionHandlerService.cs` - reaction handling
- `Wrapper/DiscordClientFactory.cs` - direct/test Discord client creation outside the production DI startup path
- `Wrapper/DiscordClientEventHandler.cs` - Discord lifecycle event handling
