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

### Presentation/

Message/embed response construction and sending.

### Persistence/

Reserved for service-level persistence orchestration. Persistence contracts and implementations are currently in:

- `../Interface/Service/Persistence/`
- `../Persistence/`

## Service Architecture

```
Commands
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

