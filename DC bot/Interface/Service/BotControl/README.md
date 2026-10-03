# Bot Control Interfaces

This folder contains interfaces for API-originated bot-control command execution.

## Purpose

The API records bot-control commands in persistence. The bot process later claims and executes those commands against Discord/Lavalink. These interfaces define that bot-side execution boundary.

## Files

### IBotControlWorker.cs

Background worker contract for polling/claiming pending bot-control commands.

### IBotControlCommandDispatcher.cs

Dispatches a claimed command to the correct execution path.

### IBotControlContextResolver.cs

Resolves Discord guild, text channel, voice channel, and member context needed by a command.

### IBotControlDiscordResponseSink.cs

Sends command responses back to Discord when a text channel can be resolved.

### IBotControlResultFactory.cs

Builds normalized command result payloads for persistence/API realtime updates.

## Related Components

- `Service/BotControl/` - implementations
- `Interface/Service/BotControl/Models/` - execution context/result records
- `DC bot.Persistence/Repositories/BotControl/` - command persistence
- `API/Handlers/BotControl/` - HTTP command creation/status handlers
