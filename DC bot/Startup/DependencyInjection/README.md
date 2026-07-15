# Dependency Injection Startup Modules

This folder contains domain-specific `IServiceCollection` extension methods used by `Startup/BotServiceProviderFactory.cs`.

## Registration Flow

`BotServiceProviderFactory.Create(...)` composes the runtime graph in this order:

```csharp
new ServiceCollection()
    .AddBotLogging()
    .AddCoreBotServices(botSettings)
    .AddDiscordRuntime(discordToken)
    .AddLavalinkRuntime(lavalinkSettings)
    .AddPersistenceServices(postgresConnectionString)
    .AddCommandServices()
    .AddMusicServices()
    .BuildServiceProvider();
```

## Files

### LoggingServiceCollectionExtensions.cs

Registers console logging with minimum `Debug` level.

### CoreServiceCollectionExtensions.cs

Registers core application services:

- `BotSettings`
- `Func<IEnumerable<ICommand>>` and `ICommandRegistry -> CommandRegistry`
- `IFileSystem -> PhysicalFileSystem`
- `IDiscordMessageFactory -> DiscordMessageWrapperFactory`
- `DiscordClientEventHandler`
- `BotService`
- reaction control components (`ReactionActionDispatcher`, `ReactionContextFactory`, `ReactionControlMessageService`, `ReactionHandlerService`)
- `CommandHandlerService`
- `IResponseBuilder -> ResponseBuilder`
- `ICommandHelper -> CommandValidationService`
- `IValidationService` and `IUserValidationService -> ValidationService`
- `ILocalizationService -> LocalizationService`

### DiscordServiceCollectionExtensions.cs

Registers the DSharpPlus client and event callbacks through DSharpPlus 5 builder APIs.

Wired events include:

- socket opened/closed
- session created/resumed/zombied
- guild available
- voice state updated
- voice server updated
- unknown gateway event
- message created
- message reaction added/removed

### LavalinkServiceCollectionExtensions.cs

Configures Lavalink4NET with HTTP/HTTPS and WS/WSS endpoints derived from `LavalinkSettings`, then registers Lavalink services.

### PersistenceServiceCollectionExtensions.cs

Registers EF Core and repository implementations:

- `IDbContextFactory<BotDbContext>` using Npgsql
- `IGuildDataRepository -> GuildDataRepository`
- `IPlaybackStateRepository -> PlaybackStateRepository`
- `IQueueRepository -> QueueRepository`
- `IPlaylistRepository -> PlaylistRepository`
- `IPlaylistTrackRepository -> PlaylistTrackRepository`
- `IRepeatListRepository -> RepeatListRepository`

### CommandServiceCollectionExtensions.cs

`AddCommandServices()` groups command registration:

- text command implementations as `ICommand`
- slash executor/context services
- slash command modules
- DSharpPlus `SlashCommandProcessor`

The internal `AddTextCommands`, `AddSlashCommandServices`, and `AddSlashCommandProcessor` helpers are private so callers use one command registration entry point.

### MusicServiceCollectionExtensions.cs

Registers the music domain services:

- repeat/current-track state
- `ITrackSerializer -> LavalinkTrackSerializer`
- track notification/formatting/playback
- player connection and Lavalink node connection
- playback event/control/request services
- track-ended handling
- queue management
- progressive timer
- saved playlist service
- search resolver

## Change Guidance

- Add new text or slash commands in `CommandServiceCollectionExtensions.cs` through `AddCommandServices()`.
- Add new music orchestration services in `MusicServiceCollectionExtensions.cs`.
- Add new saved playlist service contracts in `MusicServiceCollectionExtensions.cs` when they belong to the music domain.
- Add new repositories in `PersistenceServiceCollectionExtensions.cs`.
- Keep startup orchestration in `BotServiceProviderFactory.cs`; avoid moving runtime work into `Program.cs`.
