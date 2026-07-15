# Service

This folder contains the business logic layer of the application.

## Overview

Services implement use-cases and orchestration between command handlers, Discord/Lavalink wrappers, and persistence
repositories.

## Root Files

### BotService.cs

Starts and maintains the Discord client lifecycle. `StartAsync` accepts a `CancellationToken` so production shutdown can
stop the indefinite wait cleanly.

### LocalizationService.cs

Handles localization lookup and guild language persistence.

## Reaction Handler

`ReactionHandler/` contains message reaction handlers used for playback controls.

Main reaction components:

- `ReactionHandler/ReactionHandlerService.cs` - registers Discord reaction events and delegates playback-control reactions
- `ReactionHandler/ReactionControlMessageService.cs` - builds/sends the now-playing control message and attaches playback emojis
- `ReactionHandler/ReactionContextFactory.cs` - converts DSharpPlus reaction event payloads into Discord wrapper context
- `ReactionHandler/ReactionActionDispatcher.cs` - maps normalized control emojis to Lavalink/repeat actions
- `ReactionHandler/ReactionControlEmojis.cs` - central emoji constants and normalization

## Subfolders

### Core/

Command dispatching, command registry lookup, and validation services.

### Music/

Playback orchestration, queue behavior, repeat state, and saved playlist use-cases.

Main entry service: `LavaLinkService.cs`

Subcomponents:

- `MusicServices/` - focused music playback, queue, repeat, notification, and connection services
- `PlaylistService/` - saved playlist creation, saving, listing, viewing, renaming, deletion, and track append behavior
- `ProgressiveTimer/` - now-playing message update timer

### Presentation/

Message/embed response construction and sending.

### SlashCommands/

Slash command execution adapters that turn DSharpPlus interaction contexts into the existing text command pipeline.

Files:

- `SlashCommandExecutor.cs`

## Persistence Boundary

There is no `Service/Persistence/` folder in the current source tree. Persistence contracts and implementations are currently in:

- `../Interface/Service/Persistence/`
- `../Persistence/`

## Service Architecture

```
Commands / Slash adapters
   ↓
ICommandHelper / IValidationService
   ↓
Core Services
   ↓
Music Services / Localization Services
   ↓
ILavaLinkService / IPlaylistService / Repository Interfaces
   ↓
Lavalink / PostgreSQL / Discord
```

## Related Components

- `Interface/Service/` - service contracts
- `Interface/Service/Persistence/` - persistence contracts
- `Persistence/` - repository implementations
- `Commands/` - service consumers

