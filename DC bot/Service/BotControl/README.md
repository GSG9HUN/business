# Bot Control Services

This folder contains bot-side execution services for commands submitted through the API.

## Purpose

Mobile/API clients should not execute Discord or Lavalink operations directly. Instead, API handlers persist bot-control command rows. The bot process claims those rows and executes them here, using the same service layer as Discord commands.

## Files

### BotControlWorker.cs

Background worker that claims pending commands and coordinates execution.

### BotControlCommandDispatcher.cs

Maps command types such as pause, resume, skip, previous, leave, repeat, repeat-list, queue enqueue, clear, shuffle, and playlist-related actions to service calls.

### BotControlContextResolver.cs

Resolves Discord guild, text channel, voice channel, and member context from persisted command metadata.

### BotControlDiscordResponseSink.cs

Sends Discord-facing responses for bot-control commands when a response channel exists.

### BotControlResultFactory.cs

Builds persisted result payloads so API realtime clients can observe command completion.

### Discord Context Wrappers

`BotControlDiscordChannelWrapper`, `BotControlDiscordMemberWrapper`, `BotControlDiscordMessageWrapper`, and `BotControlDiscordVoiceStateWrapper` adapt resolved Discord context to the bot's wrapper interfaces.

## Playback Notes

- Playback commands still flow through music services such as `LavaLinkService`, `PlaybackControlService`, and `PlayerConnectionService`.
- Join/play readiness behavior is owned by `PlayerConnectionService` and `PlayerConnectionRetryPolicy`; bot-control dispatchers should not duplicate Lavalink validation logic.

## Related Components

- `Interface/Service/BotControl/`
- `DC bot.Persistence/Repositories/BotControl/`
- `API/Handlers/BotControl/`
- `API/Realtime/Events/BotControlCommandEvent.cs`
