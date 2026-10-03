# Bot Control Handlers

This folder contains API handlers for bot-control command creation and status lookup.

## Files

### BotControlCommandHandler.cs

Handles HTTP requests that enqueue commands for the bot process and retrieve command status.

## Flow

```text
MobileApp/API client
  -> command endpoint
  -> BotControlCommandHandler
  -> BotControlCommandsRepository
  -> bot worker claims command
  -> bot executes Discord/Lavalink action
  -> command result + realtime event
```

## Notes

- API handlers should not execute Lavalink operations directly.
- Join/play readiness is handled in the bot process by `PlayerConnectionService`.
