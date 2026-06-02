# Service

This folder contains the business logic layer of the application.

## Overview

Services implement use-cases and orchestration between command handlers, Discord/Lavalink wrappers, and persistence
repositories.

## Root Files

### BotService.cs

Starts and maintains the Discord client lifecycle.

### LocalizationService.cs

Handles localization lookup and guild language persistence.

### ReactionHandler.cs

Registers message reaction handlers used for playback controls.

## Subfolders

### Core/

Command dispatching and validation services.

### Music/

Playback orchestration and queue behavior.

Main entry service: `LavaLinkService.cs`

Subcomponents:

- `MusicServices/` - focused music playback, queue, repeat, notification, and connection services
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
ILavaLinkService / Repository Interfaces
   ↓
Lavalink / PostgreSQL / Discord
```

## Related Components

- `Interface/Service/` - service contracts
- `Interface/Service/Persistence/` - persistence contracts
- `Persistence/` - repository implementations
- `Commands/` - service consumers

